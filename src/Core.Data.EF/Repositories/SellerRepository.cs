using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class SellerRepository : ISellerRepository
    {
        private readonly DataContext dataContext;

        public SellerRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<Seller> FindByIdAsync(Guid id) =>
            await dataContext.Sellers
                .Where(c => c.Id == id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        public async Task<Seller> FindByTNUserIdAsync(string TNUserId) =>
            await dataContext.Sellers
                .Where(j => j.TNUserId == TNUserId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        public async Task<Seller> FindByTNUserIdForceAsync(string TNUserId)
        {
            /*
            dataContext.Sellers.Load();
            return await dataContext.Sellers
                .Where(j => j.TNUserId == TNUserId && !(from s in dataContext.Sellers where s.TNUserId == TNUserId && j.CreatedOn < s.CreatedOn select true).Any())
                .Take(1)
                .FirstOrDefaultAsync();
            */
            return await dataContext.Sellers.Where(s => s.TNUserId == TNUserId).OrderByDescending(o => o.CreatedOn).FirstOrDefaultAsync();
        }

        public async Task<Order> FindOrderAsync(string clientId, string idPedidoPaqueteCliente, string errorDescription) =>
            await dataContext.Orders
                .Where(j => j.CAIdClientId == clientId && j.TNOrderId.ToString() == idPedidoPaqueteCliente && j.ErrorDescription == errorDescription)
                .OrderByDescending(j => j.Id)
                .FirstOrDefaultAsync();


        public async Task<Seller> FindByCAUserIdAsync(string CAUserId) =>
            await dataContext.Sellers
                .Where(x => x.CAUserId == CAUserId)
                .Take(1)
                .FirstOrDefaultAsync();

        public async Task<Seller> FindByCodeAsync(string TNCode) =>
            await dataContext.Sellers
                .Where(c => c.TNCode == TNCode)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        public void Add(Seller seller)
        {
            dataContext.Sellers.Add(seller);
            dataContext.SaveChanges();
            dataContext.Entry(seller).GetDatabaseValues();
            dataContext.Sellers.Local.Add(seller);
        }

        public void Delete(Seller seller)
        {
            dataContext.Sellers.Remove(seller);
        }

        public void Update(Seller seller)
        {
            dataContext.Sellers.Update(seller);
            dataContext.SaveChanges();
            dataContext.Entry(seller).Reload();
        }
    }
}
