using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using HRC.Common.Security;
using HRC.Framework.BL;

namespace EDRMSDocumentLoader
{
    public partial class DocLoader : System.Web.UI.Page
    {
        private const int documentExpirySeconds = 60;
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((!Page.IsPostBack) && (!Page.IsCallback))
            {
                DateTime currentDateTime = DateTime.Now;

                string documentId = Request.QueryString["Document"];
                string encryptedToken = Request.QueryString["Token"];

                if (!string.IsNullOrEmpty(documentId) && !string.IsNullOrEmpty(encryptedToken))
                {
                    DateTime dateTime = Document.DecryptToken(encryptedToken);

                    double seconds = (currentDateTime.Subtract(dateTime)).TotalSeconds;

                    if (Math.Abs(seconds) <= documentExpirySeconds)
                    {                        
                        int Id;
                        if (int.TryParse(documentId, out Id))
                        {
                            Document document = Document.Load(Id, true);
                            if (document != null)
                            {
                                if (document.CurrentVersion != null)
                                {
                                    DocumentVersion version = document.CurrentVersion;
                                    Document.LoadDocument(ref version);

                                    string documentName = Path.GetFileName(document.DocumentFullPath);

                                    string path = ResponseHelper.ExportAndOpen(this.Page, version.Document, documentName);

                                }
                            }
                            else
                            {
                                labelError.Text = string.Format("Error: Document Id: {0} not found", Id);
                            }
                        }
                        else
                        {
                            labelError.Text = "Error: Invalid DocumentId";
                        }
                    }
                    else
                    {
                        labelError.Text = "Error: Expired Token";
                    }
                }
                else
                {
                    labelError.Text = "Error: Empty DocumentId or Token";
                }
            }
        }



        
    }
}