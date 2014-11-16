using AzureJobs.Common;
using Microsoft.Ajax.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
namespace TextMinifier.Job {
    public class MinifierManager : CompressorBase, ICompressorJob {

        private Minifier _minifier;

        private static CssSettings _cssSettings = new CssSettings {
            CommentMode = CssComment.Important
        };

        private static CodeSettings _jsSettings = new CodeSettings {
            EvalTreatment = EvalTreatment.MakeImmediateSafe,
            TermSemicolons = true,
            PreserveImportantComments = false
        };

        public MinifierManager(CommandLineOptions cmdLineOptions)
            : base(cmdLineOptions) {
            _minifier = new Minifier();
            SetupDependencies();
            StartListener();
        }

        private void Minify(string sourcePath) {
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
                WriteToLog(sourcePath, content.Length, result.Length);
            }
        }

        private void WriteToLog(string modifiedFilePath, long originalSizeBytes, long newSizeBytes) {
            if (!_cmdLineOptions.SuppressCsvReport) {
                ThreadPool.QueueUserWorkItem((o) => {
                    var logItem = new LogItem {
                        FileName = BuildRelativeFilePath(modifiedFilePath),
                        OriginalSizeBytes = originalSizeBytes,
                        NewSizeBytes = newSizeBytes
                    };
                    _logger.Write(logItem);
                });
            }
        }
        public void Run() {
            QueueExistingFiles();
            Console.WriteLine("Found " + _filesToCompress.Count + " files.");

            ProcessQueue();
        }

        public void ProcessQueue() {
            for (int i = _filesToCompress.Count - 1; i >= 0; i--) {
                var entry = _filesToCompress.ElementAt(i);

                // The file should be 1 second old before we start processing
                if (entry.Value > DateTime.Now.AddSeconds(-1))
                    continue;

                if (!_store.HasChangedOrIsNew(entry.Key)) {
                    Logger.WriteLineToConsole("{0} skipped because it's up to date", entry.Key);
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

        protected override void AddToQueue(string file, DateTime date) {

            if (ShouldIgnoreFile(file)) {
                return;
            }

            base.AddToQueue(file, date);
        }

        /// <summary>
        /// Determines whether this file should be ignored for minification.
        /// Will ignore files usually named for dev purposes or previously minified
        /// </summary>
        private bool ShouldIgnoreFile(string file) {
            string[] ignoreTokens = new[] { ".min", ".insellisense", ".debug" };
            string fileExtension = Path.GetExtension(file).ToLowerInvariant();
            return (ignoreTokens.Any(ignoreToken => file.EndsWith(ignoreToken + fileExtension, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Determine whether to skip the compression of this file. This method determines whether the user has
        /// sent the force parameter, and whether the hash entry is new or has changed since last run. If either are false, then the 
        /// file should be processed/compressed.
        /// </summary>
        /// <param name="entryKey">The key of the file store hash to check</param>
        private bool ShouldSkipFileEntry(string entryKey) {
            return (_cmdLineOptions.ShouldForceOptimize == false && _store.HasChangedOrIsNew(entryKey) == false);
        }
    }
}
