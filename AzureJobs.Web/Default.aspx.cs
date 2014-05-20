using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace AzureJobs.Web
{
    public partial class Default : System.Web.UI.Page
    {
        private static string folder = HostingEnvironment.MapPath("~/img/");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string file in Directory.EnumerateFiles(folder, "*.*").Reverse())
            {
                string src = "/img/" + Path.GetFileName(file);
                long size = new FileInfo(file).Length;
                divImages.InnerHtml += string.Format("<span title='{1} bytes'><img src='{0}' /></span>", src, size);
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
            Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            Response.Redirect(Request.Path, true);
        }
    }
}