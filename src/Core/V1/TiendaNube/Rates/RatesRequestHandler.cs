using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.Rates
{
    public class RatesRequestHandler : IRequestHandler<RatesRequest>
    {
        private readonly UserManager<User> userManager;
        private static readonly HttpClient client = new HttpClient();

        public RatesRequestHandler(UserManager<User> userManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public Task<Unit> Handle(RatesRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Unit.Value);
        }
    }
}
