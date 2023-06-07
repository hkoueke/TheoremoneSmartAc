using SmartAc.Domain.Alerts;
using SmartAc.Domain.DeviceReadings;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartAc.Domain;

public sealed class Device : EntityBase
{
    private readonly HashSet<DeviceRegistration> _registrations = new();

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string SerialNumber { get; set; } = string.Empty;

    public string SharedSecret { get; set; } = string.Empty;

    public string FirmwareVersion { get; private set; } = string.Empty;

    public IReadOnlyCollection<DeviceRegistration> DeviceRegistrations => _registrations;

    public IReadOnlyCollection<DeviceReading> DeviceReadings { get; private set; } = new List<DeviceReading>();

    public IReadOnlyCollection<Alert> Alerts { get; private set; } = new List<Alert>();

    public DateTimeOffset? FirstRegistrationDate { get; private set; }

    public DateTimeOffset? LastRegistrationDate { get; private set; }

    public void AddRegistration(DeviceRegistration registration, string firmwareVersion)
    {
        //Deactivate current registrations
        foreach (var r in _registrations.Where(x => x.Active))
        {
            r.Active = false;
        }

        //Update registration dates
        FirstRegistrationDate ??= registration.RegistrationDate;
        LastRegistrationDate = registration.RegistrationDate;

        //Set firmware version
        FirmwareVersion = firmwareVersion;

        //Add new registration to collection
        _registrations.Add(registration);
    }
}