using NLog;
using System.IO.Abstractions;

namespace MP3Info
{
    class EmptyDirectoryRemover
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IFileSystem fileSystem;
        private readonly bool whatif;

        public EmptyDirectoryRemover(IFileSystem fileSystem, bool whatif)
        {
            this.fileSystem = fileSystem;
            this.whatif = whatif;
        }

        public void processDirectory(string startLocation)
        {
            foreach (var directory in fileSystem.Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (fileSystem.Directory.GetFiles(directory).Length == 0 &&
                    fileSystem.Directory.GetDirectories(directory).Length == 0)
                {
                    logger.Info($"Removing directory: {directory}");
                    if (whatif == false)
                    {
                        fileSystem.Directory.Delete(directory, false);
                    }
                }
            }
        }

    }
}
