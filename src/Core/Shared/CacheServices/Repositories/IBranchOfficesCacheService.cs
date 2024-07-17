using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Shared.CacheServices.Repositories
{
    public interface IBranchOfficesCacheService
    {
        void ResetBranchOfficesCache();
        /*
        IList<BranchOfficeCacheModel> GetBranchOffices();
        Task<IList<BranchOfficeCacheModel>> GetBranchOfficesAsync();
        Task<BranchOfficeCacheModel> GetBranchOfficeByOcasaIdAsync(string caSucursal);
        */
    }
}
