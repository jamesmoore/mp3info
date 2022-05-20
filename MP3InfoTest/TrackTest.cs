using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using TagLib.Id3v2;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackTest
    {
        [TestMethod]
        public void TagRewrite_Test()
        {
            const string Filename = "xenon-sentry.mp3";

            const string testFileName = @"c:\temp\testfile.mp3";
            const string originalFileName = @"c:\temp\originalfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { originalFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            fileSystem.File.Copy(originalFileName, testFileName);

            var trackLoader = new TrackLoader(fileSystem);
            var testTrack = trackLoader.GetTrack(testFileName);

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                var firstpic = tempfile.Tag.Pictures.First();
                tempfile.Tag.Pictures = tempfile.Tag.Pictures.Where(p => p != firstpic).ToArray();
                tempfile.Save();
            }

            var removePicBytes = fileSystem.File.ReadAllBytes(testFileName);

            testTrack.RewriteTags();

            Assert.IsTrue(fileSystem.FileInfo.FromFileName(testFileName).Length < fileSystem.FileInfo.FromFileName(originalFileName).Length);

            using (var originalFile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, originalFileName)))
            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                Assert.AreEqual(originalFile.Tag.Title, tempfile.Tag.Title);
                Assert.AreEqual(originalFile.Tag.Album, tempfile.Tag.Album);
                Assert.AreEqual(originalFile.Tag.JoinedPerformers, tempfile.Tag.JoinedPerformers);
                Assert.AreEqual(originalFile.Tag.Track, tempfile.Tag.Track);

                Assert.AreEqual(originalFile.Properties.Duration, tempfile.Properties.Duration);
                Assert.AreEqual(originalFile.Properties.AudioBitrate, tempfile.Properties.AudioBitrate);
            }
        }

        [TestMethod]
        public void TagRewrite_Preserves_Hash_Test()
        {
            const string Filename = "xenon-sentry.mp3";

            const string testFileName = @"c:\temp\testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var trackTrack = trackLoader.GetTrack(testFileName);

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                var hashTextFields = GetHashTextField(tempfile);
                Assert.IsNull(hashTextFields);
            }

            var trackHashWriter = new TrackHashWriter(false, false);
            trackHashWriter.ProcessTrack(trackTrack, ".");

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                var hashTextFields = GetHashTextField(tempfile);
                Assert.IsNotNull(hashTextFields);
            }

            trackTrack.RewriteTags();

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                var hashTextFields = GetHashTextField(tempfile);
                Assert.IsNotNull(hashTextFields);
            }
        }

        private static UserTextInformationFrame GetHashTextField(TagLib.File tempfile)
        {
            var custom = tempfile.GetId3v2Tag();
            var hashTextFields = custom.GetUserTextInformationFrames().Where(p => p.Description == "hash").FirstOrDefault();
            return hashTextFields;
        }

        [DataTestMethod]
        [DataRow(FileAttributes.ReadOnly)]
        [DataRow(FileAttributes.Normal)]
        public void SetReadWrite_Test(FileAttributes existing)
        {
            const string Filename = "xenon-sentry.mp3";

            const string originalFileName = @"c:\temp\originalfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { originalFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            fileSystem.File.SetAttributes(originalFileName, existing);

            Assert.AreEqual(existing, GetAttributes(fileSystem, originalFileName));

            var trackLoader = new TrackLoader(fileSystem);
            var testTrack = trackLoader.GetTrack(originalFileName);

            testTrack.SetReadWrite();

            Assert.AreNotEqual(FileAttributes.ReadOnly, GetAttributes(fileSystem, originalFileName) & FileAttributes.ReadOnly);
        }

        private static FileAttributes GetAttributes(IFileSystem fileSystem, string originalFileName)
        {
            return fileSystem.FileInfo.FromFileName(originalFileName).Attributes;
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Normalize_Test(bool whatif)
        {
            const string Filename = "xenon-sentry.mp3";

            const string originalFileName = @"c:\temp\originalfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { originalFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var testTrack = trackLoader.GetTrack(originalFileName);

            Assert.AreEqual((UInt32)0, testTrack.Disc);
            Assert.IsTrue(string.IsNullOrWhiteSpace(testTrack.AlbumArtist));

            testTrack.Normalise(whatif);

            var reloadedTestTrack = trackLoader.GetTrack(originalFileName);
            if (whatif == false)
            {
                Assert.AreEqual((UInt32)1, reloadedTestTrack.Disc);
                Assert.IsFalse(string.IsNullOrWhiteSpace(reloadedTestTrack.AlbumArtist));
            }
            else
            {
                Assert.AreEqual((UInt32)0, reloadedTestTrack.Disc);
                Assert.IsTrue(string.IsNullOrWhiteSpace(reloadedTestTrack.AlbumArtist));
            }
        }

    }
}
