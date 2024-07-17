using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface ITNOrderStatusRepository
    {
        Task<IEnumerable<TNOrderStatus>> GetAll();
        Task<TNOrderStatus> FindByIdAsync(int id);
        Task<TNOrderStatus> FindByNameAsync(string name);
        void Add(TNOrderStatus tNOrderStatus);
        void Update(TNOrderStatus tNOrderStatus);
        void Delete(TNOrderStatus tNOrderStatus);
    }
}
