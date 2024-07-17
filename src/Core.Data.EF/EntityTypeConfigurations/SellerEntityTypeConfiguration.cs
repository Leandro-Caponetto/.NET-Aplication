using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Data.EF.EntityTypeConfigurations
{
    public class SellerEntityTypeConfiguration : IEntityTypeConfiguration<Seller>
    {
        public void Configure(EntityTypeBuilder<Seller> builder)
        {
            builder.ToTable("Sellers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedOn)
                .IsRequired(true);

            builder.Property(x => x.UpdatedOn)
                .IsRequired(true);

            builder.Property(x => x.TNCode)
                .IsRequired(true);

            builder.Property(x => x.TNAccessToken)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(x => x.TNTokenType)
                .IsRequired(true)
                .IsUnicode(true)
                .HasMaxLength(50);

            builder.Property(x => x.CADocTypeId)
                .IsRequired(false);

            builder
                .HasOne(x => x.DocumentType)
                .WithMany()
                .HasForeignKey(x => x.CADocTypeId);
        }
    }
}
