using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AzureJobs.Common
{
    public class Logger
    {
        private string _logFile;
        private static object _syncRoot = new object();

        public Logger(string path)
        {
            _logFile = GetLogFilePath(path);
        }

        public void Write(params object[] messages)
        {
            string messageString = string.Join(", ", messages);
            Trace.WriteLine(messageString);
            Console.WriteLine(messageString);

            try
            {
                lock (_syncRoot)
                {
                    if (!File.Exists(_logFile))
                        File.WriteAllText(_logFile, "Date, Filename, Original, Optimized");

                    using (FileStream fs = new FileStream(_logFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(messageString);
                    }
                }
            }
            catch
            {
                // Do nothing
            }
        }

        private string GetLogFilePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var entry = Assembly.GetEntryAssembly();
            string name = entry.ManifestModule.Name;
            string logFile = Path.Combine(path, name + ".csv");

            return logFile;
        }
    }
}