using System;
using System.IO;

namespace UnitTestProject
{
    internal class TestDirectory : IDisposable
    {
        public readonly string DirectoryPath;

        public TestDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }
    }
}