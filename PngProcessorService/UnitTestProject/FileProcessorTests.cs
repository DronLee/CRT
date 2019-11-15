using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PngProcessorService.Models;
using PngProcessorService;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class FileProcessorTests
    {
        /// <summary>
        /// Тестирование переполнения пула обрабатываемых файлов.
        /// </summary>
        [TestMethod]
        public void ProcessFile_PoolOverflow()
        {
            var fileProcessor = new FileProcessor(new TestFileFactory(), 2);

            var fileId1 = fileProcessor.AddFile(null);
            var fileId2 = fileProcessor.AddFile(null);
            var fileId3 = fileProcessor.AddFile(null);

            fileProcessor.ProcessFile(fileId1);
            fileProcessor.ProcessFile(fileId2);
            fileProcessor.ProcessFile(fileId3);

            // Первые 2 файла должны обрабатываться, а третий зависнуть в очереди, так как пул всего на 2.
            Assert.IsTrue(TestFile.ProcessFilesId.Contains(fileId1));
            Assert.IsTrue(TestFile.ProcessFilesId.Contains(fileId2));
            Assert.IsFalse(TestFile.ProcessFilesId.Contains(fileId3));
        }

        /// <summary>
        /// Тестирование отмены обработки и взятие при этом в обработку следующего файла из пула ожиданий.
        /// </summary>
        [TestMethod]
        public void CancelProcess_NextFileToProcess()
        {
            var fileProcessor = new FileProcessor(new TestFileFactory(), 2);

            var fileId1 = fileProcessor.AddFile(null);
            var fileId2 = fileProcessor.AddFile(null);
            var fileId3 = fileProcessor.AddFile(null);

            fileProcessor.ProcessFile(fileId1);
            fileProcessor.ProcessFile(fileId2);
            fileProcessor.ProcessFile(fileId3);

            fileProcessor.CancelProcess(fileId2);

            Assert.IsTrue(TestFile.CanceledProcessFilesId.Contains(fileId2));
            Assert.IsTrue(TestFile.ProcessFilesId.Contains(fileId3), 
                "После отмены обработки второго файла, в обработку должен поступить третий.");
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
            public static readonly List<string> CanceledProcessFilesId = new List<string>();

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

            public event Action<IFile> ProcessedEvent;

            public void CancelProcess()
            {
                CanceledProcessFilesId.Add(Id);
            }

            public void Process()
            {
                ProcessFilesId.Add(Id);
            }
        }
    }
}