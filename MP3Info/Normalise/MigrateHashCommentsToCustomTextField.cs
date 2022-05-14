using NLog;
using System;
using System.Linq;
using TagLib;
using TagLib.Id3v2;

namespace MP3Info.Normalise
{
    internal class MigrateHashCommentsToCustomTextField : INormaliseTrack
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool CanBeNormalised(Track track)
        {
            return 
                string.IsNullOrEmpty(track.Hash) &&
                string.IsNullOrEmpty(track.Comment) == false && 
                Convert.FromBase64String(track.Comment.Replace("-", "/")).Length == 32;
        }

        public void Normalise(File file)
        {
            var custom = (TagLib.Id3v2.Tag)file.GetTag(TagTypes.Id3v2);

            var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();

            if (hashTextFields.Any())
            {
                var hash = hashTextFields.First().Text.First();
                if (hash == custom.Comment)
                {
                    logger.Info($"Removing matching hash comments field on {file.Name}");
                    custom.Comment = null;
                }
                else
                {
                    logger.Error($"Comments does not match hash field on {file.Name}");
                }
            }
        }
    }
}
