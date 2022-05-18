using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.ArtExport;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace MP3InfoTest.ArtExport
{
    [TestClass]
    public class TestArtExporter
    {
        [TestMethod]
        public void TestArtExport()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var trackLoader = new TrackLoader(new FileSystem());
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var exporter = new ArtExporter(new FileSystem(), false);

            Assert.IsFalse(File.Exists("folder.jpg"));

            exporter.ProcessTrack(track, ".");

            using (var tempfile = TagLib.File.Create(testFilename))
            {
                Assert.IsFalse(tempfile.Tag.Pictures.Any());
            }

            File.Delete(testFilename);
            Assert.IsTrue(File.Exists("folder.jpg"));
            File.Delete("folder.jpg");
        }
    }
}
