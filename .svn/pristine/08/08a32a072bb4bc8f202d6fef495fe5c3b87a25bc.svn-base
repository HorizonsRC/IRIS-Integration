using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.UI;
using System.IO;
using HRC.Common;
//using System.Web.UI.WebControls;

namespace EDRMSDocumentLoader
{
    public class ResponseHelper
    {
        
        public static string ExportAndOpen(Page page, byte[] file, string fileName)
        {
            string path = page.Request.PhysicalApplicationPath + @"Temp\" + fileName;

            System.IO.File.WriteAllBytes(path, file);

            string directoryPath = Path.GetDirectoryName(page.Request.Url.PathAndQuery).Replace(@"\", @"/");

            if (!directoryPath.SafeSubstring(directoryPath.Length - 1, 1).Equals(@"/"))
            {
                directoryPath += @"/";
            }

            path = string.Format("{0}{1}{2}{3}Temp/{4}",
                page.Request.Url.Scheme,
                Uri.SchemeDelimiter, 
                page.Request.Url.Authority,
                directoryPath,
                fileName);

            Redirect(path, "_self", "");
            return path;
        }
        
        public static void Redirect(string url, string target)
        {
            Redirect(url, target, string.Empty);
        }

        public static void Redirect(string url, string target, string windowFeatures)
        {
            HttpContext context = HttpContext.Current;
            if ((string.IsNullOrEmpty(target) || target.Equals("_self", StringComparison.OrdinalIgnoreCase)) && string.IsNullOrEmpty(windowFeatures))
            {
                context.Response.Redirect(url);
            }
            else
            {
                Page page = (Page)context.Handler;
                if (page == null)
                {
                    throw new InvalidOperationException("Cannot redirect to new window outside Page context.");
                }
                url = page.ResolveClientUrl(url);
                string script;
                if (!string.IsNullOrEmpty(windowFeatures))
                {
                    script = @"window.open(""{0}"", ""{1}"", ""{2}"");";
                }
                else
                {
                    script = @"window.open(""{0}"", ""{1}"");";
                }
                script = String.Format(script, url, target, windowFeatures);
                ScriptManager.RegisterStartupScript(page,
                    typeof(Page),
                    "Redirect",
                    script,
                    true);
            }
        }
    }
}