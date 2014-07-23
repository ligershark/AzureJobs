namespace AzureJobs.Common {
    public class CommandLineOptions {
        public static readonly string OptimizerCacheFileArg = "--logfile";
        public static readonly string ColorArg = "--color";
        public static readonly string ItemsToProcessDirectoryArg = "--folder";
        public static readonly string NameArg = "--name";
        public static readonly string StartListenerArg = "--startlistener";
        public static readonly string HelpArg = "--help";
        public static readonly string HelpShortArg = "/?";
        public static readonly string ForceArg = "--force";
        public static readonly string SuppressCsvReportArg = "NoReport";

        public string Name { get; set; }
        public string Color { get; set; }
        public string ItemsToProcessDirectory { get; set; }
        public bool SuppressCsvReport { get; set; }
        public bool? StartListener { get; set; }
        public bool? DisplayHelp { get; set; }
        public bool ShouldForceOptimize { get; set; }
        public string OptimizerCacheFile { get; set; }
    }
}
