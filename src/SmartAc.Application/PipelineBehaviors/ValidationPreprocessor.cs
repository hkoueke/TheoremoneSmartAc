using FluentValidation;
using FluentValidation.Results;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace SmartAc.Application.PipelineBehaviors;

internal sealed class ValidationPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationPreProcessor<TRequest>> _logger;

    public ValidationPreProcessor(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationPreProcessor<TRequest>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogInformation("No validator found for request '{RequestName}'. Skipping...", requestName);
            return;
        }

        _logger.LogInformation(
            "Validation of request '{RequestName}' started at {DateTimeStarted}",
            requestName, DateTimeOffset.UtcNow);

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task
            .WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        _logger.LogInformation(
            "Validating of request '{RequestName}' ended at {DateTimeStarted}",
            requestName, DateTimeOffset.UtcNow);

        var errorsList = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (errorsList.Any())
        {
            throw new ValidationException(errorsList);
        }
    }
}