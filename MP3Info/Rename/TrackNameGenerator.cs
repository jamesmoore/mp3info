using System.IO;

namespace MP3Info.Rename
{
    public class TrackNameGenerator
    {
        public bool CanGetName(Track track)
        {
            return track.HasLegitBase64Hash() && 
                track.AlbumArtist != null &&
                string.IsNullOrWhiteSpace(BuildDirFromName(track.AlbumArtist)) == false &&
                track.Album != null &&
                string.IsNullOrWhiteSpace(BuildDirFromName(track.Album)) == false ;
        }

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
