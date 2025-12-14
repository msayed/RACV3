using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ORMContext.Domain;

namespace ORMContext.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight> {
    public void Configure(EntityTypeBuilder<Flight> builder) {

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder
            .HasMany(f => f.FlightHistories)
            .WithOne(fh => fh.Flight)
            .HasForeignKey(fh => fh.FlightId);
    }
}