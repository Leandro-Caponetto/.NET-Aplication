using System;

namespace Core.Entities
{
    public class Seller
    {
        public Seller() { }

        public Guid Id { get; set; }
        public string TNCode { get; set; }
        public string TNAccessToken { get; set; }
        public string TNTokenType { get; set; }
        public string TNUserId { get; set; }
        public string TNScope { get; set; }
        public string CAUserId { get; set; }
        public DateTime CAUserDate { get; set; }
        public int? CADocTypeId { get; set; }
        public string CADocNro { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public virtual DocumentType DocumentType { get; set; }
    }
}