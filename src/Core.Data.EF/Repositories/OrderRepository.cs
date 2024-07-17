using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DataContext dataContext;

        public OrderRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<Order> FindByIdAsync(Guid id) =>
            await dataContext.Orders
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

        public async Task<Order> FindByCAOrderIdAsync(string CAOrderId) =>
            await dataContext.Orders
                .Where(c => c.CAOrderId == CAOrderId)
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync();


        public void Add(Order order)
        {
            dataContext.Orders.Add(order);
        }

        public void Delete(Order order)
        {
            dataContext.Orders.Remove(order);
        }

        public void Update(Order order)
        {
            dataContext.Orders.Update(order);
        }

        public async Task SavaChangesContext()
            => await dataContext.SaveChangesAsync();
    }
}

