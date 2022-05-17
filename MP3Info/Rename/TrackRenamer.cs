using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
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
                var originalName = track.Filename;
                logger.Info($"Renaming: {originalName} ➡ {toRename.NewName}");
                if (whatif == false)
                {
                    try
                    {
                        track.Move(toRename.NewName);

                        var destinationPath = Path.GetDirectoryName(toRename.NewName);
                        var sourcePath = Path.GetDirectoryName(originalName);

                        MoveImages(sourcePath, destinationPath);

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

        private static void MoveImages(string sourcePath, string destinationPath)
        {
            var sourceDirectory = new DirectoryInfo(sourcePath);

            var filetypes = new string[] {
                            "*.jpg",
                            "*.png",
                            "*.jpeg",
                            "*.gif",
                        };

            if (sourceDirectory.Exists)
            {
                var sourceFiles = filetypes.Select(p => Directory.GetFiles(sourceDirectory.FullName, p, SearchOption.TopDirectoryOnly)).SelectMany(p => p).OrderBy(p => p).ToList();
                foreach (var sourceFile in sourceFiles)
                {
                    var destinationFile = Path.Combine(destinationPath, Path.GetFileName(sourceFile));
                    MoveFile(sourceFile, destinationFile);
                }
            }
        }

        private static void MoveFile(string sourceFile, string destinationFile)
        {
            if (File.Exists(destinationFile) == false)
            {
                logger.Info($"Renaming: {sourceFile} ➡ {destinationFile}");
                File.Move(sourceFile, destinationFile);
            }
            else
            {
                logger.Info($"Not renaming (destination exists): {sourceFile} ➡ {destinationFile}");
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
