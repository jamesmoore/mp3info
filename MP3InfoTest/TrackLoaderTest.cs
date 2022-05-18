using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System;
using System.IO;
using System.IO.Abstractions;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackLoaderTest
    {
        [TestMethod]
        public void Test_TrackLoader()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var trackLoader = new TrackLoader(new FileSystem());
            var track = trackLoader.GetTrack(fileInfo.FullName);

            Assert.IsNotNull(track);

            Assert.AreEqual("Una Reverencia a Bach", track.Album);
            Assert.AreEqual("Musick's Recreation. Milena Cord-to-Krax", track.Artist);
            Assert.AreEqual("Musick's Recreation. Milena Cord-to-Krax", track.AlbumArtist);
            Assert.AreEqual("Prelude — Tres viste (BWV 995)", track.Title);
            Assert.AreEqual((UInt32)1, track.TrackNumber);
            File.Delete(testFilename);
        }
    }
}
