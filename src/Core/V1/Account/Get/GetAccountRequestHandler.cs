using Core.Entities;
using Core.V1.Account.Get.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.Get
{
    public class GetAccountRequestHandler : IRequestHandler<GetAccountRequest, AccountModel>
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GetAccountRequestHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager;
        }

        public async Task<AccountModel> Handle(GetAccountRequest request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByNameAsync(request.GetUserName());

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>();
            claims.AddRange(await userManager.GetClaimsAsync(user));

            var roles = await userManager.GetRolesAsync(user);
            foreach (var roleId in roles)
            {
                var role = await roleManager.FindByIdAsync(roleId);
                claims.Add(new Claim(ClaimTypes.Role, role.Id));
                claims.AddRange(await roleManager.GetClaimsAsync(role));
            }

            var response = new AccountModel(user.Id, user.Email, user.FirstName, user.LastName, user.EmailConfirmed);

            return response;
        }
    }
}
