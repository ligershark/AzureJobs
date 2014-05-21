using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureJobs.Common;
using Microsoft.Ajax.Utilities;

namespace TextMinifier.Job
{
    class Program
    {
        private static string _folder = @"D:\home\site\wwwroot\";
        private static string[] _filters = { "*.css", "*.js" };
        private static List<string> _cache = new List<string>();
        private static Minifier _minifier = new Minifier();
        private static Logger _log;

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
            _log = new Logger(_folder);
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
                w.Changed += async (s, e) => await ProcessFile(e.FullPath);
                w.EnableRaisingEvents = true;
            }
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
                await Task.Delay(TimeSpan.FromMilliseconds(1000));

                Minify(file, ext);

                // Wait a bit to avoid multiple runs on the same file due to FSW quirks
                await Task.Delay(TimeSpan.FromMilliseconds(100));
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

            _log.Write("Minifying " + Path.GetFileName(sourcePath));

            if (ext == ".js")
                result = _minifier.MinifyJavaScript(content, _jsSettings);
            else if (ext == ".css")
                result = _minifier.MinifyStyleSheet(content, _cssSettings);
            else
                return;

            if (content != result)
            {
                File.WriteAllText(sourcePath, result, Encoding.UTF8);
                _log.Write("Minification done. Old size: " + content.Length + " bytes. New size: " + result.Length + " bytes");
            }
            else
                _log.Write("Couldn't minify any further" + Environment.NewLine);
        }
    }
}