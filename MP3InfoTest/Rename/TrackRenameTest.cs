using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using MP3Info.Rename;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace MP3InfoTest.Rename
{
    [TestClass]
    public class TrackRenameTest
    {
        private const string expectedDestination = @".\Musick's Recreation. Milena Cord-to-Krax\Una Reverencia a Bach\0001 qfBRTb9LheSXVPw2QBB7bgY7k4GlDjsHPl48C7jFfqU=.mp3";

        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public void TrackRenamer_Test(bool whatif, bool expectFileMovedToDestination)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            string testFileName = @".\temp\testfile.mp3".ToCurrentSystemPathFormat();
            string testPictureName = @".\temp\albumcover.jpg".ToCurrentSystemPathFormat();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
                { testPictureName, new MockFileData(Guid.NewGuid().ToByteArray()) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);

            var hashWriter = new TrackHashWriter(false, false);

            hashWriter.ProcessTrack(track, null);

            var sut = new TrackRenamer(fileSystem, whatif);

            sut.ProcessTrack(track, ".");

            Assert.AreEqual(expectFileMovedToDestination, fileSystem.File.Exists(expectedDestination.ToCurrentSystemPathFormat()));
            Assert.AreEqual(expectFileMovedToDestination, fileSystem.File.Exists(fileSystem.Path.Combine(
                fileSystem.FileInfo.FromFileName(expectedDestination.ToCurrentSystemPathFormat()).DirectoryName,
                "albumcover.jpg"
                )));

            Assert.AreNotEqual(expectFileMovedToDestination, fileSystem.File.Exists(testFileName));
            Assert.AreNotEqual(expectFileMovedToDestination, fileSystem.File.Exists(testPictureName));
        }
    }
}
