using NLog;
using TagLib;

namespace MP3Info.Normalise
{
    internal class DiskFixNormalise : INormaliseTrack
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool CanBeNormalised(Track track)
        {
            return track.Disc == 0;
        }

        public void Normalise(File file)
        {
            logger.Info($"Fixing disc on {file.Name}");
            file.Tag.Disc = 1;
        }
    }
}
