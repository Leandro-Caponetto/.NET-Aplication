using System;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.V1.Account.Login.Models
{
    public class LoginModel
    {
        public LoginModel(string id, IImmutableList<Claim> claims)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Claims = claims ?? throw new ArgumentNullException(nameof(claims));
        }

        public string Id { get; set; }
        public IImmutableList<Claim> Claims { get; internal set; }
    }
}
