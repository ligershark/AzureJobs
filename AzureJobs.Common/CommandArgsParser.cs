using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureJobs.Common {
    public class CommandArgsParser {

        // --file one.txt --name "azure image optimizer" --color Green
        // --help or /?
        public IDictionary<string, string> ParseArgs(string[] args) {
            IDictionary<string, string> result = new Dictionary<string, string>();

            string currentKey = null;
            StringBuilder currentValue = new StringBuilder();
            foreach (string argPart in args) {
                string fixedArg = argPart;

                if (fixedArg != null) { fixedArg = argPart.Trim(); }

                if (fixedArg.StartsWith("-")) {

                    if(!string.IsNullOrEmpty(currentKey)){
                        // apply current key / value to the dictionary
                        string value = currentValue.ToString().Trim();
                        if(string.IsNullOrEmpty(value)){
                            value = true.ToString();
                        }

                        result[currentKey] = value;
                        currentValue = new StringBuilder();
                    }
                                       
                    currentKey = fixedArg.ToLower();
                }
                else if (string.Compare(CommandLineOptions.HelpShortArg, fixedArg, StringComparison.OrdinalIgnoreCase) == 0 ) {
                    currentKey = CommandLineOptions.HelpArg;
                }
                else {
                    currentValue.Append(argPart);
                    currentValue.Append(" ");
                }
            }

            if(!string.IsNullOrEmpty(currentKey)){
                // apply current key / value to the dictionary
                string value = currentValue.ToString().Trim(); ;
                if(string.IsNullOrEmpty(value)){
                    value = true.ToString();
                }

                result.Add(currentKey, value);
            }

            return result;
        }

        public CommandLineOptions BuildCommandLineOptions(string[] args) {
            IDictionary<string, string> argsDict = ParseArgs(args);
            ICollection<string> keys = argsDict.Keys;
            CommandLineOptions options = new CommandLineOptions();
            if (keys.Contains(CommandLineOptions.HelpArg)) {
                options.DisplayHelp = true;
            }

            if (keys.Contains(CommandLineOptions.ColorArg)) {
                options.Color = argsDict[CommandLineOptions.ColorArg];
            }

            if (keys.Contains(CommandLineOptions.FolderArg)) {
                options.Folder = argsDict[CommandLineOptions.FolderArg];
            }

            if (keys.Contains(CommandLineOptions.LogFileArg)) {
                options.LogFile = argsDict[CommandLineOptions.LogFileArg];
            }

            if (keys.Contains(CommandLineOptions.NameArg)) {
                options.Name = argsDict[CommandLineOptions.NameArg];
            }

            if (keys.Contains(CommandLineOptions.StartListenerArg)) {
                options.StartListener = true;
            }

            if (keys.Contains(CommandLineOptions.NoReportArg)) {
                options.NoReport = true;
            }

            return options;
        }
    }
}