namespace FileMetaTagger
{
    public interface IPodcast
    {
        string Directory { get; set; }
        string Name { get; set; }
        string TitleRegex { get; set; }
        bool TagExistingFiles { get; set; }
    }
}