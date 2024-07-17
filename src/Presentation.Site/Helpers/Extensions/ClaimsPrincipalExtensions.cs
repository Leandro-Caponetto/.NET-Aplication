using System.Security.Claims;

namespace Presentation.Site.Helpers.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string UserName(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }

        public static string Id(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue("Id");
        }
    }
}
