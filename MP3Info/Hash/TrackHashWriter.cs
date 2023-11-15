using NLog;

namespace MP3Info.Hash
{
    public class TrackHashWriter(bool whatif, bool force) : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public void ProcessTrack(Track track, string root)
        {
            var hashStatus = track.GetCachedHashStatus();

            if (hashStatus == Track.TrackHashStatus.Valid)
            {
                return;
            }
            else if (force == false && (hashStatus == Track.TrackHashStatus.BadlyFormatted || hashStatus == Track.TrackHashStatus.Invalid))
            {
                logger.Warn($"Invalid hash on file {track.Filename} ({track.Hash})");
            }
            else if (whatif == false)
            {
                track.WriteHash();
            }
        }
    }
}
