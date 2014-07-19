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
            string messageString = CleanMessageForCsv(string.Join(",", messages));
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

        /// <summary>
        /// Determines if the input string needs escaping in prep for writing to a csv file.
        /// If the string contains a comma, double quote, carriage return, or newline, it should be wrapped in double quotes.
        /// We might encounter this here because a file name may have a comma, and depending on the user's regional
        /// setttings, numbers may be calculated with comma separators rather than decimals.
        /// The CSV RFC: http://tools.ietf.org/html/rfc4180
        /// </summary>
        /// <param name="inputToClean"></param>
        /// <returns></returns>
        public string CleanMessageForCsv(string inputToClean)
        {
            const string DoubleQuote = "\"";
            const string EscapedItemFormat = "{0}{1}{0}";
            const string ItemsToCleanFor = ",\r\n\"";
            if (inputToClean.IndexOfAny(ItemsToCleanFor.ToCharArray()) > -1)
            {
                //wrap the input string in double quotes, while replacing any double quotes in the string with 2 double quotes
                return string.Format(EscapedItemFormat, DoubleQuote,
                                                        inputToClean.Replace(DoubleQuote, DoubleQuote + DoubleQuote));
            }
            return inputToClean;
        }

        public static void WriteToConsole(string message, params object[] args)
        {
            System.Console.Write(message, args);
        }

        public static void WriteLineToConsole(string message, params object[] args)
        {
            System.Console.WriteLine(message, args);
        }
    }
}
