using System;
using System.IO;
using System.Threading;
using NLog;
namespace MP3Info.Rename
{
    public class TrackRenamer : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;
        private readonly TrackNameGenerator trackNameGenerator = new();

        public TrackRenamer(bool whatif)
        {
            this.whatif = whatif;
        }

        public void ProcessTrack(Track track, string root)
        {
            var toRename = GetRenames(track, root);

            if (toRename.RenameState == RenameState.Ok)
            {
                logger.Info($"Renaming: {track.Filename} ➡ {toRename.NewName}");
                if (whatif == false)
                {
                    try
                    {
                        track.Move(toRename.NewName);
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
            else if (toRename.RenameState != RenameState.CorrectlyNamed)
            {
                logger.Warn($"Can't rename: {track.Filename} ({toRename.RenameState})");
            }
        }

        class PotentialRename
        {
            public PotentialRename(RenameState renameState, string newName = null)
            {
                RenameState = renameState;
                NewName = newName;
            }

            public RenameState RenameState { get; }
            public string NewName { get; }
        }

        public enum RenameState
        {
            None,
            InsufficientMetadata,
            DestinationExists,
            SourceNonExistant,
            CorrectlyNamed,
            Ok,
        }

        private PotentialRename GetRenames(Track track, string root)
        {
            var canRename = trackNameGenerator.CanGetName(track);
            if (canRename == false)
            {
                return new PotentialRename(RenameState.InsufficientMetadata);
            }
            else
            {
                var newFullPath = trackNameGenerator.GetNewName(root, track);
                if (string.Compare(newFullPath, track.Filename, true) == 0)
                {
                    return new PotentialRename(RenameState.CorrectlyNamed);
                }
                else if (File.Exists(newFullPath))
                {
                    return new PotentialRename(RenameState.DestinationExists);
                }
                else if (File.Exists(track.Filename) == false)
                {
                    return new PotentialRename(RenameState.SourceNonExistant);
                }
                else
                {
                    return new PotentialRename(RenameState.Ok, newFullPath);
                }
            }
        }
    }
}
