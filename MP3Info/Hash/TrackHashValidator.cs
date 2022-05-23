using NLog;
using System.Collections.Generic;

namespace MP3Info.Hash
{
    public class TrackHashValidator : ITrackListProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly AppContext appContext;

        public TrackHashValidator(AppContext appContext)
        {
            this.appContext = appContext;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            foreach (var track in tracks)
            {
                NewMethod(track);
            }
        }

        private void NewMethod(Track track)
        {
            string hash = track.Hash;
            var trackHashStatus = track.GetTrackHashStatus();
            switch (trackHashStatus)
            {
                case Track.TrackHashStatus.None:
                    logger.Warn($"Missing hash for file {track.Filename}");
                    break;
                case Track.TrackHashStatus.Invalid:
                    logger.Warn($"Badly formatted hash for file {track.Filename} ({hash})");
                    break;
                case Track.TrackHashStatus.Valid:
                    if (appContext.Verbose)
                    {
                        logger.Info($"✅Valid hash in file {track.Filename}");
                    }
                    break;
                default:
                    logger.Error($"❌Invalid hash in file {track.Filename}");
                    break;
            }
        }
    }
}
