using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Domain.Entities;

namespace Common.Domain.Entities.Configurations
{
    /// <summary>
    /// EF configuration for the Location entity.
    /// </summary>
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        /// <summary>
        /// Configures the Location entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(l => l.Address)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.HasMany(l => l.Sessions)
                   .WithOne(s => s.Location)
                   .HasForeignKey(s => s.LocationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
