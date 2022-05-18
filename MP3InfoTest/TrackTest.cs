using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackTest
    {
        [TestMethod]
        public void TestTagRewrite()
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
            var track = trackLoader.GetTrack(testFileName);

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                var firstpic = tempfile.Tag.Pictures.First();
                tempfile.Tag.Pictures = tempfile.Tag.Pictures.Where(p => p != firstpic).ToArray();
                tempfile.Save();
            }

            var removePicBytes = fileSystem.File.ReadAllBytes(testFileName);

            track.RewriteTags();

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
    }
}
