using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface IShippingTypeRepository
    {
        Task<IEnumerable<ShippingType>> GetAll();
        Task<ShippingType> FindByIdAsync(int id);
        Task<ShippingType> FindByNameAsync(string name);
        void Add(ShippingType shippingType);
        void Update(ShippingType shippingType);
        void Delete(ShippingType shippingType);
    }
}
