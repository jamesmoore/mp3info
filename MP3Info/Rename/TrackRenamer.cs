using System;
using System.IO;
using System.Threading;
using NLog;
namespace MP3Info.Rename
{
    class TrackRenamer : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;

        public TrackRenamer(bool whatif)
        {
            this.whatif = whatif;
        }

        public void ProcessTrack(Track track, string root)
        {
            var toRename = track.HasLegitBase64Hash() ? GetRenames(track, root) : null;

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

        class PotentialRename
        {
            public Track Track { get; set; }
            public string NewName { get; set; }
        }

        private PotentialRename GetRenames(Track track, string root)
        {
            if (track.AlbumArtist != null && track.Album != null)
            {
                var newFullPath = new TrackNameGenerator().GetNewName(root, track);

                if (ShouldRename(track, newFullPath))
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

        private static bool ShouldRename(Track track, string newFullPath)
        {
            return string.Compare(newFullPath, track.Filename, true) != 0 && File.Exists(newFullPath) == false && File.Exists(track.Filename);
        }

    }
}
