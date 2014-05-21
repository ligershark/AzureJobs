using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace AzureJobs.Web
{
    public partial class Default2 : System.Web.UI.Page
    {
        private static string folder = HostingEnvironment.MapPath("~/ImageOptimization/img/");
        private static List<string> _extensions = new List<string>() { ".jpg", ".jpeg", ".gif", ".png" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string file in Directory.EnumerateFiles(folder, "*.*").Reverse())
            {
                if (!_extensions.Contains(Path.GetExtension(file)))
                    continue;

                string src = "/imageoptimization/img/" + Path.GetFileName(file);
                long size = new FileInfo(file).Length;
                divImages.InnerHtml += string.Format("<span title='{1} bytes'><img src='{0}' /></span>", src, size);
                btnClear.Enabled = true;
            }
        }

        protected void Upload_Click(object sender, EventArgs e)
        {
            if (files.HasFiles)
            {
                foreach (var file in files.PostedFiles)
                {
                    string path = Path.Combine(folder, DateTime.Now.Ticks + Path.GetExtension(file.FileName));
                    file.SaveAs(path);
                }
            }

            Response.Redirect(Request.Path, true);
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