using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HRC.Common;
using HRC.Framework.DL;
using HRC.Common.Security;

namespace HRC.Framework.BL
{   
    public class Document : IdNameBase<int>
    {
       // public static DocumentFileStorage FileStorageMethod = DocumentFileStorage.DatabaseFileStream;
        public static DocumentFileStorage FileStorageMethod = DocumentFileStorage.FileSystem;
        private static string DocumentTokenExpiryDateFormat = "ddMMyyyy HH:mm:ss";

        public long IrisId { get; set; }
        
        public Guid CurrentVersionId
        {
            get
            {
                return this.CurrentVersion.VersionId;
            }
        }

        public string URL 
        {
            get
            {
                if (this.CurrentVersion.FileStorageMethod == DocumentFileStorage.FileSystem)
                {
                    return this.DocumentFullPath;
                }
                else if (this.CurrentVersion.FileStorageMethod == DocumentFileStorage.DatabaseBinary ||
                         this.CurrentVersion.FileStorageMethod == DocumentFileStorage.DatabaseFileStream)
                {
                    //Passing Id to DocumentPortal
                    string documentId = this.Id.ToString();
                    string token = EncryptToken();
                    return string.Format("HRC/DocLoader/DocLoader.aspx?Token={0}&Document={1}", token, documentId);
                }
                return string.Empty;
            }
        }

        public string DocumentFullPath { get; set; }

        public List<DocumentVersion> Versions { get; set; }

        public Document()
        {
            this.Versions = new List<DocumentVersion>();
        }

        public DateTime CreatedDate
        {
            get
            {
                return this.CreatedVersion.CreatedDate;
            }
        }

        public string CreatedBy
        {
            get
            {
                return this.CreatedVersion.CreatedBy;
            }
        }

        private DocumentVersion CreatedVersion
        {
            get
            {
                return this.Versions.OrderByDescending(v => v.CreatedDate).FirstOrDefault();
            }
        }

        public DocumentVersion CurrentVersion
        {
            get
            {
                return this.Versions.OrderByDescending(v => v.CreatedDate).LastOrDefault();
            }
        }

        public string Owner { get; set; }

        public DocumentStatus Status { get; set; }

        public static Document Load(int Id)
        {
            return Load(Id, false);
        }

        public static Document Load(int Id, bool loadDocument)
        {
            List<Document> documents = Document.Load(new List<int>() { Id }, loadDocument);
            return documents.Count > 0 ? documents[0] : null;
        }

        public static List<Document> Load(List<int> documentIds, bool loadDocument)
        {
            return DocumentManager.Load(documentIds, loadDocument);
        }

        public static List<Document> LoadFromIrisId(long IrisId)
        {
            return DocumentManager.Load(IrisId, false);
        }

        public void Save(DocumentVersion version)
        {
            DocumentManager.Save(this, version);
        }

        public static void LoadDocument(ref DocumentVersion version)
        {
            if (version.FileStorageMethod == DocumentFileStorage.DatabaseFileStream)
            {
                DocumentManager.LoadFileStream(ref version);
            }
        }

        public static string EncryptToken()
        {   
            string dateTime = DateTime.Now.ToString(Document.DocumentTokenExpiryDateFormat);
            return Encryption.UrlEncrypt(dateTime);
        }

        public static DateTime DecryptToken(string value)
        {
            string token = Encryption.UrlDecrypt(value);
            return DateTime.ParseExact(token, Document.DocumentTokenExpiryDateFormat, null);
        }

    }

    public class DocumentVersion 
    {
        public int DocumentId { get; set; }
        
        public Guid VersionId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        public byte[] Document { get; set; }
        
        public DocumentFileStorage FileStorageMethod { get; set; }

        public void SaveFile()
        {
            DocumentManager.SaveFile(this);            
        }
    }

    //place holder statuses, need to determine what our statuses will be
    public enum DocumentStatus : short
    {
        Open = 1,
        Closed = 2,        
    }

}
