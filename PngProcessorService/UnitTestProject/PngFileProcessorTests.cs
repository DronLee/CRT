﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PngProcessorService.Models;
using PngProcessorService;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class PngFileProcessorTests
    {
        [TestMethod]
        public void ProcessFile_PoolOverflow()
        {
            var pngFileProcessor = new PngFileProcessor(new TestFileFactory(), 2);

            var fileId1 = pngFileProcessor.AddFile(null);
            var fileId2 = pngFileProcessor.AddFile(null);
            var fileId3 = pngFileProcessor.AddFile(null);

            pngFileProcessor.ProcessFile(fileId1);
            pngFileProcessor.ProcessFile(fileId2);
            pngFileProcessor.ProcessFile(fileId3);

            // Первые 2 файла должны обрабатываться, а третий зависнуть в очереди, так как пул всего на 2.
            Assert.IsTrue(TestFile.ProcessFilesId.Contains(fileId1));
            Assert.IsTrue(TestFile.ProcessFilesId.Contains(fileId2));
            Assert.IsFalse(TestFile.ProcessFilesId.Contains(fileId3));
        }

        private class TestFileFactory : IFileFactory
        {
            public IFile CreateFile(byte[] content)
            {
                return new TestFile();
            }
        }

        private class TestFile : IFile
        {
            public static readonly List<string> ProcessFilesId = new List<string>();

            public TestFile()
            {
                Id = Guid.NewGuid().ToString();
            }

            public string Id { get; private set; }

            public double Progress
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public event Action ProcessedEvent;

            public void CancelProcess()
            {
                throw new NotImplementedException();
            }

            public void Process()
            {
                ProcessFilesId.Add(Id);
            }
        }
    }
}