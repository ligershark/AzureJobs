using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;

namespace TextMinifier.Job
{
    class Program
    {
        private static string[] filters = { "*.css", "*.js" };
        private static List<string> _cache = new List<string>();
        private static Minifier _minifier = new Minifier();

        private static CssSettings _cssSettings = new CssSettings
        {
            CommentMode = CssComment.Important
        };

        private static CodeSettings _jsSettings = new CodeSettings
        {
            EvalTreatment = EvalTreatment.MakeImmediateSafe,
            TermSemicolons = true,
            PreserveImportantComments = false
        };

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

            //folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\Minification\files";

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
            string ext = Path.GetExtension(e.FullPath).ToLowerInvariant();

            if (e.FullPath.EndsWith(".min" + ext, StringComparison.OrdinalIgnoreCase) ||
                e.FullPath.EndsWith(".intellisense" + ext, StringComparison.OrdinalIgnoreCase) ||
                e.FullPath.EndsWith(".debug" + ext, StringComparison.OrdinalIgnoreCase) ||
                _cache.Contains(e.FullPath))
                return;

            try
            {
                _cache.Add(e.FullPath);

                // Wait a bit before kicking off compression to avoid file locks
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                Minify(e.FullPath, ext);

                // Wait to exit so events on the FileSystemWatcher aren't firing.
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                _cache.Remove(e.FullPath);
            }
        }

        private static void Minify(string sourcePath, string ext)
        {
            string content = File.ReadAllText(sourcePath);
            string result;

            if (ext == ".js")
                result = _minifier.MinifyJavaScript(content, _jsSettings);
            else if (ext == ".css")
                result = _minifier.MinifyStyleSheet(content, _cssSettings);
            else
                return;

            if (content != result)
                File.WriteAllText(sourcePath, result, Encoding.UTF8);
        }
    }
}
