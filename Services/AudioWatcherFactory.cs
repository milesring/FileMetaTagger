using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileMetaTagger.Services
{
    internal class AudioWatcherFactory : IAudioWatcherFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public AudioWatcherFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public AudioWatcher CreateAudioWatcher(Podcast podcast)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<AudioWatcher>>();
            return new AudioWatcher(podcast, logger);
        }
    }
}
