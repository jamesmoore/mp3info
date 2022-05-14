using NLog;
using System.Collections.Generic;

namespace MP3Info.Hash
{
    class HashValidator : ITrackListProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool verbose;

        public HashValidator(bool verbose)
        {
            this.verbose = verbose;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            foreach (var track in tracks)
            {
                string hash = track.Hash;
                if (string.IsNullOrEmpty(hash))
                {
                    logger.Warn($"Missing hash in comment for file {track.Filename}");
                }
                else if (track.HasLegitBase64Hash() == false)
                {
                    logger.Warn($"Invalid hash in comment for file {track.Filename} ({hash})");
                }
                else
                {
                    if (track.TrackHasValidHash())
                    {
                        if (verbose)
                        {
                            logger.Info($"✅Valid hash in file {track.Filename}");
                        }
                    }
                    else
                    {
                        logger.Error($"❌Invalid hash in file {track.Filename}");
                    }
                }
            }
        }
    }
}
