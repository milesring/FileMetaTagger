using FileMetaTagger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;

namespace FileMetaTagger
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var logLevelVariable = Environment.GetEnvironmentVariable("LOG_LEVEL");
            int.TryParse(logLevelVariable, out int logLevel);
            var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.Console();
            switch (logLevel)
            {
                case 0:
                    loggerConfiguration.MinimumLevel.Information();
                    break;
                case 1:
                    loggerConfiguration.MinimumLevel.Debug();
                    break;
                default:
                    loggerConfiguration.MinimumLevel.Error();
                    break;
            }
            Log.Logger = loggerConfiguration.CreateLogger();


            ServiceCollection serviceCollection = new();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
            serviceCollection.AddSingleton<PodcastService>();
            serviceCollection.AddSingleton<IAudioWatcherFactory, AudioWatcherFactory>();
            serviceCollection.AddSingleton<AudioWatcherService>();

            string baseDirectory = "Audio";

            var podcastEnv = Environment.GetEnvironmentVariable("PODCASTS");
            if (!string.IsNullOrEmpty(podcastEnv))
            {
                var podcastDS = JsonSerializer.Deserialize<List<Podcast>>(podcastEnv);
                foreach (var podcast in podcastDS)
                {
                    serviceCollection.AddSingleton(typeof(Podcast), podcast);
                }
            }

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var watcherService = serviceProvider.GetService<AudioWatcherService>();
            await watcherService.CreateAudioWatchers();
            await watcherService.StartAudioWatchers();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            while (true)
            {
                logger.LogInformation("Enter a command (-w to return podcast watchers running): ");
                var input = Console.ReadLine();

                if (input.Equals("-w", StringComparison.OrdinalIgnoreCase))
                {
                    var podcasts = watcherService.GetActiveWatchers();
                    logger.LogInformation("Watchers running:");
                    foreach (var podcast in podcasts)
                    {
                        logger.LogInformation($"- {podcast}");
                    }
                }

                if (input.ToLower().Equals("exit"))
                {
                    break;
                }
            }
        }
    }
}
