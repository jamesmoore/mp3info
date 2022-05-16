using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MP3Info.ArtExport
{
    public class ArtExporter : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;
        private readonly NonDestructiveFileSaver nonDestructiveFileSaver;
        public ArtExporter(bool whatif)
        {
            nonDestructiveFileSaver = new NonDestructiveFileSaver();
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
                        PicureBytes = p.Data.Data,
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
                            var reduce = nonDestructiveFileSaver.ExtractPictureToFile(
                                pictureWithTemplate.PicureBytes,
                                (int? i) => string.Format(artPath, i)
                                );
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

        private static void RemoveArt(TagLib.File tagFile, TagLib.IPicture picture)
        {
            tagFile.Tag.Pictures = tagFile.Tag.Pictures.Where(p => p != picture).ToArray();
            tagFile.Save();
        }
    }
}