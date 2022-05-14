using NLog;
using System.Collections.Generic;

namespace MP3Info.Hash
{
    class HashValidator : ITrackListProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            foreach (var track in tracks)
            {
                string comment = track.Comment;
                if (string.IsNullOrEmpty(comment))
                {
                    logger.Warn($"Missing hash in comment for file {track.Filename}");
                }
                else if (track.HasLegitBase64Hash() == false)
                {
                    logger.Warn($"Invalid hash in comment for file {track.Filename} ({comment})");
                }
                else
                {
                    if (track.TrackHasValidHash())
                    {
                        logger.Info($"✅Valid hash in file {track.Filename}");
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
