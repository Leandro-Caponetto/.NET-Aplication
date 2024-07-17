using Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public interface IDocumentTypeRepository
    {
        Task<IList<DocumentType>> GetAll();
        Task<DocumentType> FindByIdAsync(int id);
        Task<DocumentType> FindByNameAsync(string documentName); 
        void Add(DocumentType documentType);
        void Update(DocumentType documentType);
        void Delete(DocumentType documentType);
    }
}
