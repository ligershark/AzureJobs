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
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.css", "*.js" };
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
            //_folder = @"C:\Users\madsk\Documents\Visual Studio 2013\Projects\AzureJobs\Azurejobs.Web\Minification\files";
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
            string logfile = Path.Combine(parent.FullName, "minificationlog.txt");

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
                w.NotifyFilter = NotifyFilters.LastWrite;
                w.Changed += w_Changed;
                w.EnableRaisingEvents = true;
            }
        }

        private async static void w_Changed(object sender, FileSystemEventArgs e)
        {
            await ProcessFile(e.FullPath);
        }

        private static async Task ProcessFile(string file)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (file.EndsWith(".min" + ext, StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".intellisense" + ext, StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".debug" + ext, StringComparison.OrdinalIgnoreCase) ||
                _cache.Contains(file))
                return;

            try
            {
                _cache.Add(file);

                // Wait a bit before kicking off compression to avoid file locks
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                Minify(file, ext);
            }
            finally
            {
                _cache.Remove(file);
            }
        }

        private static void Minify(string sourcePath, string ext)
        {
            string content = File.ReadAllText(sourcePath);
            string result;

            Console.WriteLine("Minifying " + Path.GetFileName(sourcePath));

            if (ext == ".js")
                result = _minifier.MinifyJavaScript(content, _jsSettings);
            else if (ext == ".css")
                result = _minifier.MinifyStyleSheet(content, _cssSettings);
            else
                return;

            if (content != result)
            {
                File.WriteAllText(sourcePath, result, Encoding.UTF8);
                Console.WriteLine("Minification done. Old size: " + content.Length + " bytes. New size: " + result.Length + " bytes");
            }
            else
                Console.WriteLine("Couldn't minify any further" + Environment.NewLine);
        }
    }
}