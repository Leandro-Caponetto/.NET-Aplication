using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Site.Auth.Jwt
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedTokenAsync(string userName, ClaimsIdentity identity, TimeSpan? duration = null);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id, IImmutableList<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromToken(string token, bool allowExpired);
    }
}
