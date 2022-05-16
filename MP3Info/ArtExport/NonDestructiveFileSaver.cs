using NLog;
using System.IO;

namespace MP3Info.ArtExport
{
    internal class NonDestructiveFileSaver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool ExtractPictureToFile(byte[] bytes, string artPathTemplate, int? index)
        {
            var artPath = string.Format(artPathTemplate, index);

            if (File.Exists(artPath))
            {
                var existingBytes = File.ReadAllBytes(artPath);
                if (ByteArrayCompare(existingBytes, bytes))
                {
                    logger.Info($"Picture already exists: {artPath}");
                    return true;
                }
                else
                {
                    return ExtractPictureToFile(bytes, artPathTemplate, index.HasValue ? index.Value + 1 : 1);
                }
            }
            else
            {
                logger.Info($"Exporting picture {artPath}");
                File.WriteAllBytes(artPath, bytes);
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
