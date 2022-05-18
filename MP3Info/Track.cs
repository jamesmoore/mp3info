using MP3Info.Normalise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TagLib.Id3v2;

namespace MP3Info
{
    public class Track
    {
        public Track()
        {

        }

        public Track(string filename)
        {
            this.LoadFromFile(filename);
        }

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
        public string Hash { get; set; }

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

        internal void LoadFromFile(string filename)
        {
            var fileInfo = new FileInfo(filename);
            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                RefreshTags(file);
                LastUpdated = fileInfo.LastWriteTime;
                Filename = fileInfo.FullName;
            }
        }

        private void RefreshTags(TagLib.File file)
        {
            AlbumArtist = file.Tag.JoinedAlbumArtists;
            Artist = file.Tag.JoinedPerformers;
            Year = file.Tag.Year;
            Album = file.Tag.Album;
            Disc = file.Tag.Disc;
            DiscCount = file.Tag.DiscCount;
            TrackNumber = file.Tag.Track;
            TrackCount = file.Tag.TrackCount;
            Title = file.Tag.Title;
            Pictures = file.Tag.Pictures.Count();
            Comment = file.Tag.Comment;
            TagTypes = file.TagTypesOnDisk;

            var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);

            var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();

            Hash = hashTextFields.FirstOrDefault()?.Text.FirstOrDefault();
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
            return this.GetHashInBase64() == this.Hash;
        }

        public void WriteHash()
        {
            var hash = this.GetHashInBase64();
            using (var tagFile = TagLib.File.Create(this.Filename))
            {
                var custom = (TagLib.Id3v2.Tag)tagFile.GetTag(TagLib.TagTypes.Id3v2);

                var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();
                foreach (var frame in hashTextFields)
                {
                    custom.RemoveFrame(frame);
                }

                var newHash = new UserTextInformationFrame("hash")
                {
                    Text = new string[] { hash }
                };
                custom.AddFrame(newHash);

                tagFile.RemoveTags(TagLib.TagTypes.Id3v1);
                this.SetReadWrite();
                tagFile.Save();
                this.Hash = hash;
            }
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

        public void Normalise(bool whatif)
        {
            var normalisers = new List<INormaliseTrack>()
            {
                new MissingArtistNormalise(),
                new ID3v1Normalise(),
                new DiskFixNormalise(),
            };

            var eligible = normalisers.Where(p => p.CanBeNormalised(this)).ToList();

            if (eligible.Any())
            {
                using (var file = TagLib.File.Create(this.Filename))
                {
                    foreach (var item in eligible)
                    {
                        item.Normalise(file);
                    }
                    RefreshTags(file);
                    if (whatif == false)
                    {
                        file.Save();
                    }
                }
            }
        }

        public bool HasLegitBase64Hash()
        {
            try
            {
                return string.IsNullOrEmpty(this.Hash) == false && Convert.FromBase64String(this.Hash.Replace("-", "/")).Length == 32;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetExpectedFilename()
        {
            var expectedFilename = $"{this.Disc:00}{this.TrackNumber:00} {this.Hash}{Path.GetExtension(this.Filename)}";
            return expectedFilename;
        }
    }
}