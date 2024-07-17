using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class ShippingTypeRepository : IShippingTypeRepository
    {
        private readonly DataContext dataContext;

        public ShippingTypeRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<ShippingType> FindByIdAsync(int id) =>
            await dataContext.ShippingTypes
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync(); 

        public async Task<ShippingType> FindByNameAsync(string name) =>
            await dataContext.ShippingTypes
                .Where(c => c.TypeName == name)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<ShippingType>> GetAll() =>
            await dataContext.ShippingTypes.ToListAsync();

        public void Add(ShippingType shippingType)
        {
            dataContext.ShippingTypes.Add(shippingType);
        }

        public void Delete(ShippingType shippingType)
        {
            dataContext.ShippingTypes.Remove(shippingType);
        }

        public void Update(ShippingType shippingType)
        {
            dataContext.ShippingTypes.Update(shippingType);
        }
    }
}
