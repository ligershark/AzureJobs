﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace ImageCompressor.Job {
    internal class ImageCompressor {
        public event EventHandler<CompressionResult> Finished;

        public void CompressFile(string sourceFile) {
            string targetFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(sourceFile));

            ProcessStartInfo start = new ProcessStartInfo("cmd") {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"Tools\"),
                Arguments = GetArguments(sourceFile, targetFile),
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (start.Arguments == null)
                return;

            RunProcess(sourceFile, targetFile, start);
        }

        private void RunProcess(string sourceFile, string targetFile, ProcessStartInfo start) {
            try {
                using (var process = Process.Start(start)) {
                    process.WaitForExit(7000);
                    var result = new CompressionResult(sourceFile, targetFile);
                    HandleResult(result);
                }
            }
            catch { }
        }

        private void HandleResult(CompressionResult result) {
            try {
                if (result.Saving > 0 && result.ResultFileSize > 0) {
                    File.Copy(result.ResultFileName, result.OriginalFileName, true);
                }

                OnFinished(result);

                File.Delete(result.ResultFileName);
            }
            catch { }
        }

        private void OnFinished(CompressionResult result) {
            if (Finished != null)
                Finished(this, result);
        }

        private static string GetArguments(string sourceFile, string targetFile) {
            if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
                return null;

            string ext;

            try {
                ext = Path.GetExtension(sourceFile).ToLowerInvariant();
            }
            catch (ArgumentException) {
                return null;
            }

            switch (ext) {
                case ".png":
                    return string.Format(CultureInfo.CurrentCulture, "/c png.cmd \"{0}\" \"{1}\"", sourceFile, targetFile);

                case ".jpg":
                case ".jpeg":
                    return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy none -optimize -progressive -outfile \"{1}\" \"{0}\"", sourceFile, targetFile);

                case ".gif":
                    return string.Format(CultureInfo.CurrentCulture, "/c gifsicle -O3 --batch --colors=256 \"{0}\" --output=\"{1}\"", sourceFile, targetFile);
            }
            return null;
        }
    }
}
