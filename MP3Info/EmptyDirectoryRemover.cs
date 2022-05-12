using NLog;
using System.IO;

namespace MP3Info
{
    class EmptyDirectoryRemover
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;

        public EmptyDirectoryRemover(bool whatif)
        {
            this.whatif = whatif;
        }

        public void processDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    logger.Info($"Removing directory: {directory}");
                    if (whatif == false)
                    {
                        Directory.Delete(directory, false);
                    }
                }
            }
        }

    }
}
