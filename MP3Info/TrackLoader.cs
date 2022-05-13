using System;
using System.IO;
using System.Linq;
using NLog;
namespace MP3Info
{
    public class TrackLoader : ITrackLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Track GetTrack(string filename)
        {
            logger.Info($"Indexing: {filename}");

            try
            {
                return BuildTrack(filename);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception reading file {filename}");
                return null;
            }
        }

        private static Track BuildTrack(string filename)
        {
            var fileInfo = new FileInfo(filename);

            using (var file = TagLib.File.Create(filename))
            {
                var track = new Track()
                {
                    AlbumArtist = file.Tag.JoinedAlbumArtists,
                    Artist = file.Tag.JoinedPerformers,
                    Year = file.Tag.Year,
                    Album = file.Tag.Album,
                    Disc = file.Tag.Disc,
                    DiscCount = file.Tag.DiscCount,
                    TrackNumber = file.Tag.Track,
                    TrackCount = file.Tag.TrackCount,
                    Title = file.Tag.Title,
                    Pictures = file.Tag.Pictures.Count(),
                    LastUpdated = fileInfo.LastWriteTime,
                    Filename = filename,
                    Comment = file.Tag.Comment,
                    TagTypes = file.TagTypesOnDisk,
                };
                return track;
            }
        }
    }
}
