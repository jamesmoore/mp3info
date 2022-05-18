using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info.ArtExport;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace MP3InfoTest.ArtExport
{
    [TestClass]
    public class NonDestructiveFileSaverTest
    {
        [TestMethod]
        public void NonDestructiveFileSaver_No_Existing_File_Test()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            });

            var sut = new NonDestructiveFileSaver(fileSystem);
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(fileSystem.File.Exists("testfile.txt"));
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Different_Contents_File_Test()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"testfile.txt", new MockFileData("xyz789", Encoding.ASCII) },
            });

            var sut = new NonDestructiveFileSaver(fileSystem);
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(fileSystem.File.Exists("testfile1.txt"));
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Different_Length_File_Test()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"testfile.txt", new MockFileData("xyz7890", Encoding.ASCII) },
            });

            var sut = new NonDestructiveFileSaver(fileSystem);
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(fileSystem.File.Exists("testfile1.txt"));
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Same_File_Test()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"testfile.txt", new MockFileData("abc123", Encoding.ASCII) },
            });

            var sut = new NonDestructiveFileSaver(fileSystem);
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsFalse(fileSystem.File.Exists("testfile1.txt"));
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Multiple_Different_File_Test()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"testfile.txt", new MockFileData("xyz7890", Encoding.ASCII) },
                { @"testfile1.txt", new MockFileData("xyz78901", Encoding.ASCII) },
            });

            var sut = new NonDestructiveFileSaver(fileSystem);
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(fileSystem.File.Exists("testfile2.txt"));
        }
    }
}
