using System;
using System.IO;
using System.Threading;
using NLog;
namespace MP3Info
{
    class TrackRenamer : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;

        public TrackRenamer(bool whatif)
        {
            this.whatif = whatif;
        }

        class PotentialRename
        {
            public Track Track { get; set; }
            public string NewName { get; set; }
        }

        public void ProcessTracks(Track track, string root)
        {
            var toRename = GetRenames(track, root);

            if (toRename != null)
            {
                logger.Info($"Renaming: {toRename.Track.Filename} ➡ {toRename.NewName}");
                if (whatif == false)
                {
                    try
                    {
                        toRename.Track.Move(toRename.NewName);
                        if (DateTime.Now.Ticks % 500 == 0)
                        {
                            logger.Info("Sleeping");
                            Thread.Sleep(10000);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                }
            }

            
        }

        private PotentialRename GetRenames(Track track, string root)
        {
            if (track.AlbumArtist != null && track.Album != null && track.HasLegitBase64Hash())
            {
                var newFullPath = GetNewName(root, track);

                if (newFullPath != track.Filename && File.Exists(newFullPath) == false && File.Exists(track.Filename))
                {
                    return new PotentialRename()
                    {
                        Track = track,
                        NewName = newFullPath,
                    };
                }
            }
            return null;
        }

        private static string GetNewName(string root, Track track)
        {
            return Path.Combine(
                root,
                BuildDirFromName(track.AlbumArtist),
                BuildDirFromName(track.Album),
                track.GetExpectedFilename()
                );
        }

        private static string BuildDirFromName(string track)
        {
            return track.Replace(":", " - ").Replace("\"", "").Replace("/", " - ").Replace("\\", " - ").Replace("?", "").Replace("...", "…").Replace("  ", " ").Replace(" ; ", "; ").Trim();
        }
    }
}
