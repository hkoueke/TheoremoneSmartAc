using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAc.API.Bindings;
using SmartAc.API.Filters;
using SmartAc.Application.Contracts;
using SmartAc.Application.Features.DeviceReadings.StoreReadings;
using SmartAc.Application.Features.Devices.AlertLogs;
using SmartAc.Application.Features.Devices.Registration;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ErrorOr;

namespace SmartAc.API.Controllers;

[ApiController]
[Route("api/v1/device")]
[Authorize(Policy = "DeviceIngestion")]
public sealed class DeviceIngestionController : ControllerBase
{
    private readonly ISender _sender;
    public DeviceIngestionController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Allow smart ac devices to register themselves  
    /// and get a jwt token for subsequent operations
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM</param>
    /// <param name="sharedSecret">Unique device shareable secret burned into ROM</param>
    /// <param name="firmwareVersion">Device firmware version at the moment of registering</param>
    /// <returns>A jwt token</returns>
    /// <response code="400">If any of the required data is not present or is invalid.</response>
    /// <response code="404">If there is no matching device found.</response>
    /// <response code="200">If the registration was successfully.</response>
    [HttpPost("{serialNumber}/registration")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous, ValidateFirmware]
    public async Task<IActionResult> RegisterDevice(
        [Required][FromRoute] string serialNumber,
        [Required][FromHeader(Name = "x-device-shared-secret")] string sharedSecret,
        [Required][FromQuery] string firmwareVersion)
    {
        ErrorOr<string> registerResult = await
            _sender.Send(new RegisterDeviceCommand(serialNumber, sharedSecret, firmwareVersion));

        return registerResult.Match<IActionResult>(
            onValue: Ok,
            onError: errors => NotFound(new
            {
                errors.First().Code,
                errors.First().Description
            }));
    }

    /// <summary>
    /// Allow smart ac devices to send sensor readings in batch.
    /// This will additionally trigger analysis over the sensor readings
    /// to generate alerts based on it.
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="sensorReadings">Collection of sensor readings send by a device.</param>
    /// <response code="400">If readings are not properly formatted</response>
    /// <response code="401">If device is not authenticated.</response>
    /// <response code="403">If device is not authorized.</response>
    /// <response code="202">If sensor readings are successfully accepted.</response>
    /// <returns>No Content.</returns>
    [HttpPost("readings/batch")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ValidateReadings]
    public Task<IActionResult> AddSensorReadings(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromBody] IEnumerable<SensorReading> sensorReadings)
    {
        _sender.Send(new StoreReadingCommand(serialNumber, sensorReadings));
        return Task.FromResult<IActionResult>(Accepted());
    }

    /// <summary>
    /// Allow smart ac devices to read their alerts
    /// 
    /// </summary>
    /// <param name="serialNumber">Unique device identifier burned into ROM.</param>
    /// <param name="parameters">Query parameters for data filtering and paging.</param>
    /// <response code="401">If something is wrong on the information provided.</response>
    /// <response code="403">If device is not authorized.</response>
    /// <response code="200">If there are log items to display or not.</response>
    /// <returns>A List of alerts matching the request parameters, with paging information in the response header.</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSensorAlerts(
        [ModelBinder(BinderType = typeof(DeviceInfoBinder))] string serialNumber,
        [FromQuery] QueryParams parameters)
    {
        PagedList<LogItem> logResult = await
            _sender.Send(new GetAlertLogsQuery(serialNumber, parameters));

        var metadata = new
        {
            logResult.TotalCount,
            logResult.PageSize,
            logResult.CurrentPage,
            logResult.TotalPages,
            logResult.HasNext,
            logResult.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
        return Ok(logResult);
    }
}