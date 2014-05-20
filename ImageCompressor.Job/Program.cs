using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _compressor = new ImageCompressor();
        private static List<string> _cache = new List<string>();

        static void Main(string[] args)
        {
            //_folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\ImageOptimization\img";
            Initialize();
            StartListener();

            while (true)
            {
                System.Threading.Thread.Sleep(int.MaxValue);
            }
        }

        private static async void Initialize()
        {
            var parent = Directory.GetParent(_folder);
            string logfile = Path.Combine(parent.FullName, "imagecompressionlog.txt");

            if (File.Exists(logfile))
                return;

            File.WriteAllText(logfile, "Started");

            foreach (string filter in _filters)
            {
                foreach (string file in Directory.EnumerateFiles(_folder, filter, SearchOption.AllDirectories))
                    await ProcessFile(file);
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
                w.Changed += w_Changed;
                w.Created += w_Changed;
                w.EnableRaisingEvents = true;
            }
        }

        private async static void w_Changed(object sender, FileSystemEventArgs e)
        {
            await ProcessFile(e.FullPath);
        }

        private static async Task ProcessFile(string file)
        {
            if (_cache.Contains(file))
                return;

            try
            {
                _cache.Add(file);

                // Wait a bit before kicking off compression to avoid file locks
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                var result = _compressor.CompressFile(file);

                // Wait to exit so events on the FileSystemWatcher aren't firing.
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                _cache.Remove(file);
            }
        }
    }
}