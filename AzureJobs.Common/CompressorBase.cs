using System;
using System.Collections.Generic;
using System.IO;

namespace AzureJobs.Common {
    public class CompressorBase {

        protected Dictionary<string, DateTime> _filesToCompress = new Dictionary<string, DateTime>();
        protected Logger _logger;
        protected FileHashStore _store;
        protected object _syncRoot = new object();
        protected bool _isProcessing;

        protected CommandLineOptions _cmdLineOptions;

        public CompressorBase(CommandLineOptions cmdLineOptions) {
            _cmdLineOptions = cmdLineOptions;
        }
        public static void ShowUsage() {
            string helpText = new CommandLineOptions().ToString();
            string usage = string.Format("{0}\r\n{1}", AppDomain.CurrentDomain.FriendlyName, helpText);
            Console.WriteLine(usage);
        }

        protected string BuildRelativeFilePath(string originalFileName) {
            return new Uri(_cmdLineOptions.ItemsToProcessDirectory).MakeRelativeUri(new Uri(originalFileName)).ToString();

        }

        /// <summary>
        /// Set the img optimizer cache file. If pass by user by cmd line, use that. Otherwise use our subdir in the user's AppData dir.
        /// Ensure the directory exists.
        /// </summary>
        protected void EnsureImgOptimizerCacheFileExists() {
            string logFile = !string.IsNullOrEmpty(_cmdLineOptions.OptimizerCacheFile) ?
                                _cmdLineOptions.OptimizerCacheFile :
                                Environment.ExpandEnvironmentVariables(@"%APPDATA%\LigerShark\AzureJobs\imageoptimizer-cache.xml");

            _store = new FileHashStore(logFile);
        }

        /// <summary>
        /// Ensure all the related dependencies are ready to use. Pull from cmd line options given, setup logger, and events.
        /// </summary>
        protected void SetupDependencies() {
            EnsureImgOptimizerCacheFileExists();
            _logger = new Logger(_cmdLineOptions.ItemsToProcessDirectory);            
        }
        protected void QueueExistingFiles() {
            foreach (string filter in _cmdLineOptions.FileExtensionsToCompress)
                foreach (string file in Directory.EnumerateFiles(_cmdLineOptions.ItemsToProcessDirectory, filter, SearchOption.AllDirectories)) {
                    AddToQueue(file, DateTime.MinValue);
                }
        }

        protected virtual void AddToQueue(string file, DateTime date) {
            _filesToCompress[file] = date;
        }

        protected void StartListener() {
            foreach (string filter in _cmdLineOptions.FileExtensionsToCompress) {
                FileSystemWatcher w = new FileSystemWatcher(_cmdLineOptions.ItemsToProcessDirectory);
                w.Filter = filter;
                w.IncludeSubdirectories = true;
                w.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
                w.Changed += (s, e) => AddToQueue(e.FullPath, DateTime.Now);
                w.Renamed += (s, e) => AddToQueue(e.FullPath, DateTime.Now);
                w.EnableRaisingEvents = true;
            }
        }

    }
}
