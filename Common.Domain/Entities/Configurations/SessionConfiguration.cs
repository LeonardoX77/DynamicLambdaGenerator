using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Common.Domain.Entities;

namespace Common.Domain.Entities.Configurations
{
    /// <summary>
    /// EF configuration for the Session entity.
    /// </summary>
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        /// <summary>
        /// Configures the Session entity type.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Date)
                   .IsRequired();

            builder.Property(s => s.Time)
                   .IsRequired();

            builder.Property(s => s.SessionType)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(s => s.Notes)
                   .HasMaxLength(500);

            builder.HasOne(s => s.Client)
                   .WithMany(c => c.Sessions)
                   .HasForeignKey(s => s.ClientId);

            builder.HasOne(s => s.Photographer)
                   .WithMany(p => p.Sessions)
                   .HasForeignKey(s => s.PhotographerId);

            builder.HasOne(s => s.Location)
                   .WithMany(l => l.Sessions)
                   .HasForeignKey(s => s.LocationId);
        }
    }
}
