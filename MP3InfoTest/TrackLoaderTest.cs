using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackLoaderTest
    {
        [TestMethod]
        public void Test_TrackLoader()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            const string testFileName = @"c:\temp\testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);

            Assert.IsNotNull(track);

            Assert.AreEqual("Una Reverencia a Bach", track.Album);
            Assert.AreEqual("Musick's Recreation. Milena Cord-to-Krax", track.Artist);
            Assert.AreEqual("Musick's Recreation. Milena Cord-to-Krax", track.AlbumArtist);
            Assert.AreEqual("Prelude — Tres viste (BWV 995)", track.Title);
            Assert.AreEqual((UInt32)1, track.TrackNumber);
        }
    }
}
