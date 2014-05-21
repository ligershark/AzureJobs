using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using AzureJobs.Common;

namespace ImageCompressor.Job
{
    internal class ImageCompressor
    {
        private const string _dataUriPrefix = "base64-";
        private Logger _log;

        public ImageCompressor(Logger log)
        {
            _log = log;
        }

        public static bool IsFileSupported(string fileName)
        {
            return GetArguments(fileName, string.Empty) != null;
        }

        public CompressionResult CompressFile(string fileName)
        {
            _log.Write("Optimizing " + fileName + "...");
            string targetFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(fileName));

            ProcessStartInfo start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"Tools\"),
                Arguments = GetArguments(fileName, targetFile),
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                var process = Process.Start(start);
                {
                    process.WaitForExit(5000);
                    var result = new CompressionResult(fileName, targetFile);
                    HandleResult(fileName, result);
                    return result;
                }
            }
            catch
            {
                CompressionResult result = new CompressionResult(fileName, targetFile);
                File.Delete(targetFile);
                return result;
            }
        }

        private void HandleResult(string file, CompressionResult result)
        {
            string name = file.Contains(_dataUriPrefix) ? "the dataUri" : Path.GetFileName(file);

            if (result.Saving > 0)
            {
                File.Copy(result.ResultFileName, result.OriginalFileName, true);

                string text = "Compressed " + name + " by " + result.Saving + " bytes / " + result.Percent + "%";
                _log.Write(text);
            }
            else
            {
                _log.Write(name + " is already optimized");
            }
        }

        private static string GetArguments(string sourceFile, string targetFile)
        {
            if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
                return null;

            string ext;

            try
            {
                ext = Path.GetExtension(sourceFile).ToLowerInvariant();
            }
            catch (ArgumentException)
            {
                return null;
            }

            switch (ext)
            {
            case ".png":
                return string.Format(CultureInfo.CurrentCulture, "/c png.cmd \"{0}\" \"{1}\"", sourceFile, targetFile);

            case ".jpg":
            case ".jpeg":
                return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy none -optimize -progressive \"{0}\" \"{1}\"", sourceFile, targetFile);

            case ".gif":
                return string.Format(CultureInfo.CurrentCulture, "/c gifsicle --crop-transparency --no-comments --no-extensions --no-names --optimize=3 --batch \"{0}\" --output=\"{1}\"", sourceFile, targetFile);
            }

            return null;
        }
    }
}