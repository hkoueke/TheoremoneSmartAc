using AutoMapper;
using SmartAc.Application.Dto;
using SmartAc.Domain;

namespace SmartAc.Application.Profiles;

internal sealed class DeviceLogProfile : Profile
{
    public DeviceLogProfile()
    {
        CreateProjection<Device, DeviceLogDto>()
            .ForMember(
                l => l.Alerts, 
                config => config.MapFrom(d => d.Alerts))
            .ForMember(
                l => l.DeviceReadings, 
                config => config.MapFrom(d => d.DeviceReadings));
    }
}