using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Data.EF.EntityTypeConfigurations
{
    public class TNOrderStatusEntityTypeConfiguration : IEntityTypeConfiguration<TNOrderStatus>
    {
        public void Configure(EntityTypeBuilder<TNOrderStatus> builder)
        {
            builder.ToTable("TNOrderStatuses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StatusName)
                .HasMaxLength(50)
                .IsRequired(true);
        }
    }
}
