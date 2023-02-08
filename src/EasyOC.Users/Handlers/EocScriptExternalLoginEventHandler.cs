using EasyOC.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OrchardCore.Entities;
using OrchardCore.Scripting;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Helpers;

namespace EasyOC.Users.Handlers
{
    public class EocScriptExternalLoginEventHandler : IExternalLoginEventHandler
    {
        private readonly ILogger _logger;
        private readonly IScriptingManager _scriptingManager;
        private readonly ISiteService _siteService;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private readonly UserManager<IUser> _userManager;


        public EocScriptExternalLoginEventHandler(
            ISiteService siteService,
            IScriptingManager scriptingManager,
            ILogger<ScriptExternalLoginEventHandler> logger
,
            UserManager<IUser> userManager)
        {
            _siteService = siteService;
            _scriptingManager = scriptingManager;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<string> GenerateUserName(string provider, IEnumerable<SerializableClaim> claims)
        {
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (registrationSettings.UseScriptToGenerateUsername)
            {
                var context = new { userName = String.Empty, loginProvider = provider, externalClaims = claims };
                _logger.LogWarning("GenerateUserName Context:{context}", JsonConvert.SerializeObject(context, _jsonSettings));
                var script = $"js: function generateUsername(context) {{\n{registrationSettings.GenerateUsernameScript}\n}}\nvar context = {JsonConvert.SerializeObject(context, _jsonSettings)};\ngenerateUsername(context);\nreturn context;";

                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                if (evaluationResult?.userName != null)
                {
                    return evaluationResult.userName;
                }
            }
            return string.Empty;
        }

        public async Task UpdateRoles(UpdateRolesContext updateContext)
        {
            var context = updateContext as UpdateUserContext;
            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseScriptToSyncRoles)
            {
                var script = $"js: function syncRoles(context) {{\n{loginSettings.SyncRolesScript}\n}}\nvar context={JsonConvert.SerializeObject(context, _jsonSettings)};\nsyncRoles(context);\nreturn context;";
                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                context.RolesToAdd.AddRange((evaluationResult.rolesToAdd as object[]).Select(i => i.ToString()));
                context.RolesToRemove.AddRange((evaluationResult.rolesToRemove as object[]).Select(i => i.ToString()));
                var dict = (evaluationResult.claimsToAdd as IDictionary<string, object>)?.ToDictionary(x => x.Key, v => v.Value.ToString());
                foreach (var key in dict.Keys)
                {
                    context.ClaimsToAdd[key] = dict[key];
                }
                context.ClaimsToRemove.AddRange((evaluationResult.claimsToRemove as object[]).Select(i => i.ToString()));
                if (evaluationResult.propertiesToUpdate != null)
                {
                    context.PropertiesToUpdate.Merge(evaluationResult.propertiesToUpdate);
                }

            }
        }
    }
}
