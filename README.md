# FileMetaTagger

FileMetaTagger is a tool designed to enhance your podcast listening experience by automatically tagging podcast episodes with correct ID3 metadata. 
It is intended to be paired with a podcast application that auto-downloads new episodes of podcasts. 
This tool monitors specific directories for new podcast files and sets the appropriate ID3 tags, allowing for seamless organization and categorization in your audio library.

## Features

- Automatically sets ID3 tags for podcast episodes:
    - Title
    - Artist
    - Album Artist
    - Album
    - Year
    - Track Number
- Watches designated directories for new podcast files.
- Configurable scan rate for monitoring directories.
- Specified for Docker containerization.

## Installation

FileMetaTagger can be easily deployed as a Docker container. Follow these steps to set up FileMetaTagger:

1. Ensure you have Docker installed on your system.
2. Clone this repository.
3. Configure the necessary environment variables:
   - `SCAN_RATE`: The frequency for the application to scan the podcast directories (double) (Defaults to 1 hour if not set).
   - `PODCASTS`: JSON serialized enumerable of Podcast objects.
4. Mount a volume to the container's `/Audio` directory, which serves as the base directory for audio files.

## Configuration

FileMetaTagger requires the following environment variables to be set:

- `SCAN_RATE`: Frequency for the application to scan the podcast directories (double).
- `PODCASTS`: JSON serialized enumerable of Podcast objects.

### Example Podcast Configuration

Podcast properties are as follows:
`Directory`: (String) The relative directory from the base volume of the podcast files
`Name`: (String) The name of the podcast
`TitleRegex`: (String) Regex to parse titles into Number, Date, and Title. Each expression must contain the following named capture groups: "num", "year", "month", "day", "title"
`TagExistingFiles`: (Bool) True to process existing files in the directory, false to only process new files. (Default = true)

An example set of podcasts in JSON format:

```json
[
    {
        "Directory": "/Audio/Matt and Shane's Secret Podcast",
        "Name": "Matt and Shane's Secret Podcast",
        "TitleRegex": "(?<num>\\d+)-(?<year>\\d{4})-(?<month>\\d{2})-(?<day>\\d{2})-(?<title>\\S+)\\.mp3",
        "TagExistingFiles": true
    },
    {
        "Directory": "/Audio/Dudesy",
        "Name": "Dudesy",
        "TitleRegex": "(?<num>\\d+)-(?<year>\\d{4})-(?<month>\\d{2})-(?<day>\\d{2})-(?<title>\\S+)\\.mp3",
        "TagExistingFiles": true
    },
    {
        "Directory": "/Audio/The Adam Friedland Show Podcast",
        "Name": "The Adam Friedland Show Podcast",
        "TitleRegex": "(?<num>\\d+)-(?<year>\\d{4})-(?<month>\\d{2})-(?<day>\\d{2})-(?<title>\\S+)\\.mp3",
        "TagExistingFiles": true
    }
]
```

## Usage

Once FileMetaTagger is configured and running, it will automatically monitor the specified directories for new podcast files. It will then apply the appropriate ID3 tags based on the rules defined in the podcast configuration.

## License

This project is licensed under the [MIT License](LICENSE.txt).

## Acknowledgements

FileMetaTagger utilizes [TagLib#](https://github.com/mono/taglib-sharp)
