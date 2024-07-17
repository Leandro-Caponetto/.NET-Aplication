using Core.Entities;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> FindByIdAsync(string id);
        Task<Session> FindByUserIdAndToken(string username, string token);
        void Add(Session session);
        void Update(Session session);
        void Delete(Session session);
    }
}
