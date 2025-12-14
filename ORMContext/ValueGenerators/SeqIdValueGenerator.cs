using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NUlid.Rng;

namespace ORMContext.ValueGenerators;

public class SeqIdValueGenerator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;
    public override string Next(EntityEntry entry) {
        var rng = new MonotonicUlidRng(); 
        return NUlid.Ulid.NewUlid(rng).ToString().ToLower();
    }
}