using NLog;

namespace MP3Info.Hash
{
    public class TrackHashWriter : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;
        private readonly bool force;

        public TrackHashWriter(bool whatif, bool force)
        {
            this.whatif = whatif;
            this.force = force;
        }

        public void ProcessTrack(Track track, string root)
        {
            if (string.IsNullOrEmpty(track.Hash) == false)
            {
                if (track.HasLegitBase64Hash() == false)
                {
                    logger.Warn($"Invalid hash on file {track.Filename} ({track.Hash})");
                    if (force == false)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            if (whatif == false)
            {
                track.WriteHash();
            }
        }
    }
}
