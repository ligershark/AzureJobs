using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace AzureJobs.Web.Minification
{
    public partial class Default : System.Web.UI.Page
    {
        private static string folder = HostingEnvironment.MapPath("~/Minification/files/");
        private static List<string> _extensions = new List<string>() { ".css", ".js" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            BindFile();
        }

        private void BindFile()
        {
            string file = Request.QueryString.ToString();

            if (!_extensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                return;

            if (!string.IsNullOrEmpty(file))
            {
                FileInfo info = new FileInfo(folder + file);
                string src = "/minification/files/" + file;
                long size = info.Length;
                preResult.InnerHtml = File.ReadAllText(info.FullName);
                aFile.Text = file + " (" + size + " bytes)";
                aFile.NavigateUrl = src;
            }
        }

        protected void Upload_Click(object sender, EventArgs e)
        {
            if (files.HasFile)
            {
                string path = Path.Combine(folder, files.PostedFile.FileName);
                files.PostedFile.SaveAs(path);
            }

            Response.Redirect(Request.Path + "?" + files.PostedFile.FileName, true);
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            foreach (string file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();

                if (_extensions.Contains(ext))
                    File.Delete(file);
            }

            Response.Redirect(Request.Path, true);
        }
    }
}