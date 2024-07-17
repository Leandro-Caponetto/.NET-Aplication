
using Microsoft.AspNetCore.Identity;
using System;

namespace Core.Entities
{
    public class DocumentType
    {
        public DocumentType() { }

        public int Id { get; set; }
        public string DocumentName { get; set; }
        
    }
}
