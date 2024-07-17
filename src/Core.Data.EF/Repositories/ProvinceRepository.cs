using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly DataContext dataContext;

        public ProvinceRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<Province> FindByIdAsync(int id) =>
            await dataContext.Provinces
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync(); 

        public async Task<Province> FindByNameAsync(string name) =>
            await dataContext.Provinces
                .Where(c => c.Name == name)
                .FirstOrDefaultAsync();

        public void Add(Province province)
        {
            dataContext.Provinces.Add(province);
        }

        public void Delete(Province province)
        {
            dataContext.Provinces.Remove(province);
        }

        public void Update(Province province)
        {
            dataContext.Provinces.Update(province);
        }

        public async Task<IEnumerable<Province>> GetAll() =>
            await dataContext.Provinces.ToListAsync();
    }
}
