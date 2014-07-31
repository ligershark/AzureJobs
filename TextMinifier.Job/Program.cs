using AzureJobs.Common;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextMinifier.Job {
    class Program {
        private static string _directoryToOptimize = @"D:\home\site\wwwroot\";
        private static string[] _fileExtentionsToCompress = { "*.css", "*.js" };
        private static Dictionary<string, DateTime> _filesToCompress = new Dictionary<string, DateTime>();
        private static Minifier _minifier = new Minifier();
        private static Logger _logger;
        private static FileHashStore _store;
        private static CommandLineOptions cmdLineOptions;

        private static CssSettings _cssSettings = new CssSettings {
            CommentMode = CssComment.Important
        };

        private static CodeSettings _jsSettings = new CodeSettings {
            EvalTreatment = EvalTreatment.MakeImmediateSafe,
            TermSemicolons = true,
            PreserveImportantComments = false
        };

        static void Main(string[] args) {
            cmdLineOptions = new CommandArgsParser().BuildCommandLineOptions(args);
            cmdLineOptions.ItemsToProcessDirectory = @"C:\Users\Phil\Documents\GitHub\RssPerson";
            //_folder = @"C:\Users\madsk\Documents\GitHub\AzureJobs\Azurejobs.Web\Minification\files";
            if (cmdLineOptions.DisplayHelp || string.IsNullOrEmpty(cmdLineOptions.ItemsToProcessDirectory)) {
                ShowUsage();
                return;
            }
            SetupMinificationDependencies();
            QueueExistingFiles();
            ProcessQueue();
            StartListener();

            while (true) {
                System.Threading.Thread.Sleep(1000);
                ProcessQueue();
            }
        }
        private static void SetupMinificationDependencies() {
            EnsureTextMinifierCacheFileExists();
            _directoryToOptimize = cmdLineOptions.ItemsToProcessDirectory;
            _logger = new Logger(cmdLineOptions.ItemsToProcessDirectory);
        }

        private static void ShowUsage() {
            string helpText = new CommandLineOptions().ToString();
            string usage = string.Format("{0}\r\n{1}", AppDomain.CurrentDomain.FriendlyName, helpText);
            Console.WriteLine(usage);
        }

        private static void EnsureTextMinifierCacheFileExists() {
            string logFile = !string.IsNullOrEmpty(cmdLineOptions.OptimizerCacheFile) ?
                                cmdLineOptions.OptimizerCacheFile :
                                Environment.ExpandEnvironmentVariables(@"%APPDATA%\LigerShark\AzureJobs\textminifier-cache.xml");

            _store = new FileHashStore(logFile);
        }
        private static void QueueExistingFiles() {
            foreach (string filter in _fileExtentionsToCompress)
                foreach (string file in Directory.EnumerateFiles(_directoryToOptimize, filter, SearchOption.AllDirectories))
                    AddToQueue(file, DateTime.MinValue);
        }

        private static void AddToQueue(string file, DateTime date) {
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (file.EndsWith(".min" + ext, StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".intellisense" + ext, StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".debug" + ext, StringComparison.OrdinalIgnoreCase))
                return;

            _filesToCompress[file] = date;
        }

        public static void StartListener() {
            foreach (string filter in _fileExtentionsToCompress) {
                FileSystemWatcher w = new FileSystemWatcher(_directoryToOptimize);
                w.Filter = filter;
                w.IncludeSubdirectories = true;
                w.NotifyFilter = NotifyFilters.LastWrite;
                w.Changed += (s, e) => AddToQueue(e.FullPath, DateTime.Now);
                w.EnableRaisingEvents = true;
            }
        }

        private static void ProcessQueue() {
            for (int i = _filesToCompress.Count - 1; i >= 0; i--) {
                var entry = _filesToCompress.ElementAt(i);

                // The file should be 1 second old before we start processing
                if (entry.Value > DateTime.Now.AddSeconds(-1))
                    continue;

                if (!_store.HasChangedOrIsNew(entry.Key)) {
                    _filesToCompress.Remove(entry.Key);
                    continue;
                }

                try {
                    Minify(entry.Key);

                    _store.Save(entry.Key);
                    _filesToCompress.Remove(entry.Key);
                }
                catch (IOException) {
                    // Do nothing, let's try again next time
                }
                catch {
                    _filesToCompress.Remove(entry.Key);
                }
            }
        }

        private static void Minify(string sourcePath) {
            string ext = Path.GetExtension(sourcePath).ToLowerInvariant();
            string content = File.ReadAllText(sourcePath);
            string result;

            if (ext == ".js")
                result = _minifier.MinifyJavaScript(content, _jsSettings);
            else if (ext == ".css")
                result = _minifier.MinifyStyleSheet(content, _cssSettings);
            else
                return;

            if (content != result) {
                File.WriteAllText(sourcePath, result, Encoding.UTF8);
                string name = new Uri(_directoryToOptimize).MakeRelativeUri(new Uri(sourcePath)).ToString();
                var logItem = new LogItem { FileName = name, OriginalSizeBytes = content.Length, NewSizeBytes = result.Length };
                _logger.Write(logItem);
            }
        }
    }
}
