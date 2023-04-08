using EasyOC.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using System.Security.Claims;
using IWorkflowManager = OrchardCore.Workflows.Services.IWorkflowManager;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace EasyOC.Users.Controllers
{
    [Authorize]
    public class EocAccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly IScriptingManager _scriptingManager;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly INotifier _notifier;
        private readonly IClock _clock;
        private readonly IDistributedCache _distributedCache;
        private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;

        public EocAccountController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<EocAccountController> logger,
            ISiteService siteService,
            IHtmlLocalizer<AccountController> htmlLocalizer,
            IStringLocalizer<AccountController> stringLocalizer,
            IEnumerable<ILoginFormEvent> accountEvents,
            IScriptingManager scriptingManager,
            INotifier notifier,
            IClock clock,
            IDistributedCache distributedCache,
            IDataProtectionProvider dataProtectionProvider,
            IEnumerable<IExternalLoginEventHandler> externalLoginHandlers)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;
            _accountEvents = accountEvents;
            _scriptingManager = scriptingManager;
            _notifier = notifier;
            _clock = clock;
            _distributedCache = distributedCache;
            _dataProtectionProvider = dataProtectionProvider;
            _externalLoginHandlers = externalLoginHandlers;
            H = htmlLocalizer;
            S = stringLocalizer;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Unable to load user with ID '{UserId}'.", _userManager.GetUserId(User));
                return RedirectToAction(nameof(AccountController.Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Unexpected error occurred loading external login info for user '{UserName}'.", user.UserName);
                return RedirectToAction(nameof(AccountController.Login));
            }

            var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            if (!result.Succeeded)
            {
                _logger.LogError("Unexpected error occurred adding external login info for user '{UserName}'.", user.UserName);
                return RedirectToAction(nameof(AccountController.Login));
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Perform External Login SignIn
            await ExternalLoginSignInAsync(user, info);
            //StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            //model.StatusMessage = StatusMessage;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model, string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                _logger.LogWarning("Error loading external login info.");

                return NotFound();
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Suspicious login detected from external provider. {provider} with key [{providerKey}] for {identity}",
                    info.LoginProvider, info.ProviderKey, info.Principal?.Identity?.Name);

                return RedirectToAction(nameof(AccountController.Login));
            }

            await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!signInResult.Succeeded)
            {
                user = null;
                ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
            }
            else
            {
                var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (identityResult.Succeeded)
                {
                    _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);
                    // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                    // the user is not allowed to login
                    if ((await ExternalLoginSignInAsync(user, info)).Succeeded)
                    {
                        return await LoggedInActionResult(user, returnUrl, info);
                    }
                }
                AddIdentityErrors(identityResult);
            }

            return RedirectToAction(nameof(AccountController.Login));
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                ModelState.AddModelError("", S["An error occurred in external provider."]);
                return RedirectToLogin(returnUrl);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Could not get external login info.");
                ModelState.AddModelError("", S["An error occurred in external provider."]);
                return RedirectToLogin(returnUrl);
            }

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var iUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (iUser != null)
            {
                if (!await AddConfirmEmailError(iUser) && !AddUserEnabledError(iUser))
                {
                    await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), iUser, ModelState, _logger);

                    var signInResult = await ExternalLoginSignInAsync(iUser, info);
                    if (signInResult.Succeeded)
                    {
                        return await LoggedInActionResult(iUser, returnUrl, info);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
                    }
                }
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

                if (!string.IsNullOrWhiteSpace(email))
                {
                    iUser = await _userManager.FindByEmailAsync(email);
                }

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                if (iUser != null)
                {
                    if (iUser is User userToLink && registrationSettings.UsersMustValidateEmail && !userToLink.EmailConfirmed)
                    {
                        return RedirectToAction("ConfirmEmailSent", new { Area = "OrchardCore.Users", Controller = "Registration", ReturnUrl = returnUrl });
                    }

                    // Link external login to an existing user
                    ViewData["UserName"] = iUser.UserName;
                    ViewData["Email"] = email;

                    return View(nameof(LinkExternalLogin));
                }
                else
                {
                    // no user could be matched, check if a new user can register
                    if (registrationSettings.UsersCanRegister == UserRegistrationType.NoRegistration)
                    {
                        string message = S["Site does not allow user registration."];
                        _logger.LogWarning(message);
                        ModelState.AddModelError("", message);
                    }
                    else
                    {
                        var externalLoginViewModel = new RegisterExternalLoginViewModel();

                        externalLoginViewModel.NoPassword = registrationSettings.NoPasswordForExternalUsers;
                        externalLoginViewModel.NoEmail = registrationSettings.NoEmailForExternalUsers;
                        externalLoginViewModel.NoUsername = registrationSettings.NoUsernameForExternalUsers;

                        // If registrationSettings.NoUsernameForExternalUsers is true, this username will not be used
                        externalLoginViewModel.UserName = await GenerateUsername(info);
                        externalLoginViewModel.Email = email;

                        // The user doesn't exist, if no information required, we can create the account locally
                        // instead of redirecting to the ExternalLogin
                        var noInformationRequired = externalLoginViewModel.NoPassword
                                                        && externalLoginViewModel.NoEmail
                                                        && externalLoginViewModel.NoUsername;

                        if (noInformationRequired)
                        {
                            iUser = await this.RegisterUser(new RegisterViewModel()
                            {
                                UserName = externalLoginViewModel.UserName,
                                Email = externalLoginViewModel.Email,
                                Password = null,
                                ConfirmPassword = null
                            }, S["Confirm your account"], _logger);

                            // If the registration was successful we can link the external provider and redirect the user
                            if (iUser != null)
                            {
                                var identityResult = await _signInManager.UserManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                                if (identityResult.Succeeded)
                                {
                                    _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                                    // The login info must be linked before we consider a redirect, or the login info is lost.
                                    if (iUser is User user)
                                    {

                                        if (registrationSettings.UsersMustValidateEmail && !user.EmailConfirmed)
                                        {
                                            return RedirectToAction("ConfirmEmailSent", new { Area = "OrchardCore.Users", Controller = "Registration", ReturnUrl = returnUrl });
                                        }

                                        if (registrationSettings.UsersAreModerated && !user.IsEnabled)
                                        {
                                            return RedirectToAction("RegistrationPending", new { Area = "OrchardCore.Users", Controller = "Registration", ReturnUrl = returnUrl });
                                        }
                                    }

                                    // We have created/linked to the local user, so we must verify the login.
                                    // If it does not succeed, the user is not allowed to login
                                    var signInResult = await ExternalLoginSignInAsync(iUser, info);
                                    if (signInResult.Succeeded)
                                    {
                                        return await LoggedInActionResult(iUser, returnUrl, info);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
                                        return RedirectToLogin(returnUrl);
                                    }
                                }
                                AddIdentityErrors(identityResult);
                            }
                        }

                        return View("RegisterExternalLogin", externalLoginViewModel);
                    }
                }
            }

            return RedirectToLogin(returnUrl);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExternalLogin(RegisterExternalLoginViewModel model, string returnUrl = null)
        {
            IUser iUser = null;
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                _logger.LogWarning("Error loading external login info.");
                return NotFound();
            }

            if (settings.UsersCanRegister == UserRegistrationType.NoRegistration)
            {
                _logger.LogWarning("Site does not allow user registration.");
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            model.NoPassword = settings.NoPasswordForExternalUsers;
            model.NoEmail = settings.NoEmailForExternalUsers;
            model.NoUsername = settings.NoUsernameForExternalUsers;

            ModelState.Clear();

            if (model.NoEmail && String.IsNullOrWhiteSpace(model.Email))
            {
                model.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
            }

            if (model.NoUsername && String.IsNullOrWhiteSpace(model.UserName))
            {
                model.UserName = await GenerateUsername(info);
            }

            if (model.NoPassword)
            {
                model.Password = null;
                model.ConfirmPassword = null;
            }

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                iUser = await this.RegisterUser(new RegisterViewModel() { UserName = model.UserName, Email = model.Email, Password = model.Password, ConfirmPassword = model.ConfirmPassword }, S["Confirm your account"], _logger);
                if (iUser is null)
                {
                    ModelState.AddModelError(string.Empty, "Registration Failed.");
                }
                else
                {
                    var identityResult = await _signInManager.UserManager.AddLoginAsync(iUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation(3, "User account linked to {provider} provider.", info.LoginProvider);

                        // The login info must be linked before we consider a redirect, or the login info is lost.
                        if (iUser is User user)
                        {
                            if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                            {
                                return RedirectToAction("ConfirmEmailSent", new { Area = "OrchardCore.Users", Controller = "Registration", ReturnUrl = returnUrl });
                            }

                            if (settings.UsersAreModerated && !user.IsEnabled)
                            {
                                return RedirectToAction("RegistrationPending", new { Area = "OrchardCore.Users", Controller = "Registration", ReturnUrl = returnUrl });
                            }
                        }

                        // we have created/linked to the local user, so we must verify the login. If it does not succeed,
                        // the user is not allowed to login
                        var signInResult = await ExternalLoginSignInAsync(iUser, info, true);
                        if (signInResult.Succeeded)
                        {
                            return await LoggedInActionResult(iUser, returnUrl, info);
                        }
                    }
                    AddIdentityErrors(identityResult);
                }
            }
            return View("RegisterExternalLogin", model);
        }

        private async Task<IActionResult> LoggedInActionResult(IUser user, string returnUrl = null, ExternalLoginInfo info = null)
        {
            var workflowManager = HttpContext.RequestServices.GetService<IWorkflowManager>();
            if (workflowManager != null)
            {
                var input = new Dictionary<string, object>();
                input["UserName"] = user.UserName;
                input["ExternalClaims"] = info == null ? Enumerable.Empty<SerializableClaim>() : info.Principal.GetSerializableClaims();
                input["Roles"] = ((User)user).RoleNames;
                input["Provider"] = info?.LoginProvider;
                await workflowManager.TriggerEventAsync(nameof(OrchardCore.Users.Workflows.Activities.UserLoggedInEvent),
                    input: input, correlationId: ((User)user).UserId);
            }

            return RedirectToLocal(returnUrl);
        }
        private async Task<string> GenerateUsername(ExternalLoginInfo info)
        {
            var ret = string.Concat("u", IdGenerator.GenerateId());
            var externalClaims = info?.Principal.GetSerializableClaims();
            var userNames = new Dictionary<Type, string>();

            foreach (var item in _externalLoginHandlers)
            {
                try
                {
                    var userName = await item.GenerateUserName(info.LoginProvider, externalClaims.ToArray());
                    if (!String.IsNullOrWhiteSpace(userName))
                    {
                        // set the return value to the username generated by the first IExternalLoginHandler
                        if (userNames.Count == 0)
                        {
                            ret = userName;
                        }
                        userNames.Add(item.GetType(), userName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.GenerateUserName threw an exception", item.GetType());
                }
            }
            if (userNames.Count > 1)
            {
                _logger.LogWarning("More than one IExternalLoginHandler generated username. Used first one registered, {externalLoginHandler}", userNames.FirstOrDefault().Key);
            }
            return ret;
        }
        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        private async Task<SignInResult> ExternalLoginSignInAsync(IUser user, ExternalLoginInfo info, bool isFirstLogin = false)
        {
            var claims = info.Principal.GetSerializableClaims();
            var userRoles = await _userManager.GetRolesAsync(user);

            var context = new UpdateUserContext(user, info.LoginProvider, claims, userRoles);

            context.IsFirstLogin = isFirstLogin;
            foreach (var item in _externalLoginHandlers)
            {
                try
                {
                    await item.UpdateRoles(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.UpdateRoles threw an exception", item.GetType());
                }
            }
            var ocUser = user as User;

            await _userManager.AddToRolesAsync(user, context.RolesToAdd.Distinct());
            await _userManager.RemoveFromRolesAsync(user, context.RolesToRemove.Distinct());

            var userNeedUpdate = false;
            if (context.PropertiesToUpdate != null)
            {
                ocUser.Properties.Merge(context.PropertiesToUpdate);
                userNeedUpdate = true;
            }
            var currentClaims = ocUser.UserClaims.DistinctBy(x => new { x.ClaimType, x.ClaimValue }).ToList();
            if (context.ClaimsToAdd != null)
            {
                foreach (var cKey in context.ClaimsToAdd.Keys)
                {
                    var existing = currentClaims.FirstOrDefault(x => x.ClaimType == cKey);
                    if (existing != null)
                    {
                        existing.ClaimValue = context.ClaimsToAdd[cKey];
                    }
                    else
                    {
                        currentClaims.Add(new UserClaim() { ClaimType = cKey, ClaimValue = context.ClaimsToAdd[cKey] });
                    }
                }
            }
            if (context.ClaimsToRemove != null)
            {
                currentClaims = currentClaims.Where(x => !context.ClaimsToRemove.Contains(x.ClaimValue)).ToList();
            }
            if (ocUser.UserClaims.Count != currentClaims.Count)
            {
                ocUser.UserClaims = currentClaims;
                userNeedUpdate = true;
            }
            if (userNeedUpdate)
            {
                await _userManager.UpdateAsync(ocUser);
            }


            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                var identityResult = await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error updating the external authentication tokens.");
                }
            }
            else
            {
                await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
            }

            return result;
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl.ToUriComponents());
            }
            else
            {
                return Redirect("~/");
            }
        }

        private RedirectToActionResult RedirectToLogin(string returnUrl)
        {
            var iix = 0;
            foreach (var state in ModelState.Where(x => x.Key == string.Empty))
            {
                foreach (var item in state.Value.Errors)
                {
                    TempData[$"error_{iix++}"] = item.ErrorMessage;
                }
            }
            return RedirectToAction(nameof(AccountController.Login), new { returnUrl });
        }
        private async Task<bool> AddConfirmEmailError(IUser user)
        {
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            if (registrationSettings.UsersMustValidateEmail == true)
            {
                // Require that the users have a confirmed email before they can log on.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, S["You must confirm your email."]);
                    return true;
                }
            }

            return false;
        }

        private bool AddUserEnabledError(IUser user)
        {
            var localUser = user as User;

            if (localUser == null || !localUser.IsEnabled)
            {
                ModelState.AddModelError(String.Empty, S["The specified user is not allowed to sign in."]);
                return true;
            }

            return false;
        }
    }
}
