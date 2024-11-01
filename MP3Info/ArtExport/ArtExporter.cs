using NLog;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace MP3Info.ArtExport
{
    public class ArtExporter(IFileSystem fileSystem, bool whatif) : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly NonDestructiveFileSaver nonDestructiveFileSaver = new NonDestructiveFileSaver(fileSystem);

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
            var directory = fileSystem.Path.GetDirectoryName(filename);

            var mimeFileMap = new Dictionary<string, string>()
            {
                { "image/jpeg",  "folder{0}.jpg"},
                { "image/jpg",  "folder{0}.jpg"},
                { "image/png",  "folder{0}.png"},
                { "image/gif",  "folder{0}.gif"},
                { "binary",  "folder{0}.bin"},
            }.ToLookup(p => p.Key, p => p.Value);

            using (var tagFile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, filename)))
            {
                var pictures = tagFile.Tag.Pictures.ToList();

                var picturesWithTemplate = pictures.Select(p =>
                    new
                    {
                        Picture = p,
                        PicureBytes = p.Data.Data,
                        FilenameTemplate = mimeFileMap[p.MimeType.ToLower()].FirstOrDefault(),
                    }).ToList();

                foreach (var pictureWithTemplate in picturesWithTemplate)
                {
                    if (pictureWithTemplate.FilenameTemplate != null)
                    {
                        logger.Info($"Exporting picture from: {filename}");

                        if (whatif == false)
                        {
                            var artPath = fileSystem.Path.Combine(directory, pictureWithTemplate.FilenameTemplate);
                            var reduce = nonDestructiveFileSaver.SaveBytesToFile(
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