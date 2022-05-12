using NLog;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MP3Info
{
    public class Track
    {
        public string AlbumArtist { get; set; }
        public string Artist { get; set; }
        public uint Year { get; set; }
        public string Album { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
        public uint TrackNumber { get; set; }
        public uint TrackCount { get; set; }
        public string Title { get; set; }
        public int Pictures { get; set; }
        public string Filename { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Comment { get; set; }

        public TagLib.TagTypes TagTypes { get; set; }

        public void Move(string newFullPath)
        {
            if (File.Exists(newFullPath))
            {
                throw new InvalidOperationException($"Attempt to overwrite file at: {newFullPath}");
            }

            var newPath = Path.GetDirectoryName(newFullPath);

            if (Directory.Exists(newPath) == false)
            {
                Directory.CreateDirectory(newPath);
            }

            File.Move(this.Filename, newFullPath, false);
            this.Filename = newFullPath;
        }

        public string GetDirectory()
        {
            return Path.GetDirectoryName(this.Filename);
        }

        private string GetHashInBase64()
        {
            var originalBytes = File.ReadAllBytes(this.Filename);
            using (var ms = new MemoryStream(originalBytes))
            {
                var fakeFile = new FileBytesAbstraction(this.Filename, ms);

                using (var tagFile = TagLib.File.Create(fakeFile))
                {
                    tagFile.RemoveTags(TagLib.TagTypes.AllTags);
                    tagFile.Save();
                }

                ms.Position = 0;
                var hash = Convert.ToBase64String(GetHashSha256(ms)).Replace("/", "-");
                return hash;
            }
        }

        public bool TrackHasValidHash()
        {
            return this.GetHashInBase64() == this.Comment;
        }

        public void WriteHash()
        {
            var hash = this.GetHashInBase64();
            using (var tagFile = TagLib.File.Create(this.Filename))
            {
                tagFile.Tag.Comment = hash;
                tagFile.RemoveTags(TagLib.TagTypes.Id3v1);
                this.SetReadWrite();
                tagFile.Save();
            }
            this.Comment = hash;
        }

        private byte[] GetHashSha256(MemoryStream ms)
        {
            using (var sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(ms);
            }
        }

        public void RewriteTags()
        {
            var bytes = File.ReadAllBytes(this.Filename);
            var backupTag = new TagLib.Id3v2.Tag();

            using (var ms = new MemoryStream(bytes))
            {
                using (var tagFileToClear = TagLib.File.Create(new FileBytesAbstraction(this.Filename, ms)))
                {
                    tagFileToClear.Tag.CopyTo(backupTag, true);
                    tagFileToClear.RemoveTags(TagLib.TagTypes.AllTags);
                    tagFileToClear.Save();
                }

                ms.Position = 0;
                bytes = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);

                using (var tagFileRestore = TagLib.File.Create(new FileBytesAbstraction(this.Filename, ms)))
                {
                    tagFileRestore.RemoveTags(TagLib.TagTypes.Id3v1);
                    backupTag.CopyTo(tagFileRestore.Tag, true);
                    tagFileRestore.Save();
                }

                ms.Position = 0;
                File.WriteAllBytes(this.Filename, ms.ToArray());
            }
        }

        public void SetReadWrite()
        {
            var attr = File.GetAttributes(this.Filename);
            if (attr.HasFlag(FileAttributes.ReadOnly))
            {
                attr = attr & ~FileAttributes.ReadOnly;
                File.SetAttributes(this.Filename, attr);
            }
        }

        public void Normalise(ILogger logger, bool whatif)
        {
            bool save = false;

            var missingArtist = string.IsNullOrWhiteSpace(this.AlbumArtist);
            var id3v1 = (this.TagTypes & TagLib.TagTypes.Id3v1) == TagLib.TagTypes.Id3v1;
            var disk0 = this.Disc == 0;

            if (missingArtist || id3v1 || disk0)
            {
                using (var file = TagLib.File.Create(this.Filename))
                {

                    if (missingArtist)
                    {
                        logger.Info($"Fixing album artists on {file.Name}");
                        file.Tag.AlbumArtists = (string[])file.Tag.Performers.Clone();
                        this.AlbumArtist = file.Tag.JoinedAlbumArtists;
                        save = true;
                    }

                    if (disk0)
                    {
                        logger.Info($"Fixing disc on {file.Name}");
                        file.Tag.Disc = 1;
                        this.Disc = 1;
                        save = true;
                    }

                    if (id3v1)
                    {
                        logger.Info($"Removing ID3v1 on {file.Name}");
                        file.RemoveTags(TagLib.TagTypes.Id3v1);
                        save = true;
                    }

                    if (save && whatif == false)
                    {
                        this.SetReadWrite();
                        file.Save();
                    }
                }
            }
        }

        public bool HasLegitBase64Hash()
        {
            try
            {
                return Convert.FromBase64String(this.Comment.Replace("-", "/")).Length == 32;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetExpectedFilename()
        {
            var expectedFilename = $"{this.Disc:00}{this.TrackNumber:00} {this.Comment}{Path.GetExtension(this.Filename)}";
            return expectedFilename;
        }
    }
}