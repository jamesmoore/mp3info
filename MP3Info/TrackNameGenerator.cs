using System.IO;

namespace MP3Info
{
    public class TrackNameGenerator
    {
        public string GetNewName(string root, Track track)
        {
            return Path.Combine(
                root,
                BuildDirFromName(track.AlbumArtist),
                BuildDirFromName(track.Album),
                track.GetExpectedFilename()
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
                replaced = replaced.Substring(0, replaced.Length - 1);
            }
            return replaced;
        }

    }
}
