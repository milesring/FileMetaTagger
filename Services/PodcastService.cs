using Microsoft.Extensions.Logging;

namespace FileMetaTagger.Services
{
    internal class PodcastService
    {
        private readonly ILogger<PodcastService> _logger;
        private readonly IEnumerable<Podcast> _podcasts;
        public PodcastService(ILogger<PodcastService> logger, IEnumerable<Podcast> podcasts)
        {
            _logger = logger;
            _podcasts = podcasts;
        }
        public IEnumerable<Podcast> GetPodcasts()
        {
            _logger.LogDebug($"{nameof(PodcastService)} serving Podcasts.");
            //serve podcasts
            return _podcasts;
        }
    }
}
