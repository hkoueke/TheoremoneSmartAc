using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartAc.Application.Features.DeviceReadings.StoreReadings;

namespace SmartAc.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class ValidateReadingsAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var readings =
            context.ActionArguments.Values.OfType<IEnumerable<SensorReading>>().Single();

        var validator =
            context.HttpContext.RequestServices.GetRequiredService<IValidator<IEnumerable<SensorReading>>>();

        var validationResults = await validator.ValidateAsync(readings);

        var failures = validationResults.Errors
            .Where(x => x is not null)
            .GroupBy(x => x.PropertyName,
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