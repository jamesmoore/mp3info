using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.ArtExport;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace MP3InfoTest.ArtExport
{
    [TestClass]
    public class TestArtExporter
    {
        [TestMethod]
        public void TestArtExport()
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            const string testFileName = @"testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);

            var exporter = new ArtExporter(fileSystem, false);

            Assert.IsFalse(fileSystem.File.Exists("folder.jpg"));

            exporter.ProcessTrack(track, ".");

            using (var tempfile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, testFileName)))
            {
                Assert.IsFalse(tempfile.Tag.Pictures.Any());
            }

            Assert.IsTrue(fileSystem.File.Exists("folder.jpg"));
        }
    }
}
