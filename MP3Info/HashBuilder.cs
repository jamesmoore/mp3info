using NLog;
using System.Collections.Generic;

namespace MP3Info
{
    class HashBuilder : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;
        private readonly bool force;

        public HashBuilder(bool whatif, bool force)
        {
            this.whatif = whatif;
            this.force = force;
        }

        public void ProcessTracks(Track track, string root)
        {
            if (string.IsNullOrEmpty(track.Comment) == false)
            {
                if (track.HasLegitBase64Hash() == false)
                {
                    logger.Warn($"Invalid hash on file {track.Filename} ({track.Comment})");
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
