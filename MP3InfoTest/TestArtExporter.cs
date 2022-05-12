using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MP3InfoTest
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

            var originalBytes = File.ReadAllBytes(testFilename);

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var exporter = new ArtExporter(false);

            exporter.ProcessTracks(track, ".");

            File.Delete(testFilename);
            Assert.IsTrue(File.Exists("folder.jpg"));
            File.Delete("folder.jpg");
        }
    }
}
