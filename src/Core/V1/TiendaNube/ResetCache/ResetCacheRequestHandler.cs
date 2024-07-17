using Core.Entities;
using Core.Shared.CacheServices.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.ResetCache
{
    public class ResetCacheRequestHandler : IRequestHandler<ResetCacheRequest>
    {
        private readonly IBranchOfficesCacheService branchOfficesCacheService;

        public ResetCacheRequestHandler(
            IBranchOfficesCacheService branchOfficesCacheService)
        {
            this.branchOfficesCacheService = branchOfficesCacheService ?? throw new ArgumentNullException(nameof(branchOfficesCacheService));

        }

        public async Task<Unit> Handle(ResetCacheRequest request, CancellationToken cancellationToken)
        {
            await Task.Run(() => branchOfficesCacheService.ResetBranchOfficesCache());
            
            return Unit.Value;
        }
    }
}
