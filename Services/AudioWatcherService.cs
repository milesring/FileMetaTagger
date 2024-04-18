using Microsoft.Extensions.Logging;

namespace FileMetaTagger.Services
{
    internal class AudioWatcherService
    {
        private readonly ILogger<AudioWatcherService> _logger;
        private readonly PodcastService _podcastService;
        private readonly IAudioWatcherFactory _audioWatcherFactory;

        public List<AudioWatcher> Watchers { get; private set; }
        public AudioWatcherService(ILogger<AudioWatcherService> logger, PodcastService podcastService, IAudioWatcherFactory audioWatcherFactory)
        {
            _logger = logger;
            _podcastService = podcastService;
            _audioWatcherFactory = audioWatcherFactory;
            Watchers = new();
        }

        public async Task CreateAudioWatchers()
        {
            Parallel.ForEach(_podcastService.GetPodcasts(), (podcast) =>
                {
                    _logger.LogDebug($"{podcast.Name} creating Audio Watcher");
                    Watchers.Add(_audioWatcherFactory.CreateAudioWatcher(podcast));
                });
        }

        public async Task StartAudioWatchers()
        {
            Parallel.ForEach(Watchers, (watcher) =>
                {
                    _logger.LogDebug($"{typeof(AudioWatcherService)}.{watcher.GetPodcastName()} watcher starting");
                    watcher.StartWatcher();
                });
        }

        public string?[] GetActiveWatchers()
        {
            return Watchers.Select(x => x.GetPodcastName()).ToArray();
        }
    }
}
