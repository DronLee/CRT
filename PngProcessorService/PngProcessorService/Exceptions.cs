using System;

namespace PngProcessorService
{
    public class ProcessIsAlreadyRunningException : Exception { }
    public class FileIsAlreadyProcessedException : Exception { }
    public class ProcessIsNotRunningException : Exception { }
}