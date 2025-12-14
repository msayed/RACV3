using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ORMContext.Domain;
using ORMContext.ValueGenerators;

namespace ORMContext.Configurations;

public class FlightHistoryConfiguration : IEntityTypeConfiguration<FlightHistory> {
    public void Configure(EntityTypeBuilder<FlightHistory> builder) {

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasValueGenerator<SeqIdValueGenerator>()
            .ValueGeneratedOnAdd();
        
        builder.HasOne(fh => fh.Flight)
            .WithMany(f => f.FlightHistories)
            .HasForeignKey(fh => fh.FlightId);
        
    }
}