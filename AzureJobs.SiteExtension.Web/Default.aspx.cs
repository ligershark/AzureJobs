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
        private static string _file = ConfigurationManager.AppSettings.Get("file");
        private static string _name = ConfigurationManager.AppSettings.Get("name");
        private IEnumerable<Result> _results;

        protected void Page_Load(object sender, EventArgs e)
        {
            _results = GetResults();

            if (IsPostBack)
                return;

            if (Request.QueryString["download"] != null)
            {
                Response.ContentType = "text/plain";
                Response.WriteFile(_file);
            }
            else
            {
                filesProcessed.Text = _results.Count().ToString("#,#0");
                filesOptmized.Text = _results.Where(r => r != null && r.Saving > 0).Count().ToString("#,#0");
                totalSavings.Text = _results.Where(r => r!= null).Sum(r => r.Saving).ToString("#,#0");
                
                name.Text = _name;
                error.Visible = !File.Exists(_file);
                success.Visible = !error.Visible;
            }
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

            var lines = File.ReadAllLines(_file).Reverse();

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
            }

            Response.Redirect("/", true);
        }
    }
}