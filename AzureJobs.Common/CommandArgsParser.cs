using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AzureJobs.Common {
    public class CommandArgsParser {
        /// <summary>
        /// Given all the args from the user, create key value pairs from them.
        /// Example:
        /// --cache one.txt --name "azure image optimizer" --color Green
        /// --help or /?
        /// </summary>
        /// <param name="args">The args directly from the user on the cmd line.</param>
        /// <returns>A dictionary with key/value pairs of the cmd line entries.</returns>
        public IDictionary<string, string> ParseArgs(string[] args) {
            IDictionary<string, string> result = new Dictionary<string, string>();

            string currentKey = null;
            StringBuilder currentValue = new StringBuilder();
            foreach (string argPart in args) {
                string fixedArg = argPart;

                if (fixedArg != null) { fixedArg = argPart.Trim(); }

                if (string.Compare("/?", fixedArg, StringComparison.OrdinalIgnoreCase) == 0) {
                    currentKey = "--help"; //todo make this a real check for --help or /?
                }
                else if (fixedArg.StartsWith("--") || fixedArg.StartsWith("/")) {

                    if (!string.IsNullOrEmpty(currentKey)) {
                        // apply current key / value to the dictionary
                        string value = currentValue.ToString().Trim();
                        if (string.IsNullOrEmpty(value)) {
                            value = true.ToString();
                        }

                        result[currentKey] = value;
                        currentValue = new StringBuilder();
                    }

                    currentKey = fixedArg.ToLower();
                }
                else {
                    currentValue.Append(argPart);
                    currentValue.Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(currentKey)) {
                // apply current key / value to the dictionary
                string value = currentValue.ToString().Trim(); ;
                if (string.IsNullOrEmpty(value)) {
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
        public CommandLineOptions BuildCommandLineOptions(string[] args) {
            IDictionary<string, string> argsDict = ParseArgs(args);
            ICollection<string> keys = argsDict.Keys;
            CommandLineOptions options = new CommandLineOptions();

            options.Name = GetFirstMatchInDictionaryOrEmpty(argsDict, GetAliasesOnProperty("Name"));
            options.Color = GetFirstMatchInDictionaryOrEmpty(argsDict, GetAliasesOnProperty("Color"));
            options.ItemsToProcessDirectory = GetFirstMatchInDictionaryOrEmpty(argsDict, GetAliasesOnProperty("ItemsToProcessDirectory"));
            options.OptimizerCacheFile = GetFirstMatchInDictionaryOrEmpty(argsDict, GetAliasesOnProperty("OptimizerCacheFile"));

            options.DisplayHelp = keys.Any(key => GetAliasesOnProperty("DisplayHelp").Contains(key));
            options.StartListener = keys.Any(key => GetAliasesOnProperty("StartListener").Contains(key));
            options.SuppressCsvReport = keys.Any(key => GetAliasesOnProperty("SuppressCsvReport").Contains(key));
            options.ShouldForceOptimize = keys.Any(key => GetAliasesOnProperty("ShouldForceOptimize").Contains(key));

            return options;
        }

        /// <summary>
        /// Iterate through the cmd line args, and find if any of the given switches were passed. If so, return the value that was passed for that switch.
        /// </summary>
        /// <param name="argsDict">The args and values pulled from the cmd line.</param>
        /// <param name="cmdLineSwitches">The alias and short alias.</param>
        /// <returns>The value given by the user on the cmd line.</returns>
        private string GetFirstMatchInDictionaryOrEmpty(IDictionary<string, string> argsDict, IEnumerable<string> cmdLineSwitches) {

            foreach (string cmdLineSwitch in cmdLineSwitches) {
                if (argsDict.Keys.Contains(cmdLineSwitch)) {
                    return argsDict[cmdLineSwitch];
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get the cmd line switch aliases for this property.
        /// </summary>
        /// <param name="propertyName">The name of the property on CommandLineOptions</param>        
        private IEnumerable<string> GetAliasesOnProperty(string propertyName) {

            var cmdLineConfig = typeof(CommandLineOptions)
            .GetProperty(propertyName)
            .GetCustomAttribute<CommandLineArgAttribute>();
            return new[] { cmdLineConfig.ShortAlias, cmdLineConfig.Alias }.Where(x => x.Length > 0); //ensure non-empty string values from the attribute
        }
    }
}