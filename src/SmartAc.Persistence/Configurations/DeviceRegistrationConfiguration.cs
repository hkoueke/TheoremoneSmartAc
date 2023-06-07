using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAc.Domain;

namespace SmartAc.Persistence.Configurations;

internal sealed class DeviceRegistrationConfiguration :  IEntityTypeConfiguration<DeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DeviceRegistration> builder)
    {
        builder.HasKey(x => x.DeviceRegistrationId);
    }
}