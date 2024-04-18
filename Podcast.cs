namespace FileMetaTagger
{
    public class Podcast : IPodcast
    {
        public string Directory { get; set; }
        public string Name { get; set; }
        public string TitleRegex { get; set; }
        public bool TagExistingFiles { get; set; } = true;
    }
}
