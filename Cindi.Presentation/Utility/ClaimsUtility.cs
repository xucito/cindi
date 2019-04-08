using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cindi.Presentation.Utility
{
    public static class ClaimsUtility
    {
        public static string GetUsername(ClaimsPrincipal user)
        {
            return user.Claims.Where(c => c.Type == "username").FirstOrDefault().Value;
        }

        public static string GetId(ClaimsPrincipal user)
        {
            var claim = user.Claims.Where(c => c.Type == "id").FirstOrDefault();

            if (claim == null)
                return null;
            return claim.Value;
        }
    }
}
