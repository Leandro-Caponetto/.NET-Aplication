using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Data.EF.EntityTypeConfigurations
{
    public class ShippingTypeEntityTypeConfiguration : IEntityTypeConfiguration<ShippingType>
    {
        public void Configure(EntityTypeBuilder<ShippingType> builder)
        {
            builder.ToTable("ShippingTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TypeName)
                .IsRequired(true);

            builder.Property(x => x.Description)
                .IsRequired(true);


        }
    }
}
