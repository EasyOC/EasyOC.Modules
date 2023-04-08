using Newtonsoft.Json.Linq;
using OrchardCore.Users;
using OrchardCore.Users.Handlers;

namespace EasyOC.Users.Models
{
    public class UpdateUserContext : UpdateRolesContext
    {
        public UpdateUserContext(IUser user, string loginProvider, IEnumerable<SerializableClaim> externalClaims, IEnumerable<string> userRoles)
            : base(user, loginProvider, externalClaims, userRoles)
        {
        }
        public bool IsFirstLogin { get; set; }
        public IDictionary<string, string> ClaimsToAdd { get; set; } = new Dictionary<string, string>();

        //
        // 摘要:
        //     Gets the roles to be removed from the user claims.
        public List<string> ClaimsToRemove { get; set; } = new List<string>();
        public JObject PropertiesToUpdate { get; set; }
    }
}
