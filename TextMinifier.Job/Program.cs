using AzureJobs.Common;
using System;

namespace TextMinifier.Job {
    class Program {

        private static CommandLineOptions cmdLineOptions;
        private static string[] _fileExtentionsToCompress = { "*.css", "*.js" };

        static void Main(string[] args) {
            cmdLineOptions = new CommandArgsParser().BuildCommandLineOptions(args);
            cmdLineOptions.FileExtensionsToCompress = _fileExtentionsToCompress;
            
            if (cmdLineOptions.DisplayHelp || string.IsNullOrEmpty(cmdLineOptions.ItemsToProcessDirectory)) {
                CompressorBase.ShowUsage();
                Console.ReadLine();
                return;
            }
            RunMinifier();
            return;
        }

        private static void RunMinifier() {
            Console.WriteLine("Monitoring dir " + cmdLineOptions.ItemsToProcessDirectory);
            var minMgr = new MinifierManager(cmdLineOptions);
            minMgr.Run();
            if (cmdLineOptions.StartListener) {
                Console.WriteLine("Press Enter to quit at any time.");
                do {
                    while (!Console.KeyAvailable) {
                        System.Threading.Thread.Sleep(1000);
                        minMgr.ProcessQueue();
                    }
                } while (Console.ReadKey(intercept: true).Key != ConsoleKey.Enter);
            }
        }
    }
}
