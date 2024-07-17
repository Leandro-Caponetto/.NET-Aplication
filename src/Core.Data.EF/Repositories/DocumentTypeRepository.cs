using Core.Data.Repositories;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Data.EF.Repositories
{
    public class DocumentTypeRepository : IDocumentTypeRepository
    {
        private readonly DataContext dataContext;

        public DocumentTypeRepository(DataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<IList<DocumentType>> GetAll() =>
            await dataContext.DocumentTypes.ToListAsync();

        public async Task<DocumentType> FindByIdAsync(int id) =>
            await dataContext.DocumentTypes
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

        public async Task<DocumentType> FindByNameAsync(string documentName) =>
           await dataContext.DocumentTypes
               .Where(c => c.DocumentName == documentName)
               .FirstOrDefaultAsync();

        public void Add(DocumentType documentType)
        {
            dataContext.DocumentTypes.Add(documentType);
        }

        public void Delete(DocumentType documentType)
        {
            dataContext.DocumentTypes.Remove(documentType);
        }

        public void Update(DocumentType documentType)
        {
            dataContext.DocumentTypes.Update(documentType);
        }
    }
}
