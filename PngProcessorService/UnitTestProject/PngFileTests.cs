using Microsoft.VisualStudio.TestTools.UnitTesting;
using PngProcessorService;
using PngProcessorService.Models;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class PngFileTests
    {
        /// <summary>
        /// Тест на инициализацию PngFile.
        /// </summary>
        [TestMethod]
        public void Init()
        {
            using (var testDirectory = new TestDirectory())
            {
                var pngFile = new PngFile(testDirectory.DirectoryPath, new byte[] { 1, 2, 3 });

                Assert.IsFalse(string.IsNullOrEmpty(pngFile.Id));
                Assert.AreEqual(0, pngFile.Progress);
            }
        }

        /// <summary>
        /// Тест на получение исключения ProcessIsNotRunningException при попытке отменить процесс обработки, которого нет.
        /// </summary>
        [TestMethod]
        public void ProcessIsNotRunningException()
        {
            using (var testDirectory = new TestDirectory())
            {
                var pngFile = new PngFile(testDirectory.DirectoryPath, new byte[] { 1, 2, 3 });
                try
                {
                    pngFile.CancelProcess();
                    Assert.Fail($"Должно было возникнуть исключение ProcessIsNotRunningException.");
                }
                catch (ProcessIsNotRunningException) { }
                catch(Exception exc)
                {
                    Assert.Fail($"Должно было возникнуть исключение ProcessIsNotRunningException, а не {exc.Message}.");
                }
            }
        }

        // Если бы у нас был интерфейс от PngProcessor и мы бы работали через него, то можно было бы сделать фэйковый PngProcessor и добавть ещё тесты:
        // на обработку файла, получение прогресса, отмену обработки. А так как нет возможности управлять методом Process класса PngProcessor, то такие тесты будут не надёжными.
        // Или если бы у меня была VisualStudio Professional, можно было создать фэйковую сборку для ImageProcessor и для тестов использовать её методы.
    }
}