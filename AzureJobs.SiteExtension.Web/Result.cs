using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureJobs.SiteExtension.Web
{
    public class Result
    {
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public int OriginalSize { get; set; }
        public int OptimizedSize { get; set; }

        public int Saving {
            get { return OriginalSize - OptimizedSize; }
        }
    }
}