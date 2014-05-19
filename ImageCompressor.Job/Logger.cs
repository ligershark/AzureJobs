using System;
using System.Diagnostics;

namespace ImageCompressor.Job
{
    public static class Logger
    {
        public static void Log(string message)
        {
            Trace.WriteLine(message);
            Console.WriteLine(message);
        }    
    }
}
