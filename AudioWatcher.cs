using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FileMetaTagger
{
    internal class AudioWatcher : IAudioWatcher
    {
        private readonly Podcast _podcast;
        private readonly ILogger<AudioWatcher> _logger;

        private delegate Task EventHandler();

        private event EventHandler LoadEvent;
        private event EventHandler LoadCompletedEvent;

        private HashSet<string> _previousFiles = new();
        private Timer _timer;
        public AudioWatcher(Podcast podcast, ILogger<AudioWatcher> logger)
        {
            _podcast = podcast;
            _logger = logger;
            LoadEvent += LoadHandler;
            LoadCompletedEvent += LoadCompletedHandler;
        }

        public string? GetPodcastName()
        {
            return _podcast?.Name;
        }

        public virtual void StartWatcher()
        {
            LoadEvent?.Invoke();
        }

        private async Task LoadHandler()
        {
            _logger.LogDebug($"{_podcast.Name}: Loading initial files.");
            //check all existing files for tags
            //get files
            if (!Directory.Exists(_podcast.Directory))
            {
                _logger.LogError($"{_podcast.Name}: Directory {_podcast.Directory} does not exist.");
                if (Directory.Exists(Directory.GetParent(_podcast.Directory).FullName))
                {
                    _logger.LogError($"{_podcast.Name}: Parent Directory: ");
                    foreach (var directory in Directory.GetDirectories(Directory.GetParent(_podcast.Directory).FullName))
                    {
                        _logger.LogError($"\t{directory}");
                    }
                }
            }
            string[] mp3Files = Directory.GetFiles(_podcast.Directory, "*.mp3");
            string[] mp4Files = Directory.GetFiles(_podcast.Directory, "*.mp4");

            string[] audioFiles = mp3Files.Concat(mp4Files).ToArray();
            _logger.LogDebug($"{_podcast.Name}: Found {audioFiles.Length} existing files.");

            //get initial file set
            _previousFiles = new HashSet<string>(audioFiles);

            if (audioFiles.Length > 0)
            {
                int filesToShow = audioFiles.Length > 10 ? 10 : audioFiles.Length;
                if (filesToShow > 0)
                {
                    _logger.LogDebug($"\tShowing {filesToShow} files.");
                    for (int i = 0; i < filesToShow; i++)
                    {
                        _logger.LogDebug($"\t{audioFiles[i]}");
                    }
                }
            }
            if (_podcast.TagExistingFiles)
            {
                _logger.LogInformation($"{_podcast.Name}: Setting initial tags for existing files.");
                Parallel.ForEach(audioFiles, SetTags);
                _logger.LogInformation($"{_podcast.Name}: Initial tags set for existing files.");
            }

            LoadCompletedEvent?.Invoke();
        }
        private async Task LoadCompletedHandler()
        {
            var rateEnv = Environment.GetEnvironmentVariable("SCAN_RATE");
            TimeSpan rate = TimeSpan.FromHours(1);
            if (double.TryParse(rateEnv, out double hours))
            {
                rate = TimeSpan.FromHours(hours);
            }
            _timer = new(ScanDirectory, null, TimeSpan.Zero, rate);
        }

        private void SetTags(string filePath)
        {
            if (!IsFileReady(filePath))
            {
                _logger.LogDebug($"{_podcast.Name}:{Path.GetFileName(filePath)} not able to be opened.");
                return;
            }
            TagLib.File? tfile = null;
            try
            {
                tfile = TagLib.File.Create(filePath);
            }
            catch (TagLib.CorruptFileException e)
            {
                _logger.LogError($"{_podcast.Name}: File {Path.GetFileName(filePath)} corrupt or unable to be opened.");
                return;
            }
            var retagAllFilesVariable = Environment.GetEnvironmentVariable("RETAGALLFILES");

            if (!bool.TryParse(retagAllFilesVariable, out bool retag) && tfile.Tag.AlbumArtists.Contains(_podcast.Name) &&
                tfile.Tag.Performers.Contains(_podcast.Name) &&
                tfile.Tag.Album.Equals(_podcast.Name))
            {
                _logger.LogDebug($"{_podcast.Name}: file {Path.GetFileName(filePath)} tags already set.");
                //tags already set
                return;
            }

            Regex titleRegex = new Regex(_podcast.TitleRegex);
            var matches = titleRegex.Match(Path.GetFileName(filePath));
            if (matches.Success)
            {
                //process title
                int year = int.Parse(matches.Groups["year"].Value);
                int month = int.Parse(matches.Groups["month"].Value);
                int day = int.Parse(matches.Groups["day"].Value);
                DateTime episodeDate = new DateTime(year, month, day);
                string replacedTitle = matches.Groups["title"].Value.Replace("-", " ");
                string title = $"{episodeDate.ToShortDateString()} - {replacedTitle}";
                tfile.Tag.Title = title;

                tfile.Tag.Year = (uint)year;

                //process episode number
                uint num = uint.Parse(matches.Groups["num"].Value);
                tfile.Tag.Track = num;
            }
            else
            {
                _logger.LogError($"{_podcast.Name}: Unable to parse filename for title. {Path.GetFileName(filePath)}");
            }

            tfile.Tag.AlbumArtists = [_podcast.Name];
            tfile.Tag.Performers = [_podcast.Name];
            tfile.Tag.Album = _podcast.Name;
            tfile.Tag.Genres = ["Podcast"];
            try
            {

                tfile.Save();
                _logger.LogDebug($"{_podcast.Name}: File {Path.GetFileName(filePath)} tags set.");
                _logger.LogInformation($"{_podcast.Name}: New episode added. {tfile.Tag.Track} - {tfile.Tag.Title}");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                _logger.LogError($"{_podcast.Name}: File {Path.GetFileName(filePath)} tags unable to be saved.");
            }

        }


        private bool IsFileReady(string filePath)
        {
            _logger.LogDebug($"{_podcast.Name}: {Path.GetFileName(filePath)} checking if file is ready.");
            try
            {
                //attempt to open the file with read-only access
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    _logger.LogDebug($"{_podcast.Name}: {Path.GetFileName(filePath)} able to be opened.");
                    return true;
                }
            }
            catch (IOException)
            {
                //file still in use
                _logger.LogError($"{_podcast.Name}: File {Path.GetFileName(filePath)} currently in use.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_podcast.Name}: File {Path.GetFileName(filePath)} other exception.\n{ex.Message}");
                //attempt to fix with permissions
                FileInfo fileInfo = new FileInfo(filePath);
                try
                {
                    fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                    fileInfo.Attributes |= FileAttributes.Normal;
                    return IsFileReady(filePath);
                }
                catch (Exception setAttributesException)
                {
                    _logger.LogError($"{_podcast.Name}: Failed to alter attributes of {filePath}.\n{setAttributesException.Message}");
                }
                return false;
            }
        }

        private void ScanDirectory(object state)
        {
            try
            {
                _logger.LogInformation($"{_podcast.Name} Directory scan running.");
                string[] mp3Files = Directory.GetFiles(_podcast.Directory, "*.mp3");
                string[] mp4Files = Directory.GetFiles(_podcast.Directory, "*.mp4");

                string[] audioFiles = mp3Files.Concat(mp4Files).ToArray();

                foreach (string file in audioFiles)
                {
                    if (!_previousFiles.Contains(file))
                    {
                        //new file found
                        _logger.LogInformation($"{_podcast.Name}: New episode detected detected.");
                        while (!IsFileReady(file))
                        {
                            Thread.Sleep(5000);
                            _logger.LogDebug($"{_podcast.Name}: New file still copying..");
                        }
                        SetTags(file);
                    }
                }
                _previousFiles = new HashSet<string>(audioFiles);

            }
            catch (Exception ex)
            {
                _logger.LogError($"{_podcast.Name}: {ex}");
            }
        }
    }
}
