using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AzureJobs.Common
{
    public class Logger
    {
        private string _path;
        private static object _syncRoot = new object();

        public Logger(string path)
        {
            _path = path;
}

        public bool Exist()
        {
            return File.Exists(GetLogFilePath());
        }

        public void Write(params object[] messages)
        {
            string messageString = string.Join(", ", messages);
            Trace.WriteLine(messageString);
            Console.WriteLine(messageString);

            string file = GetLogFilePath();

            string dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(file))
                File.WriteAllText(file, "Date, Filename, Original, Optimized");

            lock (_syncRoot)
            {
                File.AppendAllText(file, Environment.NewLine + messageString);
            }
        }

        private string GetLogFilePath()
        {
            var ass = Assembly.GetEntryAssembly();
            string name = ass.ManifestModule.Name;
            return Path.Combine(_path, name + ".csv");
        }
    }
}