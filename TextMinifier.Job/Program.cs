using AzureJobs.Common;
using System;
using System.IO;
using Microsoft.Azure;

namespace TextMinifier.Job {
    class Program {
        private static string[] _fileExtentionsToCompress = { "*.css", "*.js" };

        static void Main(string[] args) {
            if (new AzureHelper().IsRunningAsWebJob()) {
                StartAsAzureJob();
            }
            else {
                StartAsConsole(args);
            }

            return;
        }
        private static void StartAsAzureJob() {
            System.Diagnostics.Trace.TraceInformation("TextMin:StartAsAzureJob");
            try {
                CommandLineOptions cmdLineOptions = new CommandLineOptions();
                cmdLineOptions.ItemsToProcessDirectory = CloudConfigurationManager.GetSetting("AZURE_MINIFIER_PATH") ?? @"D:\home\site\wwwroot\";
                cmdLineOptions.FileExtensionsToCompress = _fileExtentionsToCompress;
                cmdLineOptions.OptimizerCacheFile = Path.Combine(cmdLineOptions.ItemsToProcessDirectory, @"app_data\TextMinifierHashTable.xml");
                //cmdLineOptions.ItemsToProcessDirectory = @"C:\Users\madsk\Documents\GitHub\AzureJobs\Azurejobs.Web\ImageOptimization\img";
                RunMinifier(cmdLineOptions);
            }
            catch (Exception ex) {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw ex;
            }
        }
        private static void StartAsConsole(string[] args) {
            System.Diagnostics.Trace.TraceInformation("TextMin:StartAsConsole");
            CommandLineOptions cmdLineOptions = new CommandArgsParser().BuildCommandLineOptions(args);
            cmdLineOptions.FileExtensionsToCompress = _fileExtentionsToCompress;

            if (cmdLineOptions.DisplayHelp || string.IsNullOrEmpty(cmdLineOptions.ItemsToProcessDirectory)) {
                CompressorBase.ShowUsage();
                Console.ReadLine();
                return;
            }
            RunMinifier(cmdLineOptions);
        }

        private static void RunMinifier(CommandLineOptions cmdLineOptions) {
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
