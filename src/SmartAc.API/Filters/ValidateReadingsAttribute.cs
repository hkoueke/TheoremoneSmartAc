using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.Application.Features.DeviceReadings.StoreReadings;
using FluentValidation.Results;

namespace SmartAc.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class ValidateReadingsAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await Task.Yield();

        IEnumerable<SensorReading> readings =
            context.ActionArguments.Values.OfType<IEnumerable<SensorReading>>().Single();

        IValidator<SensorReading> validator =
            context.HttpContext.RequestServices.GetRequiredService<IValidator<SensorReading>>();

        var tasks =
            readings.Select(x => validator.ValidateAsync(x));

        ValidationResult[] results = await Task.WhenAll(tasks);

        Dictionary<string, string[]> failures =
            results.SelectMany(x => x.Errors)
                   .GroupBy(
                       x => x.PropertyName,
                       x => x.ErrorMessage,
                       (propertyName, errorMessages) => new
                       {
                           Property = propertyName,
                           Errors = errorMessages.Distinct().ToArray()
                       })
                   .ToDictionary(x => x.Property, x => x.Errors);

        if (!failures.Any())
        {
            await next();
            return;
        }

        var problemDetails = new ValidationProblemDetails(failures);
        context.Result = new UnprocessableEntityObjectResult(problemDetails);
    }
}