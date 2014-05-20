using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageCompressor.Job
{
    class Program
    {
        private static string[] filters = { "*.png", "*.jpg", "*.jpeg", "*.gif" };
        private static ImageCompressor _compressor = new ImageCompressor();
        private static List<string> _cache = new List<string>();

        static void Main(string[] args)
        {
            StartListener();

            while (true)
            {
                System.Threading.Thread.Sleep(int.MaxValue);
            }
        }

        public static void StartListener()
        {
            // Path to the website
            string folder = @"D:\home\site\wwwroot\";

            //folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\ImageOptimization\img";

            foreach (string filter in filters)
            {
                FileSystemWatcher w = new FileSystemWatcher(folder);
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
            if (_cache.Contains(e.FullPath))
                return;

            try
            {
                _cache.Add(e.FullPath);

                // Wait a bit before kicking off compression to avoid file locks
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                var result = _compressor.CompressFile(e.FullPath);

                // Wait to exit so events on the FileSystemWatcher aren't firing.
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                _cache.Remove(e.FullPath);
            }
        }
    }
}