using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System;
using System.IO;
using System.IO.Abstractions;
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

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var originalBytes = File.ReadAllBytes(testFilename);

            var trackLoader = new TrackLoader(new FileSystem());
            var track = trackLoader.GetTrack(testFilename);

            using (var tempfile = TagLib.File.Create(testFilename))
            {
                var firstpic = tempfile.Tag.Pictures.First();
                tempfile.Tag.Pictures = tempfile.Tag.Pictures.Where(p => p != firstpic).ToArray();
                tempfile.Save();
            }

            var removePicBytes = File.ReadAllBytes(testFilename);

            track.RewriteTags();

            var newBytes = File.ReadAllBytes(testFilename);

            Assert.IsTrue(newBytes.Length < originalBytes.Length);

            using (var originalFile = TagLib.File.Create(Filename))
            using (var tempfile = TagLib.File.Create(testFilename))
            {
                Assert.AreEqual(originalFile.Tag.Title, tempfile.Tag.Title);
                Assert.AreEqual(originalFile.Tag.Album, tempfile.Tag.Album);
                Assert.AreEqual(originalFile.Tag.JoinedPerformers, tempfile.Tag.JoinedPerformers);
                Assert.AreEqual(originalFile.Tag.Track, tempfile.Tag.Track);

                Assert.AreEqual(originalFile.Properties.Duration, tempfile.Properties.Duration);
                Assert.AreEqual(originalFile.Properties.AudioBitrate, tempfile.Properties.AudioBitrate);
            }

            File.Delete(testFilename);
        }
    }
}
