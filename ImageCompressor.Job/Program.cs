using AzureJobs.Common;
using System;
using System.IO;
using System.Threading;

namespace ImageCompressor.Job {
    class Program {

        /// <summary>
        /// If options are passed into the cmd line they will be availabe here
        /// </summary>
        private static CommandLineOptions cmdLineOptions;
        private static string[] _fileExtentionsToCompress = { "*.png", "*.jpg", "*.jpeg", "*.gif" };

        static void Main(string[] args) {
            if (new AzureHelper().IsRunningAsWebJob()) {
                StartAsAzureJob();
            }
            else {
                StartAsConsole(args);
            }
        }
        
        private static void StartAsAzureJob() {
            try {
                cmdLineOptions = new CommandLineOptions();
                cmdLineOptions.ItemsToProcessDirectory = @"D:\home\site\wwwroot\";
                cmdLineOptions.FileExtensionsToCompress = new string[] { "*.png", "*.jpg", "*.jpeg", "*.gif" };
                cmdLineOptions.OptimizerCacheFile = Path.Combine(cmdLineOptions.ItemsToProcessDirectory, @"app_data\ImageOptimizerHashTable.xml");
                //cmdLineOptions.ItemsToProcessDirectory = @"C:\Users\madsk\Documents\GitHub\AzureJobs\Azurejobs.Web\ImageOptimization\img";

                var imgCompressorMgr = new ImageCompressorManager(cmdLineOptions);
                imgCompressorMgr.Run();

                Timer timer = new Timer((o) => imgCompressorMgr.ProcessQueue());
                timer.Change(1000, 5000);

                while (true) {
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw ex;
            }
        }

        private static void StartAsConsole(string[] args) {
            cmdLineOptions = new CommandArgsParser().BuildCommandLineOptions(args);
            cmdLineOptions.FileExtensionsToCompress = _fileExtentionsToCompress;

            // cmdLineOptions.ItemsToProcessDirectory = @"C:\Users\Phil\Desktop\resize";

            if (cmdLineOptions.DisplayHelp || string.IsNullOrEmpty(cmdLineOptions.ItemsToProcessDirectory)) {
                CompressorBase.ShowUsage();
                Console.ReadLine();
                return;
            }
            RunCompressor();
            return;
        }

        /// <summary>
        /// Runs the image compressor (gathering imgs, compressing, writing to log). Optionally will 
        /// listen for file system changes on the directory via cmd line options StartListener.
        /// </summary>
        private static void RunCompressor() {
            Console.WriteLine("Monitoring dir " + cmdLineOptions.ItemsToProcessDirectory);
            var imgCompressorMgr = new ImageCompressorManager(cmdLineOptions);
            imgCompressorMgr.Run();
            if (cmdLineOptions.StartListener) {
                Console.WriteLine("Press Enter to quit at any time.");
                do {
                    while (Console.KeyAvailable) {
                        System.Threading.Thread.Sleep(1000);
                        imgCompressorMgr.ProcessQueue();
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
            }
        }
    }
}
