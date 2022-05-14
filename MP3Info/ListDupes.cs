using NLog;
using System.Collections.Generic;
using System.Linq;

namespace MP3Info
{
    class ListDupes : ITrackListProcessor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            var dupes = tracks.GroupBy(p => p.Hash).Where(p => p.Count() > 1).ToList();

            int i = 1;
            foreach (var dupe in dupes)
            {
                logger.Info($"Dupe {i++}");
                foreach (var item in dupe)
                {
                    logger.Info($"\t{item.Filename}");
                }
            }
        }
    }
}
