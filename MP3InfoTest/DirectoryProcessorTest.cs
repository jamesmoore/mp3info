using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MP3Info;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace MP3InfoTest
{
    [TestClass]
    public class DirectoryProcessorTest
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DirectoryProcessor_Test(bool whatif)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            string testFileName = @".\temp\testfile.mp3".ToCurrentSystemPathFormat();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackListProcessor = new Mock<ITrackListProcessor>();

            var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@".\temp\".ToCurrentSystemPathFormat(), trackListProcessor.Object, whatif);

            trackListProcessor.Verify(p => p.ProcessTracks(It.IsAny<IEnumerable<Track>>(), @".\temp".ToCurrentSystemPathFormat()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DirectoryProcessor_Missing_Directory_Test(bool whatif)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            var trackListProcessor = new Mock<ITrackListProcessor>();

            var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@".\random\".ToCurrentSystemPathFormat(), trackListProcessor.Object, whatif);

            trackListProcessor.Verify(p => p.ProcessTracks(It.IsAny<IEnumerable<Track>>(), It.IsAny<string>()), Times.Never);
        }
    }
}
