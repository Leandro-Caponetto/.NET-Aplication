using Core.Data.RequestServices;
using Core.Shared.CacheServices.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Shared.CacheServices
{
    public class BranchOfficeCacheModel
    {
        public string CASucursal { get; set; }
        public int Adress { get; set; }
        public string AdressNro { get; set; }
        public string Floor { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string ZipCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class BranchOfficesCacheService : BaseCacheService, IBranchOfficesCacheService
    {
        private readonly string cacheKey = "BranchOfficesList";
        private readonly TimeSpan expirationTime;
        private readonly IApiMiCorreo caRequestService;

        public BranchOfficesCacheService(IMemoryCache cache, TimeSpan expirationTime, IApiMiCorreo caRequestService)
            : base(cache)
        {
            this.expirationTime = expirationTime;
            this.caRequestService = caRequestService ?? throw new ArgumentNullException(nameof(caRequestService));
        }

        public void ResetBranchOfficesCache()
            => ResetCacheKey(cacheKey);
        /*
        public IList<BranchOfficeCacheModel> GetBranchOffices()
            => GetAllElementsCache<IList<BranchOfficeCacheModel>>(cacheKey, expirationTime, GetBranchOfficesCacheModels);

        public async Task<IList<BranchOfficeCacheModel>> GetBranchOfficesAsync()
            => await GetAllElementsCacheAsync<IList<BranchOfficeCacheModel>>(cacheKey, expirationTime, GetBranchOfficesCacheModels);
        
        public async Task<BranchOfficeCacheModel> GetBranchOfficeByOcasaIdAsync(string caSucursal)
            => (await GetBranchOfficesAsync()).Where(x => x.CASucursal == caSucursal).FirstOrDefault();
        /*
        private IList<BranchOfficeCacheModel> GetBranchOfficesCacheModels()
        {
            var branchOffices = caRequestService.GetBrachOffices();
            return branchOffices.Select(x => new BranchOfficeCacheModel()
            {
                CASucursal = ""
            }).ToList();
        }
        */
    }
}
