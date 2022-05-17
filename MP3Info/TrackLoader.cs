using System;
using System.IO;
using NLog;
namespace MP3Info
{
    public class TrackLoader : ITrackLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Track GetTrack(string filename)
        {
            logger.Info($"Indexing: {filename}");

            try
            {
                return BuildTrack(filename);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception reading file {filename}");
                return null;
            }
        }

        private static Track BuildTrack(string filename)
        {
            var fileInfo = new FileInfo(filename);

                var track = new Track();

                track.LoadFromFile(fileInfo);
                
                return track;
        }
    }
}
