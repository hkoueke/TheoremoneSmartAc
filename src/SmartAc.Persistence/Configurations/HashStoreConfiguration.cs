using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAc.Domain;

namespace SmartAc.Persistence.Configurations;

internal sealed class HashStoreConfiguration : IEntityTypeConfiguration<IdempotentRequestEntry>
{
    public void Configure(EntityTypeBuilder<IdempotentRequestEntry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.HashString)
            .IsUnique();
    }
}