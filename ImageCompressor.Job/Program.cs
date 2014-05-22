using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureJobs.Common;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _compressor;
        private static Dictionary<string, DateTime> _cache = new Dictionary<string, DateTime>();
        private static Logger _log;
        private static FileHashStore _store;

        static void Main(string[] args)
        {
            //_folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\ImageOptimization\img";
            _log = new Logger(Path.Combine(_folder, "app_data"));
            _compressor = new ImageCompressor(_log);
            _store = new FileHashStore(Path.Combine(_folder, "app_data\\ImageOptimizerHashTable.xml"), _log);

            QueueExistingFiles();
            ProcessQueue();
            StartListener();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                ProcessQueue();
            }
        }

        private static void QueueExistingFiles()
        {
            foreach (string filter in _filters)
                foreach (string file in Directory.EnumerateFiles(_folder, filter, SearchOption.AllDirectories))
                {
                    _cache[file] = DateTime.MinValue;
                }
        }


        public static void StartListener()
        {
            foreach (string filter in _filters)
            {
                FileSystemWatcher w = new FileSystemWatcher(_folder);
                w.Filter = filter;
                w.IncludeSubdirectories = true;
                w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                w.Changed += (s, e) => _cache[e.FullPath] = DateTime.Now;
                w.EnableRaisingEvents = true;
            }
        }

        private static void ProcessQueue()
        {
            for (int i = _cache.Count - 1; i >= 0; i--)
            {
                var entry = _cache.ElementAt(i);

                // The file should be 1 second old before we start processing
                if (entry.Value > DateTime.Now.AddSeconds(-1))
                    continue;

                if (!_store.HasChangedOrIsNew(entry.Key))
                {
                    _cache.Remove(entry.Key);
                    continue;
                }

                try
                {
                    _compressor.CompressFile(entry.Key);
                    
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
}