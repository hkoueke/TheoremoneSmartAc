using Microsoft.Extensions.Options;

namespace SmartAc.Infrastructure.BackgroundJobs.Setup;

internal abstract class SetupBase<TOptions>
{
    protected readonly TOptions Options;

    protected SetupBase(IOptionsMonitor<TOptions> options)
    {
        Options = options.CurrentValue;
    }
}
