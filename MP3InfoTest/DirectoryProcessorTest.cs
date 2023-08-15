using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            const string Filename = TestTracks.TEST_MP3_NAME;

            string testFileName = @".\temp\testfile.mp3".ToCurrentSystemPathFormat();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackListProcessor = A.Fake<ITrackListProcessor>();

            var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@".\temp\".ToCurrentSystemPathFormat(), trackListProcessor, whatif);

            A.CallTo(() => trackListProcessor.ProcessTracks(A<List<Track>>._, @".\temp".ToCurrentSystemPathFormat())).MustHaveHappened();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DirectoryProcessor_Missing_Directory_Test(bool whatif)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

			var trackListProcessor = A.Fake<ITrackListProcessor>();

			var sut = new DirectoryProcessor(fileSystem);

            var result = sut.ProcessList(@".\random\".ToCurrentSystemPathFormat(), trackListProcessor, whatif);

			A.CallTo(() => trackListProcessor.ProcessTracks(A<List<Track>>._, @".\random".ToCurrentSystemPathFormat())).MustNotHaveHappened();
		}
	}
}
