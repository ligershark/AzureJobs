using AzureJobs.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading;
namespace ImageCompressor.Job {
    public class ImageCompressorManager : CompressorBase, ICompressorJob {

        private ImageCompressor _imageCompressor;

        public ImageCompressorManager(CommandLineOptions cmdLineOptions)
            : base(cmdLineOptions) {
            _imageCompressor = new ImageCompressor();

            SetupDependencies();
            StartListener();
            _imageCompressor.Finished += WriteToLog;
        }

        private void WriteToLog(object sender, CompressionResult e) {
            if (!_cmdLineOptions.SuppressCsvReport) {
                ThreadPool.QueueUserWorkItem((o) => {
                    _store.Save(e.OriginalFileName);

                    if (e == null /*|| e.ResultFileSize == 0*/) {
                        return;
                    }
                    var logItem = new LogItem { FileName = BuildRelativeFilePath(e.OriginalFileName), OriginalSizeBytes = e.OriginalFileSize, NewSizeBytes = e.ResultFileSize };
                    _logger.Write(logItem);
                });
            }
        }
        public void Run() {
            QueueExistingFiles();
            ProcessQueue();
        }

        public void ProcessQueue() {
            if (_isProcessing)
                return;

            _isProcessing = true;
            int length = _filesToCompress.Count - 1;

            for (int i = length; i >= 0; i--) {
                if (_filesToCompress.Count < i)
                    continue;

                var entry = _filesToCompress.ElementAt(i);

                try {
                    // The file should be a second old before we start processing
                    if (entry.Value > DateTime.Now.AddSeconds(-1))
                        continue;

                    if (ShouldSkipFileEntry(entry.Key)) {
                        _filesToCompress.Remove(entry.Key);
                        Logger.WriteLineToConsole("{0} skipped because it's up to date", entry.Key);
                        continue;
                    }

                    _imageCompressor.CompressFile(entry.Key);
                    _filesToCompress.Remove(entry.Key);
                }
                catch (IOException) {
                    // do nothing. We'll try again
                }
                catch {
                    _filesToCompress.Remove(entry.Key);
                }
            }

            _isProcessing = false;
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
