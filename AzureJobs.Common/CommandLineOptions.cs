using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureJobs.Common {
    public class CommandLineOptions {
        public static readonly string LogFileArg = "--logfile";
        public static readonly string ColorArg = "--color";
        public static readonly string FolderArg = "--folder";
        public static readonly string NameArg = "--name";
        public static readonly string StartListenerArg = "--startlistener";
        public static readonly string HelpArg = "--help";
        public static readonly string HelpShortArg = "/?";

        public string LogFile { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Folder { get; set; }
        public bool? StartListener { get; set; }
        public bool? DisplayHelp { get; set; }
    }
}
