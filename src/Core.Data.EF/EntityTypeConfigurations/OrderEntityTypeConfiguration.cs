using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Data.EF.EntityTypeConfigurations
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TNCreatedOn)
                .IsRequired(true);

            builder.Property(x => x.TNUpdatedOn)
                .IsRequired(true);

            builder.Property(x => x.ErrorCode)
                .IsRequired(false)
                .IsUnicode(true)
                .HasMaxLength(5);

            builder.Property(x => x.ErrorDescription)
                .IsRequired(false)
                .IsUnicode(true)
                .HasMaxLength(50);

            builder.Property(x => x.CreatedOn)
                .IsRequired(true);

            builder
               .HasOne<Province>()
               .WithMany()
               .HasForeignKey(x => x.SenderProvinceId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

            builder
               .HasOne<Province>()
               .WithMany()
               .HasForeignKey(x => x.ReceiverProvinceId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

            builder
                .HasOne(x => x.ShippingType)
                .WithMany()
                .HasForeignKey(x => x.ShippingTypeId);
        }
    }
}
