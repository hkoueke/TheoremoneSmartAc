using FluentValidation;

namespace SmartAc.Application.Features.DeviceReadings.StoreReadings;

internal sealed class ReadingValidator : AbstractValidator<SensorReading>
{
    public ReadingValidator()
    {
        RuleFor(x => x.RecordedDateTime)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(x => x.Temperature).PrecisionScale(5, 2, true);
        RuleFor(x => x.CarbonMonoxide).PrecisionScale(5, 2, true);
        RuleFor(x => x.Humidity).PrecisionScale(5, 2, true);
        RuleFor(x => x.Health).IsInEnum();
    }
}