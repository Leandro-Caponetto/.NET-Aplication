using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using System.Linq;
using System.Reflection;

namespace Core.Data.EF
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShippingType> ShippingTypes { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<TNOrderStatus> TNOrderStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureEntities(modelBuilder);
        }

        private void ConfigureEntities(ModelBuilder modelBuilder)
        {
            var entityTypeConfigurationTypes = GetType().Assembly.GetTypes()
                            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                            .ToList();

            var genericMethodDefinition = GetType()
                .GetMethod(nameof(ApplyEntityTypeConfiguration), BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var entityTypeConfigurationType in entityTypeConfigurationTypes)
            {
                var genericMethod = genericMethodDefinition
                    .MakeGenericMethod(new[] {
                        entityTypeConfigurationType,
                        entityTypeConfigurationType.GetInterfaces()[0].GetGenericArguments()[0]
                    });

                genericMethod.Invoke(this, new[] { modelBuilder });
            }
        }

        private void ApplyEntityTypeConfiguration<TEntityTypeConfiguration, TEntity>(ModelBuilder modelBuilder)
            where TEntity : class
            where TEntityTypeConfiguration : IEntityTypeConfiguration<TEntity>, new()
        {
            modelBuilder.ApplyConfiguration(new TEntityTypeConfiguration());
        }
    }
}
