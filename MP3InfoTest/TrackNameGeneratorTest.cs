using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Rename;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace MP3InfoTest
{
    [TestClass]
    public class TrackNameGeneratorTest
    {
        [TestMethod]
        public void Test_TrackNameGenerator()
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            string testFileName = @".\temp\testfile.mp3".ToCurrentSystemPathFormat();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);

            var sut = new TrackNameGenerator(fileSystem);
            var result = sut.GetNewName(@".\data\music".ToCurrentSystemPathFormat(), track);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Test_TrackNameGenerator_Directory_Fixes()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var track = new Track(fileSystem)
            {
                AlbumArtist = "W.A.S.P.",
                Album = "W.A.S.P.",
                Title = "Animal",
                Disc = 1,
                TrackNumber = 1,
                Hash = "abc123",
            };

            var sut = new TrackNameGenerator(fileSystem);
            var result = sut.GetNewName(@".\data\music".ToCurrentSystemPathFormat(), track);

            Assert.IsNotNull(result);
            Assert.AreEqual(@".\data\music\W.A.S.P\W.A.S.P\0101 abc123".ToCurrentSystemPathFormat(), result);
        }
    }
}
