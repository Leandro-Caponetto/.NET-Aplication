using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class TNOrderStatusRepository : ITNOrderStatusRepository
    {
        private readonly DataContext dataContext;

        public TNOrderStatusRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<TNOrderStatus> FindByIdAsync(int id) =>
            await dataContext.TNOrderStatuses
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync(); 

        public async Task<TNOrderStatus> FindByNameAsync(string name) =>
            await dataContext.TNOrderStatuses
                .Where(c => c.StatusName == name)
                .FirstOrDefaultAsync();

        public void Add(TNOrderStatus tNOrderStatus)
        {
            dataContext.TNOrderStatuses.Add(tNOrderStatus);
        }

        public void Delete(TNOrderStatus tNOrderStatus)
        {
            dataContext.TNOrderStatuses.Remove(tNOrderStatus);
        }

        public void Update(TNOrderStatus tNOrderStatus)
        {
            dataContext.TNOrderStatuses.Update(tNOrderStatus);
        }

        public async Task<IEnumerable<TNOrderStatus>> GetAll() =>
            await dataContext.TNOrderStatuses.ToListAsync();
    }
}
