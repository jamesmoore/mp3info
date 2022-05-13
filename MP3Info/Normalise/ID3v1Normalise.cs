using NLog;
using TagLib;

namespace MP3Info.Normalise
{
    internal class ID3v1Normalise : INormaliseTrack
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool CanBeNormalised(Track track)
        {
            return (track.TagTypes & TagTypes.Id3v1) == TagTypes.Id3v1;
        }

        public void Normalise(File file)
        {
            logger.Info($"Removing ID3v1 on {file.Name}");
            file.RemoveTags(TagTypes.Id3v1);
        }
    }
}
