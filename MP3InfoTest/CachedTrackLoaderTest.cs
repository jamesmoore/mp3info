using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace MP3InfoTest
{
    [TestClass]
    public class CachedTrackLoaderTest
    {
        [TestMethod]
        public void CachedTrackLoader_Test()
        {
            const string testFileName = @"c:\temp\testfile.mp3";
            var mockFileData = new MockFileData("xyz789")
            {
                LastWriteTime = System.DateTimeOffset.Now.AddDays(-1)
            };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { testFileName, mockFileData },
            });

            var trackLoader = A.Fake<ITrackLoader>();
            A.CallTo(() => trackLoader.GetTrack(testFileName)).Returns(new Track(fileSystem)
            {
                Album = "Test album",
                AlbumArtist = "Test album artist",
                Artist = "Test artist",
                TrackNumber = 1,
                LastUpdated = mockFileData.LastWriteTime.DateTime,
                Filename = testFileName,
            });

            var sut = new CachedTrackLoader(fileSystem, trackLoader, false);

            var track = sut.GetTrack(testFileName);

            sut.Dispose();
            Assert.IsNotNull(track);

            string expectedPath = $"data{fileSystem.Path.DirectorySeparatorChar}cache.json";
            Assert.IsTrue(fileSystem.FileExists(expectedPath));
            var cachejson = fileSystem.File.ReadAllText(expectedPath);
            Assert.IsFalse(string.IsNullOrWhiteSpace(cachejson));

			A.CallTo(() => trackLoader.GetTrack(testFileName)).MustHaveHappened(1, Times.Exactly);

            var sut2 = new CachedTrackLoader(fileSystem, trackLoader, false);
            var track2 = sut2.GetTrack(testFileName);
            Assert.IsNotNull(track2);

			A.CallTo(() => trackLoader.GetTrack(testFileName)).MustHaveHappened(2, Times.Exactly);
		}
	}
}
