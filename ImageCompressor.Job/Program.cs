using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureJobs.Common;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string _directoryToOptimize = @"D:\home\site\wwwroot\";
        private static string[] _fileExtentionsToCompress = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _imageCompressor = new ImageCompressor();
        private static Dictionary<string, DateTime> _filesToCompress = new Dictionary<string, DateTime>();
        private static Logger _logger;
        private static FileHashStore _store;
        private static object _syncRoot = new object();
        private static bool _isProcessing;
        private static bool _shouldForceOptimizeAllFiles;
        /// <summary>
        /// If options are passed into the cmd line they will be availabe here
        /// </summary>
        private static CommandLineOptions cmdLineOptions;

        static void Main(string[] args)
        {
            if (new AzureHelper().IsRunningAsWebJob())
            {
                StartAsAzureJob();
            }
            else
            {
                // invoked from the command line. Print out the args to begin with
                StartAsConsole(args);
            }
        }

        private static void StartAsAzureJob()
        {
            //_folder = @"C:\Users\madsk\Documents\GitHub\AzureJobs\Azurejobs.Web\ImageOptimization\img";
            _logger = new Logger(Path.Combine(_directoryToOptimize, "app_data"));
            _store = new FileHashStore(Path.Combine(_directoryToOptimize, "app_data\\ImageOptimizerHashTable.xml"));
            _imageCompressor.Finished += WriteToLog;

            QueueExistingFiles();
            ProcessQueue();
            StartListener();

            Timer timer = new Timer((o) => ProcessQueue());
            timer.Change(1000, 5000);

            while (true)
            {
                Thread.Sleep(2000);
            }
        }

        private static void EnsureImgOptimizerCacheFileExists()
        {
            string logFile = !string.IsNullOrEmpty(cmdLineOptions.OptimizerCacheFile) ?
                                cmdLineOptions.OptimizerCacheFile :
                                Environment.ExpandEnvironmentVariables(@"%APPDATA%\LigerShark\AzureJobs\imageoptimizer-cache.xml");

            _store = new FileHashStore(logFile);
        }

        private static void SetupOptimizationDependencies()
        {
            EnsureImgOptimizerCacheFileExists();

            _directoryToOptimize = cmdLineOptions.ItemsToProcessDirectory;
            _shouldForceOptimizeAllFiles = cmdLineOptions.ShouldForceOptimize;
            _logger = new Logger(cmdLineOptions.ItemsToProcessDirectory);
            _imageCompressor.Finished += WriteToLog;
        }
        
        private static void StartAsConsole(string[] args)
        {
            cmdLineOptions = new CommandArgsParser().BuildCommandLineOptions(args);

            if (string.IsNullOrEmpty(cmdLineOptions.ItemsToProcessDirectory))
            {
                ShowUsage();
                return;
            }
            SetupOptimizationDependencies();
            QueueExistingFiles();
            ProcessQueue();

            if (cmdLineOptions.StartListener.GetValueOrDefault())
            {
                Console.Write("Listener started. Press Enter to quit.");
                Console.ReadLine();
            }
        }
        private static void ShowUsage()
        {
            string usage = @"ImageCompressor.Job.exe --folder <folder-path> [options]

Options
    /? --help : To display help
       --logfile <logfile>  : Location of the log file. If none passed, it will be stored under App_Data
       --startlistener      : To start the listern on that directory
       --NoReport           : You can use this to prevent the .csv file from being written into the working directory.        
";

            Console.WriteLine(usage);
        }

        private static void QueueExistingFiles()
        {
            foreach (string filter in _fileExtentionsToCompress)
                foreach (string file in Directory.EnumerateFiles(_directoryToOptimize, filter, SearchOption.AllDirectories))
                {
                    AddToQueue(file, DateTime.MinValue);
                }
        }

        private static void AddToQueue(string file, DateTime date)
        {
            _filesToCompress[file] = date;
        }

        public static void StartListener()
        {
            foreach (string filter in _fileExtentionsToCompress)
            {
                FileSystemWatcher w = new FileSystemWatcher(_directoryToOptimize);
                w.Filter = filter;
                w.IncludeSubdirectories = true;
                w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                w.Changed += (s, e) => AddToQueue(e.FullPath, DateTime.Now);
                w.EnableRaisingEvents = true;
            }
        }

        private static void ProcessQueue()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            int length = _filesToCompress.Count - 1;

            for (int i = length; i >= 0; i--)
            {
                if (_filesToCompress.Count < i)
                    continue;

                var entry = _filesToCompress.ElementAt(i);

                try
                {
                    // The file should be a second old before we start processing
                    if (entry.Value > DateTime.Now.AddSeconds(-2))
                        continue;

                    if (ShouldSkipFileEntry(entry.Key))
                    {
                        _filesToCompress.Remove(entry.Key);
                        Logger.WriteLineToConsole("{0} skipped because it's up to date", entry.Key);
                        continue;
                    }

                    _imageCompressor.CompressFile(entry.Key);
                    _filesToCompress.Remove(entry.Key);
                }
                catch (IOException)
                {
                    // do nothing. We'll try again
                }
                catch
                {
                    _filesToCompress.Remove(entry.Key);
                }
            }

            _isProcessing = false;
        }

        /// <summary>
        /// Both the force override is off and the hash entry hasn't changed.
        /// </summary>
        /// <param name="entryKey"></param>
        /// <returns></returns>
        private static bool ShouldSkipFileEntry(string entryKey)
        {
            return (_shouldForceOptimizeAllFiles == false && _store.HasChangedOrIsNew(entryKey) == false);
        }

        private static void WriteToLog(object sender, CompressionResult e)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                _store.Save(e.OriginalFileName);

                if (e == null || e.ResultFileSize == 0)
                    return;

                if (cmdLineOptions == null || !cmdLineOptions.SuppressCsvReport)
                {
                    string name = new Uri(_directoryToOptimize).MakeRelativeUri(new Uri(e.OriginalFileName)).ToString();
                    _logger.Write(DateTime.Now, name, e.OriginalFileSize, Math.Min(e.ResultFileSize, e.OriginalFileSize), e.Percent);
                }
            });
        }
    }
}
