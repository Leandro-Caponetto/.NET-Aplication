using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface IProvinceRepository
    {
        Task<IEnumerable<Province>> GetAll();
        Task<Province> FindByIdAsync(int id);
        Task<Province> FindByNameAsync(string name);
        void Add(Province province);
        void Update(Province province);
        void Delete(Province province);
    }
}
