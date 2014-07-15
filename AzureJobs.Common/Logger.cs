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
        const string LogFileExtension = ".csv";
        const string Header = "Date,Filename,Original Size (B),Optimized To (B),Savings (%)";

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
                    CreateFileWithHeader();
                    AddLineToLogFile(messageString);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Creates the log file if it doesn't exist. The header is written as well.
        /// </summary>
        private void CreateFileWithHeader()
        {
            if (!File.Exists(_logFile))
            {
                File.WriteAllText(_logFile, Header + Environment.NewLine);
            }
        }

        /// <summary>
        /// Appends a line of text to the log file.
        /// </summary>
        /// <param name="messageString"></param>
        private void AddLineToLogFile(string messageString)
        {
            using (FileStream fs = new FileStream(_logFile, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(messageString);
            }
        }
        
        private string GetLogFilePath(string path)
        {
            Directory.CreateDirectory(path);
            string name = Assembly.GetEntryAssembly().ManifestModule.Name;
            return Path.Combine(path, name + LogFileExtension);
        }
    }
}
