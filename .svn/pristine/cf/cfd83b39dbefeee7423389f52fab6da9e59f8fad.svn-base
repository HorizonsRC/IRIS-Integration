using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using HRC.Common.Security;
using HRC.Framework.BL;

namespace EDRMSDocumentLoader
{
    public partial class Test : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((!Page.IsPostBack) && (!Page.IsCallback))
            {
                HyperLink1.Text = "click here";
                string documentId = "21";
                string token = Document.EncryptToken();                
                string pageUrl = string.Format("DocLoader.aspx?Token={0}&Document={1}", token, documentId);
                HyperLink1.NavigateUrl = pageUrl;
                HyperLink1.Target = "_blank";
            }
        }
    }
}