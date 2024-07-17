using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> FindByIdAsync(Guid id);
        Task<Order> FindByCAOrderIdAsync(string CAOrderId);
        void Add(Order order);
        void Update(Order order);
        void Delete(Order order);
        Task SavaChangesContext();
    }
}
