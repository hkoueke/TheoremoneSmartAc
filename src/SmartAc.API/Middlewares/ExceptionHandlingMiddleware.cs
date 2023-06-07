using FluentValidation;

// ReSharper disable once IdentifierTypo
namespace SmartAc.API.Middlewares;

internal sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            await HandleExceptionAsync(context, e).ConfigureAwait(false);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        var response = new
        {
            Title = GetTitle(exception),
            StatusCode = statusCode,
            ErrorMessage = exception.Message,
            Errors = GetErrors(exception)
        };

        context.Response.ContentType = exception is ValidationException ? "application/problem+json" : "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        ValidationException => "Validation Error.",
        _ => "Internal Server Error"
    };

    private static IReadOnlyDictionary<string, string[]>? GetErrors(Exception exception)
    {
        IReadOnlyDictionary<string, string[]>? errors = null;

        if (exception is ValidationException validationException)
        {
            errors = validationException.Errors
                .GroupBy(
                    x => x.PropertyName,
                    x => x.ErrorMessage,
                    (propertyName, errorMessages) => new
                    {
                        Key = propertyName,
                        Errors = errorMessages.Distinct().ToArray()
                    })
                .ToDictionary(x => x.Key, x => x.Errors);
        }

        return errors;
    }
}