﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info.ArtExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3InfoTest.ArtExport
{
    [TestClass]
    public class NonDestructiveFileSaverTest
    {
        [TestMethod]
        public void NonDestructiveFileSaver_No_Existing_File_Test()
        {
            // File.WriteAllText("testfile.txt", "abc123");

            var sut = new NonDestructiveFileSaver();
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(File.Exists("testfile.txt"));
            File.Delete("testfile.txt");
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Different_Contents_File_Test()
        {
            File.WriteAllText("testfile.txt", "xyz789", Encoding.ASCII);

            var sut = new NonDestructiveFileSaver();
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(File.Exists("testfile1.txt"));
            File.Delete("testfile1.txt");
            File.Delete("testfile.txt");
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Different_Length_File_Test()
        {
            File.WriteAllText("testfile.txt", "xyz7890", Encoding.ASCII);

            var sut = new NonDestructiveFileSaver();
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(File.Exists("testfile1.txt"));
            File.Delete("testfile1.txt");
            File.Delete("testfile.txt");
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Existing_Same_File_Test()
        {
            File.WriteAllText("testfile.txt", "abc123", Encoding.ASCII);

            var sut = new NonDestructiveFileSaver();
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsFalse(File.Exists("testfile1.txt"));
            File.Delete("testfile.txt");
        }

        [TestMethod]
        public void NonDestructiveFileSaver_Multiple_Different_File_Test()
        {
            File.WriteAllText("testfile.txt", "xyz7890", Encoding.ASCII);
            File.WriteAllText("testfile1.txt", "xyz7890", Encoding.ASCII);

            var sut = new NonDestructiveFileSaver();
            sut.SaveBytesToFile(Encoding.ASCII.GetBytes("abc123"), (int? i) => $"testfile{i}.txt");

            Assert.IsTrue(File.Exists("testfile2.txt"));
            File.Delete("testfile.txt");
            File.Delete("testfile1.txt");
            File.Delete("testfile2.txt");
        }
    }
}