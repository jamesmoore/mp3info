﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Rename;
using System;
using System.IO;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackNameGeneratorTest
    {
        [TestMethod]
        public void Test_TrackNameGenerator()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var sut = new TrackNameGenerator();
            var result = sut.GetNewName(".", track);
            Assert.IsNotNull(result);
            File.Delete(testFilename);
        }

        [TestMethod]
        public void Test_TrackNameGenerator_Directory_Fixes()
        {
            var track = new Track()
            {
                AlbumArtist = "W.A.S.P.",
                Album = "W.A.S.P.",
                Title = "Animal",
                Disc = 1,
                TrackNumber = 1,
            };

            var sut = new TrackNameGenerator();
            var result = sut.GetNewName(".", track);

            Assert.IsNotNull(result);
            Assert.AreEqual(@".\W.A.S.P\W.A.S.P\0101 ", result);
        }

    }
}