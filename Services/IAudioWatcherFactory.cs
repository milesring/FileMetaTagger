namespace FileMetaTagger.Services
{
    internal interface IAudioWatcherFactory
    {
        AudioWatcher CreateAudioWatcher(Podcast podcast);
    }
}
