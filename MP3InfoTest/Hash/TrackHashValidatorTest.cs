using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace MP3InfoTest.Hash
{
    [TestClass]
    public class TrackHashValidatorTest
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TrackHashValidator_No_Hash_Test(bool verbose)
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            const string testFileName = @"testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);

            var sut = new TrackHashValidator(verbose);

            sut.ProcessTracks(new List<Track>() { track }, ".");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TrackHashValidator_BadlyFormatted_Hash_Test(bool verbose)
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            const string testFileName = @"testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);
            track.Hash = "ABC123";
            var sut = new TrackHashValidator(verbose);

            sut.ProcessTracks(new List<Track>() { track }, ".");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TrackHashValidator_Valid_Hash_Test(bool verbose)
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            const string testFileName = @"testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);
            track.WriteHash();
            var sut = new TrackHashValidator(verbose);

            sut.ProcessTracks(new List<Track>() { track }, ".");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TrackHashValidator_Invalid_Hash_Test(bool verbose)
        {
            const string Filename = TestTracks.TEST_MP3_NAME;

            const string testFileName = @"testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(testFileName);
            track.WriteHash();
            track.Hash = track.Hash.ToLower();
            var sut = new TrackHashValidator(verbose);

            sut.ProcessTracks(new List<Track>() { track }, ".");
        }
    }
}
