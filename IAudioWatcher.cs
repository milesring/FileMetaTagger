namespace FileMetaTagger
{
    internal interface IAudioWatcher
    {
        void StartWatcher();
        string? GetPodcastName();
    }
}