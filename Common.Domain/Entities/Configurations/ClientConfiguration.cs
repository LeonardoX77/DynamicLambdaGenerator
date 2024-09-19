using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Domain.Entities;

namespace Common.Domain.Entities.Configurations
{
    /// <summary>
    /// EF configuration for the Client entity.
    /// </summary>
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        /// <summary>
        /// Configures the Client entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(c => c.Email)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasIndex(c => c.Email)
                   .IsUnique();

            builder.Property(c => c.PhoneNumber)
                   .HasMaxLength(15)
                   .IsRequired();

            builder.Property(c => c.Comments)
                    .HasMaxLength(254);

            builder.HasMany(c => c.Sessions)
                   .WithOne(s => s.Client)
                   .HasForeignKey(s => s.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
