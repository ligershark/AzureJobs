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
                string trimmedArgPart = argPart;

                if (trimmedArgPart != null) { trimmedArgPart = argPart.Trim(); }

                if (trimmedArgPart.StartsWith("-")) {

                    if(!string.IsNullOrEmpty(currentKey)){
                        // apply current key / value to the dictionary
                        string value = currentValue.ToString().Trim();
                        if(string.IsNullOrEmpty(value)){
                            value = true.ToString();
                        }

                        result[currentKey] = value;
                        currentValue = new StringBuilder();
                    }
                                       
                    currentKey = trimmedArgPart;
                }
                else if(trimmedArgPart.Equals("/?")){
                    currentKey = "--help";
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
    }
}