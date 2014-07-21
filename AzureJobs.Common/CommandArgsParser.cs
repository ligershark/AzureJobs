using System;
using System.Collections.Generic;
using System.Text;

namespace AzureJobs.Common
{
    public class CommandArgsParser
    {
        /// <summary>
        /// Given all the args from the user, create key value pairs from them.
        /// Example:
        /// --file one.txt --name "azure image optimizer" --color Green
        /// --help or /?
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IDictionary<string, string> ParseArgs(string[] args)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            string currentKey = null;
            StringBuilder currentValue = new StringBuilder();
            foreach (string argPart in args)
            {
                string fixedArg = argPart;

                if (fixedArg != null) { fixedArg = argPart.Trim(); }

                if (fixedArg.StartsWith("-"))
                {

                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        // apply current key / value to the dictionary
                        string value = currentValue.ToString().Trim();
                        if (string.IsNullOrEmpty(value))
                        {
                            value = true.ToString();
                        }

                        result[currentKey] = value;
                        currentValue = new StringBuilder();
                    }

                    currentKey = fixedArg.ToLower();
                }
                else if (string.Compare(CommandLineOptions.HelpShortArg, fixedArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    currentKey = CommandLineOptions.HelpArg;
                }
                else
                {
                    currentValue.Append(argPart);
                    currentValue.Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(currentKey))
            {
                // apply current key / value to the dictionary
                string value = currentValue.ToString().Trim(); ;
                if (string.IsNullOrEmpty(value))
                {
                    value = true.ToString();
                }

                result.Add(currentKey, value);
            }

            return result;
        }

        /// <summary>
        /// Build an entity that holds all the options and config from the command line.
        /// </summary>
        /// <param name="args">The arg list from the user.</param>
        /// <returns>CommandLineOptions object populated with the user's config.</returns>
        public CommandLineOptions BuildCommandLineOptions(string[] args)
        {
            IDictionary<string, string> argsDict = ParseArgs(args);
            ICollection<string> keys = argsDict.Keys;
            CommandLineOptions options = new CommandLineOptions();

            if (keys.Contains(CommandLineOptions.ColorArg))
            {
                options.Color = argsDict[CommandLineOptions.ColorArg];
            }

            if (keys.Contains(CommandLineOptions.ItemsToProcessDirectoryArg))
            {
                options.ItemsToProcessDirectory = argsDict[CommandLineOptions.ItemsToProcessDirectoryArg];
            }

            if (keys.Contains(CommandLineOptions.OptimizerCacheFileArg))
            {
                options.OptimizerCacheFile = argsDict[CommandLineOptions.OptimizerCacheFileArg];
            }

            if (keys.Contains(CommandLineOptions.NameArg))
            {
                options.Name = argsDict[CommandLineOptions.NameArg];
            }

            options.DisplayHelp = keys.Contains(CommandLineOptions.HelpArg);
            options.StartListener = keys.Contains(CommandLineOptions.StartListenerArg);
            options.SuppressCsvReport = keys.Contains(CommandLineOptions.SuppressCsvReportArg);
            options.ShouldForceOptimize = keys.Contains(CommandLineOptions.ForceArg);

            return options;
        }
    }
}