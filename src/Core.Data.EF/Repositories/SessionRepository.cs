using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly DataContext dataContext;

        public SessionRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<Session> FindByIdAsync(string id)
        {
            return await dataContext.Sessions.FindAsync(id);
        }

        public async Task<Session> FindByUserIdAndToken(string username, string token)
        {
            return await dataContext.Sessions
                .SingleAsync(x => x.UserId == username && x.Token == token);
        }

        public void Add(Session session)
        {
            dataContext.Sessions.Add(session);
        }

        public void Delete(Session session)
        {
            dataContext.Sessions.Remove(session);
        }

        public void Update(Session session)
        {
            dataContext.Sessions.Update(session);
        }
    }
}
