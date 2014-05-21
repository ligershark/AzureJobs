using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureJobs.Common;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _compressor;
        private static List<string> _cache = new List<string>();
        private static Logger _log;

        static void Main(string[] args)
        {
            _folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\ImageOptimization\img";
            _log = new Logger(_folder);
            _compressor = new ImageCompressor(_log);

            Initialize();
            StartListener();

            while (true)
            {
                System.Threading.Thread.Sleep(int.MaxValue);
            }
        }
        
        private static async void Initialize()
        {
            if (_log.Exist())
                return;

            _log.Write("Installed");

            foreach (string filter in _filters)
            {
                foreach (string file in Directory.EnumerateFiles(_folder, filter, SearchOption.AllDirectories))
                {
                    await ProcessFile(file);
                }
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
                w.Changed += async (s, e) => await ProcessFile(e.FullPath);
                w.EnableRaisingEvents = true;
            }
        }

        private static async Task ProcessFile(string file)
        {
            if (_cache.Contains(file))
                return;

            try
            {
                _cache.Add(file);

                // Wait a bit before kicking off compression to avoid file locks
                await Task.Delay(TimeSpan.FromMilliseconds(2000));

                var result = _compressor.CompressFile(file);

                // Wait a bit to avoid multiple runs on the same file due to FSW quirks
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                _cache.Remove(file);
            }
        }
    }
}