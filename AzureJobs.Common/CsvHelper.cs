namespace AzureJobs.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CsvHelper {
        public string Escape(string inputToClean) {
            const string DoubleQuote = "\"";
            const string EscapedItemFormat = "{0}{1}{0}";
            const string ItemsToCleanFor = ",\r\n\"";
            if (inputToClean.IndexOfAny(ItemsToCleanFor.ToCharArray()) > -1) {
                //wrap the input string in double quotes, while replacing any double quotes in the string with 2 double quotes
                return string.Format(EscapedItemFormat, DoubleQuote,
                                                        inputToClean.Replace(DoubleQuote, DoubleQuote + DoubleQuote));
            }

            return inputToClean;
        }
    }
}
