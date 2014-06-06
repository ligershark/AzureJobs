using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace AzureJobs.SiteExtension.Web
{
    public partial class Default : System.Web.UI.Page
    {
        private const string _fileName = "ImageCompressor.Job.exe.csv";
        private static string _folder = ConfigurationManager.AppSettings.Get("folder");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["file"] == null)
            {
                string first = Directory.EnumerateFiles(_folder, "*.csv", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (!string.IsNullOrEmpty(first))
                {
                    Response.Redirect("?file=" + Path.GetFileName(first));
                }
            }

            foreach (string file in Directory.EnumerateFiles(_folder, "*.csv", SearchOption.TopDirectoryOnly))
            {
                var li = new HtmlGenericControl("li");
                var a = new HtmlAnchor();
                a.InnerHtml = Path.GetFileName(file).Substring(0, Path.GetFileName(file).IndexOf('.'));
                a.HRef = "?" + a.InnerHtml;

                li.Controls.Add(a);
                menu.Controls.Add(li);
            }
        }

        // The return type can be changed to IEnumerable, however to support
        // paging and sorting, the following parameters must be added:
        //     int maximumRows
        //     int startRowIndex
        //     out int totalRowCount
        //     string sortByExpression
        public IQueryable<AzureJobs.SiteExtension.Web.Result> grid_GetData()
        {
            return GetResults().Where(r => r != null && r.Saving > 0).AsQueryable();
        }

        private IEnumerable<Result> GetResults()
        {
            string file = Path.Combine(_folder, Request.QueryString["file"]);

            var lines = File.ReadAllLines(file).Reverse();

            foreach (string line in lines)
            {
                string[] args = line.Split(',');

                yield return CreateResult(args);
            }
        }

        private Result CreateResult(string[] args)
        {
            if (args.Length != 4)
                return null;

            DateTime date;
            int original, optimized;

            Result result = new Result();

            if (!DateTime.TryParse(args[0], out date))
                return null;

            result.Date = date;
            result.FileName = HttpUtility.UrlDecode(args[1]);

            if (!int.TryParse(args[2], out original))
                return null;

            result.OriginalSize = original;

            if (!int.TryParse(args[3], out optimized))
                return null;

            result.OptimizedSize = optimized;

            return result;
        }
    }
}