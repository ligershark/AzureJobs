using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
namespace AzureJobs.Common {
    public class CommandLineOptions {
        [CommandLineArg(Alias = "--name", ShortAlias = "/n", Description = @"(not used for command line)")]
        public string Name { get; set; }

        [CommandLineArg(Alias = "--color", ShortAlias = "/c", Description = @"(not used for command line)")]
        public string Color { get; set; }

        [CommandLineArg(Alias = "--dir", ShortAlias = "/d", IsRequired = true, Description = @"The directory containing files you want to compress. Subdirectories will be included.")]
        public string ItemsToProcessDirectory { get; set; }

        [CommandLineArg(Alias = "--noreport", ShortAlias = "/nr", Description = @"Won't output a .csv file with compression results.")]
        public bool SuppressCsvReport { get; set; }

        [CommandLineArg(Alias = "--startlistener", ShortAlias = "/sl", Description = @"When passed after optimizing images in the dir folder, a file watcher will run on the folder to optimize any new or modified images.")]
        public bool? StartListener { get; set; }

        [CommandLineArg(Alias = "--help", ShortAlias = "/?", Description = @"Displays this text.")]
        public bool DisplayHelp { get; set; }

        [CommandLineArg(Alias = "--force", ShortAlias = "/fo", Description = @"Force the optimizer to recompress any files it's marked as processed in a previous run.")]
        public bool ShouldForceOptimize { get; set; }

        [CommandLineArg(Alias = "--cache", ShortAlias = "/ca", Description = @"Specify the file to keep working set of files. If unspecified, then %APPDATA%\LigerShark\....")]
        public string OptimizerCacheFile { get; set; }

        /// <summary>
        /// Build the Help display for console users. Grab all info from the attributes and format nicely for display.
        /// </summary>        
        public override string ToString() {
            const string ArgDisplay = "{0} or {1} \t: {2} {3}";
            var helpText = new StringBuilder("Accepted Args");
            helpText.AppendLine();

            foreach (var cmdLineAttr in ListAllCommandLineArgs().OrderByDescending(x => x.IsRequired).ThenBy(x => x.Alias)) {
                string argHelp = string.Format(ArgDisplay,
                    cmdLineAttr.ShortAlias,
                    cmdLineAttr.Alias,
                    cmdLineAttr.Description,
                    cmdLineAttr.IsRequired ? "**Required.**" : string.Empty);
                helpText.AppendLine(argHelp);

            }
            return helpText.ToString();
        }
        private IEnumerable<CommandLineArgAttribute> ListAllCommandLineArgs() {
            foreach (PropertyInfo prop in typeof(CommandLineOptions).GetProperties()) {
                yield return prop.GetCustomAttributes(typeof(CommandLineArgAttribute)).OfType<CommandLineArgAttribute>().FirstOrDefault();
            }
        }
    }

    public class CommandLineArgAttribute : Attribute {
        public string Alias { get; set; }
        public string ShortAlias { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
    }
}
