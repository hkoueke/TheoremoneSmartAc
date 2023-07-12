using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAc.Domain;

namespace SmartAc.Persistence.Configurations;

internal sealed class HashStoreConfiguration : IEntityTypeConfiguration<HashStore>
{
    public void Configure(EntityTypeBuilder<HashStore> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.HashCode)
            .IsUnique();
    }
}