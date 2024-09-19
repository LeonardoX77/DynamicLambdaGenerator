using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Domain.Entities;

namespace Common.Domain.Entities.Configurations
{

    /// <summary>
    /// EF configuration for the Photographer entity.
    /// </summary>
    public class PhotographerConfiguration : IEntityTypeConfiguration<Photographer>
    {
        /// <summary>
        /// Configures the Photographer entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Photographer> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.Email)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.PhoneNumber)
                   .HasMaxLength(15)
                   .IsRequired();

            builder.HasMany(p => p.Sessions)
                   .WithOne(s => s.Photographer)
                   .HasForeignKey(s => s.PhotographerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


