using System.IO.Abstractions;

namespace MP3Info.Rename
{
    public class TrackNameGenerator
    {
        private readonly IFileSystem fileSystem;

        public TrackNameGenerator(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public bool CanGetName(Track track)
        {
            return track.HasLegitBase64Hash() &&
                track.AlbumArtist != null &&
                string.IsNullOrWhiteSpace(BuildDirFromName(track.AlbumArtist)) == false &&
                track.Album != null &&
                string.IsNullOrWhiteSpace(BuildDirFromName(track.Album)) == false;
        }

        public string GetNewName(string root, Track track)
        {
            return fileSystem.Path.Combine(
                root,
                BuildDirFromName(track.AlbumArtist),
                BuildDirFromName(track.Album),
                GetExpectedFilename(track)
                );
        }

        private string BuildDirFromName(string track)
        {
            var replaced = track.
                Replace(":", " - ").
                Replace("\"", "").
                Replace("/", " - ").
                Replace("\\", " - ").
                Replace("?", "").
                Replace("...", "…").
                Replace("  ", " ").
                Replace(" ; ", "; ").
                Trim();

            while (replaced.EndsWith("."))
            {
                replaced = replaced[0..^1];
            }
            return replaced;
        }

        private string GetExpectedFilename(Track track)
        {
            var expectedFilename = $"{track.Disc:00}{track.TrackNumber:00} {track.Hash?.Replace("/", "-")}{fileSystem.Path.GetExtension(track.Filename)}";
            return expectedFilename;
        }
    }
}
