using NLog;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
namespace MP3Info.Rename
{
    public class TrackRenamer : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IFileSystem fileSystem;
        private readonly bool whatif;
        private readonly TrackNameGenerator trackNameGenerator;

        public TrackRenamer(IFileSystem fileSystem, bool whatif)
        {
            this.fileSystem = fileSystem;
            this.whatif = whatif;
            this.trackNameGenerator = new TrackNameGenerator(fileSystem);
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

                        var destinationPath = fileSystem.Path.GetDirectoryName(toRename.NewName);
                        var sourcePath = fileSystem.Path.GetDirectoryName(originalName);

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

        private void MoveImages(string sourcePath, string destinationPath)
        {
            var sourceDirectory = fileSystem.DirectoryInfo.New(sourcePath);

            var filetypes = new string[] {
                            "*.jpg",
                            "*.png",
                            "*.jpeg",
                            "*.gif",
                            "*.txt",
                        };

            if (sourceDirectory.Exists)
            {
                var sourceFiles = filetypes.Select(p => fileSystem.Directory.GetFiles(sourceDirectory.FullName, p, System.IO.SearchOption.TopDirectoryOnly)).SelectMany(p => p).OrderBy(p => p).ToList();
                foreach (var sourceFile in sourceFiles)
                {
                    var destinationFile = fileSystem.Path.Combine(destinationPath, fileSystem.Path.GetFileName(sourceFile));
                    MoveFile(sourceFile, destinationFile);
                }
            }
        }

        private void MoveFile(string sourceFile, string destinationFile)
        {
            if (fileSystem.File.Exists(destinationFile) == false)
            {
                logger.Info($"Renaming: {sourceFile} ➡ {destinationFile}");
                fileSystem.File.Move(sourceFile, destinationFile);
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
                else if (fileSystem.File.Exists(newFullPath))
                {
                    return new PotentialRename(RenameState.DestinationExists);
                }
                else if (fileSystem.File.Exists(track.Filename) == false)
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
