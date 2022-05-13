using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MP3Info
{
    public class ArtExporter : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;

        public ArtExporter(bool whatif)
        {
            this.whatif = whatif;
        }

        public void ProcessTrack(Track track, string root)
        {
            if (track.Pictures > 0)
            {
                ExtractArt(track);
            }
        }

        private void ExtractArt(Track trackWithPic)
        {
            var filename = trackWithPic.Filename;
            var directory = trackWithPic.GetDirectory();

            var mimeFileMap = new Dictionary<string, string>()
            {
                { "image/jpeg",  "folder{0}.jpg"},
                { "image/jpg",  "folder{0}.jpg"},
                { "image/png",  "folder{0}.png"},
                { "image/gif",  "folder{0}.gif"},
            }.ToLookup(p => p.Key, p => p.Value);

            using (var tagFile = TagLib.File.Create(filename))
            {
                var pictures = tagFile.Tag.Pictures.ToList();

                var picturesWithTemplate = pictures.Select(p =>
                    new
                    {
                        Picture = p,
                        FilenameTemplate = mimeFileMap[p.MimeType].FirstOrDefault(),
                    }).ToList();

                foreach (var pictureWithTemplate in picturesWithTemplate)
                {
                    if (pictureWithTemplate.FilenameTemplate != null)
                    {
                        logger.Info($"Exporting picture from: {filename}");

                        if (whatif == false)
                        {
                            var artPath = Path.Combine(directory, pictureWithTemplate.FilenameTemplate);
                            var reduce = ExtractPictureToFile(pictureWithTemplate.Picture.Data.Data, artPath, null);
                            if (reduce)
                            {
                                trackWithPic.SetReadWrite();
                                RemoveArt(tagFile, pictureWithTemplate.Picture);
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"Unhandled image type {pictureWithTemplate.Picture.MimeType} on {filename}");
                    }
                }
            }
            if (whatif == false)
            {
                trackWithPic.RewriteTags();
            }
        }

        private static bool ExtractPictureToFile(byte[] bytes, string artPathTemplate, int? index)
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

        private static void RemoveArt(TagLib.File tagFile, TagLib.IPicture picture)
        {
            tagFile.Tag.Pictures = tagFile.Tag.Pictures.Where(p => p != picture).ToArray();
            tagFile.Save();
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