using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AzureJobs.SiteExtension.Web
{
    public class Result
    {
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public int Original { get; set; }
        public int Optimized { get; set; }

        public int Saving
        {
            get { return Original - Optimized; }
        }

        public double Percent
        {
            get { return (double)Saving / (double)Original * 100; }
        }

        public string ShortFileName
        {
            get { return Path.GetFileName(FileName); }
        }
    }
}