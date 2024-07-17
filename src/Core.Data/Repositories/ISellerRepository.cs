using Core.Entities;
using System;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface ISellerRepository
    {
        Task<Seller> FindByIdAsync(Guid id);
        Task<Seller> FindByTNUserIdAsync(string TNUserId);
        Task<Seller> FindByTNUserIdForceAsync(string TNUserId);
        Task<Order> FindOrderAsync(string clientId, string idPedidoPaqueteCliente, string errorDescription);
        Task<Seller> FindByCAUserIdAsync(string CAUserId);
        Task<Seller> FindByCodeAsync(string TNCode);
        void Add(Seller seller);
        void Update(Seller seller);
        void Delete(Seller seller);
    }
}
