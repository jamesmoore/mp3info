using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MP3Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            const string testFileName = @"c:\temp\testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackListProcessor = new Mock<ITrackListProcessor>();

            var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@"c:\temp\", trackListProcessor.Object, whatif);

            trackListProcessor.Verify(p => p.ProcessTracks(It.IsAny<IEnumerable<Track>>(), @"c:\temp"), Times.Once);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DirectoryProcessor_Missing_Directory_Test(bool whatif)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            var trackListProcessor = new Mock<ITrackListProcessor>();

            var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@"c:\random\", trackListProcessor.Object, whatif);

            trackListProcessor.Verify(p => p.ProcessTracks(It.IsAny<IEnumerable<Track>>(), It.IsAny<string>()), Times.Never);
        }
    }
}
