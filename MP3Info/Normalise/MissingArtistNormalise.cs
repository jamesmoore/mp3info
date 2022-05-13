using NLog;
using TagLib;

namespace MP3Info.Normalise
{
    internal class MissingArtistNormalise : INormaliseTrack
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool CanBeNormalised(Track track)
        {
            return string.IsNullOrWhiteSpace(track.AlbumArtist);
        }

        public void Normalise(File file)
        {
            logger.Info($"Fixing album artists on {file.Name}");
            file.Tag.AlbumArtists = (string[])file.Tag.Performers.Clone();
        }
    }
}
