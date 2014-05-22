using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureJobs.Common;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _compressor = new ImageCompressor();
        private static Dictionary<string, DateTime> _cache = new Dictionary<string, DateTime>();
        private static Logger _log;
        private static FileHashStore _store;
        private static object _syncRoot = new object();

        static void Main(string[] args)
        {
            _folder = @"C:\Users\madsk\Documents\GitHub\AzureJobs\Azurejobs.Web\ImageOptimization\img";
            _log = new Logger(Path.Combine(_folder, "app_data"));
            _store = new FileHashStore(Path.Combine(_folder, "app_data\\ImageOptimizerHashTable.xml"), _log);

            QueueExistingFiles();
            ProcessQueue();
            StartListener();

            while (true)
            {
                System.Threading.Thread.Sleep(2000);
                ProcessQueue();
            }
        }

        private static void QueueExistingFiles()
        {
            foreach (string filter in _filters)
                foreach (string file in Directory.EnumerateFiles(_folder, filter, SearchOption.AllDirectories))
                {
                    AddToQueue(file, DateTime.MinValue);
                }
        }

        private static void AddToQueue(string file, DateTime date)
        {
            lock (_syncRoot)
                _cache[file] = date;
        }

        public static void StartListener()
        {
            foreach (string filter in _filters)
            {
                FileSystemWatcher w = new FileSystemWatcher(_folder);
                w.Filter = filter;
                w.IncludeSubdirectories = true;
                w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                w.Changed += (s, e) => AddToQueue(e.FullPath, DateTime.Now);
                w.EnableRaisingEvents = true;
            }
        }

        private static void ProcessQueue()
        {
            lock (_syncRoot)
            {
                for (int i = _cache.Count - 1; i >= 0; i--)
                {
                    var entry = _cache.ElementAt(i);

                    // The file should be 1 second old before we start processing
                    if (entry.Value > DateTime.Now.AddSeconds(-2))
                        continue;

                    if (!_store.HasChangedOrIsNew(entry.Key))
                    {
                        _cache.Remove(entry.Key);
                        continue;
                    }

                    try
                    {
                        var result = _compressor.CompressFile(entry.Key);

                        WriteToLog(result);

                        _store.Save(entry.Key);
                        _cache.Remove(entry.Key);
                    }
                    catch (IOException)
                    {
                        // do nothing. We'll try again
                    }
                    catch
                    {
                        _cache.Remove(entry.Key);
                    }
                }
            }
        }

        private static void WriteToLog(CompressionResult result)
        {
            if (result.Saving <= 0)
                return;

            string name = new Uri(_folder).MakeRelativeUri(new Uri(result.OriginalFileName)).ToString();
            _log.Write(DateTime.Now, name, result.OriginalFileSize, result.ResultFileSize);

        }
    }
}