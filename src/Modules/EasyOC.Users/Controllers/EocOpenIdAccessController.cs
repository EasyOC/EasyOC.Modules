using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OrchardCore.Modules;
using OrchardCore.OpenId;
using OrchardCore.OpenId.ViewModels;

namespace EasyOC.Users.Controllers
{
    [Authorize, Feature(OpenIdConstants.Features.Server)]
    public class EocOpenIdAccessController : Controller
    {
        [AllowAnonymous, HttpGet, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Logout()
        {
            var response = HttpContext.GetOpenIddictServerResponse();
            if (response != null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = response.Error,
                    ErrorDescription = response.ErrorDescription
                });
            }

            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return NotFound();
            }

            // Warning: unlike the main Logout method, this method MUST NOT be decorated with
            // [IgnoreAntiforgeryToken] as we must be able to reject end session requests
            // sent by a malicious client that could abuse this interactive endpoint to silently
            // log the user out without the user explicitly approving the log out operation.

            await HttpContext.SignOutAsync();

            // If no post_logout_redirect_uri was specified, redirect the user agent
            // to the root page, that should correspond to the home page in most cases.
            if (string.IsNullOrEmpty(request.PostLogoutRedirectUri))
            {
                return Redirect("~/");
            }

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        } 
    }
}
