using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAc.Domain.DeviceReadings;

namespace SmartAc.Persistence.Configurations;

internal sealed class DeviceReadingConfiguration : IEntityTypeConfiguration<DeviceReading>
{
    public void Configure(EntityTypeBuilder<DeviceReading> builder)
    {
        builder.HasKey(x => x.DeviceReadingId);

        builder
            .Property(x => x.Temperature)
            .HasPrecision(5, 2);

        builder
            .Property(x => x.Humidity)
            .HasPrecision(5, 2);

        builder
            .Property(x => x.CarbonMonoxide)
            .HasPrecision(5, 2);
    }
}