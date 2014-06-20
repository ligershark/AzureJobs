using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace AzureJobs.SiteExtension.Web
{
    public partial class Default : System.Web.UI.Page
    {
        protected static string _color = ConfigurationManager.AppSettings.Get("color");
        protected static string _logo = File.ReadAllText(HostingEnvironment.MapPath("~/img/logo.svg"));
        protected static string _name = ConfigurationManager.AppSettings.Get("name");
        private static string _file = ConfigurationManager.AppSettings.Get("file");
        private static IEnumerable<Result> _results;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["download"] != null)
                DownloadLogFile();

            try
            {
                _results = GetResults().Reverse();
            }
            catch
            {
                // File is locked because it's being written to. Ignore this and use the _results from last load (it's static).
            }

            if (_results != null)
            {
                filesProcessed.Text = _results.Count(r => r != null).ToString("#,#0");
                filesOptmized.Text = _results.Where(r => r != null && r.Saving > 0).Count().ToString("#,#0");
                totalSavings.Text = _results.Where(r => r != null).Sum(r => r.Saving).ToString("#,#0");

                double percent = double.Parse(totalSavings.Text) / (double)_results.Where(r => r != null).Sum(r => r.Original) * 100;
                totalPercent.Text = percent.ToString("#0.0");

                name.Text = _name;
                error.Visible = !File.Exists(_file);
                success.Visible = !error.Visible;
            }
        }

        private void DownloadLogFile()
        {
            Response.ContentType = "application/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=optimizations.csv");
            Response.WriteFile(_file);
            Response.End();
        }

        public IQueryable<AzureJobs.SiteExtension.Web.Result> grid_GetData()
        {
            if (_results == null)
                return null;

            return _results.Where(r => r != null && r.Saving > 0).AsQueryable();
        }

        private IEnumerable<Result> GetResults()
        {
            if (!File.Exists(_file))
                yield break;

            using (FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] args = line.Split(',');
                    yield return CreateResult(args);
                }
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

            result.Original = original;

            if (!int.TryParse(args[3], out optimized))
                return null;

            result.Optimized = optimized;

            return result;
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (File.Exists(_file))
            {
                File.Delete(_file);
                _results = null;
            }

            Response.Redirect("/", true);
        }
    }
}