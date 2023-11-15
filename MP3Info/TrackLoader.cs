using NLog;
using System;
using System.IO.Abstractions;
namespace MP3Info
{
    public class TrackLoader(IFileSystem fileSystem) : ITrackLoader
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

        private Track BuildTrack(string filename)
        {
            var track = new Track(fileSystem, filename);
            return track;
        }
    }
}
