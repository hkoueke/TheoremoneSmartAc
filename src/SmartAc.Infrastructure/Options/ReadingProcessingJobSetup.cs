using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using SmartAc.Infrastructure.BackgroundJobs;

namespace SmartAc.Infrastructure.Options
{
    internal sealed class ReadingProcessingJobSetup : IConfigureOptions<QuartzOptions>
    {
        private readonly int _intervalInSeconds;

        public ReadingProcessingJobSetup(IConfiguration config)
        {
            _intervalInSeconds =
                config.GetRequiredSection("BackgroundJobParams")
                      .GetValue<int>("IntervalInSeconds");
        }

        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(ReadingProcessorBackgroundJob));
            
            options
                .AddJob<ReadingProcessorBackgroundJob>(builder => builder.WithIdentity(jobKey))
                .AddTrigger(trigger =>
                    trigger
                        .ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule
                                .WithIntervalInSeconds(_intervalInSeconds)
                                .RepeatForever()));
        }
    }
}
