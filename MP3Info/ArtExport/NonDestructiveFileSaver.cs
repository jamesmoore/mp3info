using NLog;
using System;
using System.IO;

namespace MP3Info.ArtExport
{
    public class NonDestructiveFileSaver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool SaveBytesToFile(byte[] bytes, Func<int?, string> filenameGenerator, int? index = null)
        {
            var filePath = filenameGenerator(index);

            if (File.Exists(filePath))
            {
                var existingBytes = File.ReadAllBytes(filePath);
                if (ByteArrayCompare(existingBytes, bytes))
                {
                    logger.Info($"File already exists: {filePath}");
                    return true;
                }
                else
                {
                    return SaveBytesToFile(bytes, filenameGenerator, index.HasValue ? index.Value + 1 : 1);
                }
            }
            else
            {
                logger.Info($"Writing file {filePath}");
                File.WriteAllBytes(filePath, bytes);
                return true;
            }
        }

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }

    }
}
