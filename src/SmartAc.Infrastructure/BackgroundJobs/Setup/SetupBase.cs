using Microsoft.Extensions.Options;
using SmartAc.Infrastructure.Options;

namespace SmartAc.Infrastructure.BackgroundJobs.Setup;

internal abstract class SetupBase
{
    protected readonly JobOptions Options;

    protected SetupBase(IOptionsMonitor<JobOptions> options)
    {
        Options = options.CurrentValue;
    }
}
