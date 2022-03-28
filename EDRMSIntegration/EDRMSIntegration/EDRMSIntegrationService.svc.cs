﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using System.Configuration;
using System.IO;
using HRC.Common;
using HRC.Common.Exceptions;
using HRC.Framework.BL;
using HRC.IRIS.BL;
using DocumentFormat.OpenXml.Packaging;

using DSOFile;      //  This needs to be registered using regsvr32.exe before adding the reference to this application

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.Office.Core;

namespace EDRMSIntegration
{
    public class EDRMSIntegrationService : IEDRMSIntegrationService
    {
        private const string AP = @"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        private const string DC = @"http://purl.org/dc/elements/1.1/";
        private const string CP = @"http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string UploadDocument(byte[] document, Metadata documentMetadata)
        {
            //log            
            string IrisBasePathPath = ConfigurationManager.AppSettings["IrisBasePathPath"];            
            string path = string.Empty;
            try
            {
                //upload doc
                ProcessedMetadata processedMetadata = ProcessMetadata(documentMetadata);
                int noOfContacts = (processedMetadata.ContactIDs != null)?processedMetadata.ContactIDs.Count:0;
                Logging.Write("UploadDocument", processedMetadata.DocumentName, "ContactId:" + processedMetadata.ContactId.ToString() + " : NoOfContacts:" + (processedMetadata.ContactIDs != null?processedMetadata.ContactIDs.Count.ToString():"0"), null);
                string suffix = "";
                string versionSuffix = " v01";
                string fileType = "." + processedMetadata.FileType.Replace(".",string.Empty); //ensure fileType has period
                if (noOfContacts==1){
                    //#BP 03/12/2015    Remove date stamp and contact name from suffix
                    //suffix = " " + DateTime.Now.ToString("yyyyMMdd") + " " + processedMetadata.ContactIDs[0].ToString() + " " + processedMetadata.ContactNames.ToString();
                    suffix = "";
                }
                else if (noOfContacts > 1 || noOfContacts == 0)
                {
                    //#BP 07/09/2015    Remove date stamp
                    //suffix = " " + DateTime.Now.ToString("yyyyMMdd");
                    suffix = "";
                } 
                IRISObject data = RetrieveIRISObject(processedMetadata);
                string tempPath = GetDocumentsPath(processedMetadata, data);                
                //singleDocument to multiple contacts vs mult
                //check to see if folder exists for that ID
                if (Directory.Exists(tempPath))
                {
                    //save file
                    while (File.Exists(Path.Combine(tempPath, string.IsNullOrEmpty(processedMetadata.DocumentSubType) ? "" : processedMetadata.DocumentSubType + ", " + processedMetadata.DocumentName + suffix + versionSuffix + fileType)))
                    {
                        versionSuffix = updateVersion(versionSuffix);
                    }
                    string filePath = Path.Combine(tempPath, string.IsNullOrEmpty(processedMetadata.DocumentSubType) ? "" : processedMetadata.DocumentSubType + ", " + processedMetadata.DocumentName + suffix + versionSuffix + fileType);
                    path = filePath;
                }
                else
                {
                    //create folder
                    DirectoryInfo info = Directory.CreateDirectory(tempPath);
                    if (info.Exists)
                    {
                        //save file
                        string filePath = Path.Combine(tempPath, string.IsNullOrEmpty(processedMetadata.DocumentSubType) ? "" : processedMetadata.DocumentSubType + ", " + processedMetadata.DocumentName + suffix + versionSuffix + fileType);
                        path = filePath;
                    }
                    else
                    {
                        //error creating folder                        
                        //need to determine if this is verbose or silently handled (ie email or DB logged)
                    }
                }

                if (Document.FileStorageMethod == DocumentFileStorage.FileSystem)
                {
                    Logging.Write("UploadDocument",processedMetadata.DocumentName,null,path);
                    File.WriteAllBytes(path, document);

                    string category = processedMetadata.ObjectType.Name.ToString();
                    category += string.IsNullOrEmpty(processedMetadata.SubClassification1) ? "" : ", " + processedMetadata.SubClassification1;
                    category += string.IsNullOrEmpty(processedMetadata.SubClassification2) ? "" : ", " + processedMetadata.SubClassification2;
                    category += string.IsNullOrEmpty(processedMetadata.SubClassification3) ? "" : ", " + processedMetadata.SubClassification3;

                    switch (processedMetadata.FileType.ToLower())
                    {
                        case ".doc":
                            Logging.Write("UploadDocument", processedMetadata.FileType, "unhandled doc type", null);
                            
                            DSOFile.OleDocumentProperties doc = new DSOFile.OleDocumentProperties();
                            doc.Open(path, false, dsoFileOpenOptions.dsoOptionDefault);
                            DSOFile.SummaryProperties summaryProp = doc.SummaryProperties;
                            summaryProp.Author = processedMetadata.CreatedBy;
                            summaryProp.Title = processedMetadata.DocumentName;
                            summaryProp.Comments = processedMetadata.Description;
                            summaryProp.Category = category;
                            summaryProp.Keywords = processedMetadata.DocumentReferenceNumber.ToString();
                            summaryProp.Subject = processedMetadata.DocumentType + ", " + processedMetadata.DocumentSubType;

                            //doc.Save();
                            doc.Close(true);
                            break;
                        case ".docx":
                            using (WordprocessingDocument wordDoc =
                                    WordprocessingDocument.Open(path, true))
                            {                              
                                var props = wordDoc.PackageProperties;
                                props.Creator = processedMetadata.CreatedBy; // +"," + processedMetadata.PersonResponsible;            
                                props.Title = processedMetadata.DocumentName;
                                props.Description = processedMetadata.Description;
                                
                                props.Category = category;
                                props.Keywords = processedMetadata.DocumentReferenceNumber.ToString();
                                props.Subject = processedMetadata.DocumentType + ", " + processedMetadata.DocumentSubType;
                                
                            }                           
                            break;
                        case ".xls":
                            Logging.Write("UploadDocument", processedMetadata.FileType, "unhandled xls type", null);
                            break;
                        case ".xlsx":
                            //Logging.Write("ExcelDocument", processedMetadata.DocumentName + processedMetadata.FileType, null, null);
                            //using (SpreadsheetDocument excelDoc =
                            //        SpreadsheetDocument.Open(path, true))
                            //{
                            //    var props = excelDoc.PackageProperties;
                            //    props.Creator = processedMetadata.CreatedBy;
                                
                            //    //props.Title = processedMetadata.DocumentName;
                            //    //props.Description = processedMetadata.Description;
                            //    //string category = processedMetadata.ObjectType.Name.ToString();
                            //    //category += string.IsNullOrEmpty(processedMetadata.SubClassification1) ? "" : ", " + processedMetadata.SubClassification1;
                            //    //category += string.IsNullOrEmpty(processedMetadata.SubClassification2) ? "" : ", " + processedMetadata.SubClassification2;
                            //    //category += string.IsNullOrEmpty(processedMetadata.SubClassification3) ? "" : ", " + processedMetadata.SubClassification3;
                            //    //props.Category = category;
                            //    //props.Keywords = processedMetadata.DocumentReferenceNumber.ToString();
                            //    //props.Subject = processedMetadata.DocumentType + ", " + processedMetadata.DocumentSubType;

                            //}
                            break;
                        case ".pdf":
                            break;
                        case ".xml":
                            break;
                        case ".msg":
                            break;
                           
                        default:
                            Logging.Write("UploadDocument", processedMetadata.FileType, "unhandled file type", null);
                            break;

                    }
                }
                else
                {
                    path = Path.GetFileName(path);
                }
                SaveDocument(data, string.IsNullOrEmpty(processedMetadata.DocumentSubType) ? "" : processedMetadata.DocumentSubType + ", " + processedMetadata.DocumentName, processedMetadata.PersonResponsible, path, document);                
            }
            catch (Exception ex)
            {
                Logging.Write("UploadDocument", "Error", string.Empty, ex.Message);            
               // throw new FaultException<IRISServiceFaultContract>(new IRISServiceFaultContract(ex.Message), ex.Message);                
            }
            Logging.Write("UploadDocument", "URL", path);
            return ((Document.FileStorageMethod == DocumentFileStorage.FileSystem) ? "file:" : "") + path;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string updateVersion(string versionSuffix)
        {
            int versionNo = 0;
            int.TryParse(versionSuffix.Replace(" v",string.Empty), out versionNo);
            versionNo++;
            versionSuffix = " v" + versionNo.ToString("D2");
            return versionSuffix;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  BP 19/11/2017   Substantial changes to accommodate special requirements for reporting documents associated with Regime and Regime Activities
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public EDRMSDocuments FindDocuments(Metadata documentMetadata)
        {
            Logging.Write("FindDocuments", "Step 0","Begin","Unpack IRIS document request metadata");

            EDRMSDocuments documents = new EDRMSDocuments();
            try
            {
                //  Unpack the incoming IRIS metadata dictionary into a set of object properties
                ProcessedMetadata processedMetadata = ProcessMetadata(documentMetadata);
                //  Find the associated IRIS object and populate an object with its properties
                IRISObject data = RetrieveIRISObject(processedMetadata);
                
                if (data != null)
                {
                    Logging.Write("FindDocuments", "Step 1", 
                        "Retrieved details for object type: " + data.IRISObjectType,
                        "Type: " + data.ObjectType + ", SubType: " + data.ObjectSubType,
                        data.Id);
                    //  Load documents from folder - first determine the path of the folder where the documents are stored for this object
                    string documentsPath = GetDocumentsPath(processedMetadata, data);
                    Logging.Write("FindDocuments", "Step 2", "Found documents Path", documentsPath, data.Id);

                    if (!Directory.Exists(documentsPath))
                    {// Check if folder exists else return empty documents collection
                        return documents;
                    }
                    DirectoryInfo dInfo = new DirectoryInfo(documentsPath);

                    int docCount = 1;
                    //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    //  #BP   28/08/2017  Enable the Legacy Monitoring Activities sub-folder to be surfaced in the list of documents for Regimes and Management Sites
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    if (processedMetadata.ObjectType.Name == "Regime" || processedMetadata.ObjectType.Name == "Management Site")
                    {
                        foreach (DirectoryInfo dirInfo in dInfo.GetDirectories())
                        {
                            if (dirInfo.Name == "Legacy Monitoring Activities")
                            {// Load the folder path into an EDRMS document object (treat it just like a file)
                                EDRMSDocument doc = new EDRMSDocument();
                                doc.CreatedDate = dirInfo.CreationTime;
                                doc.CreatedBy = "";
                                doc.DocumentID = "";
                                docCount++;
                                doc.URL = dirInfo.FullName;
                                doc.Title = "\\" + dirInfo.Name;
                                doc.PersonResponsible = "";
                                doc.Status = "";
                                documents.Add(doc);
                            }
                        }
                    }
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    Logging.Write("FindDocuments", "Step 3", "Read files in folder","ObjectType: "+processedMetadata.ObjectType.Name,data.Id);
                    string exceptionMessage = "";
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    //  If it's a Regime, load all the files from the sub-directories (not recursive at this stage - let's see how it goes first)
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    if (processedMetadata.ObjectType.Name == "Regime" || processedMetadata.ObjectType.Name == "Management Site")
                    {
                        foreach (DirectoryInfo dirInfo in dInfo.GetDirectories())
                        {
                            //  Don't load the files from the historic files folder
                            if (!(dirInfo.Name == "Legacy Monitoring Activities"))
                            {
                                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                                {
                                    EDRMSDocument doc = new EDRMSDocument();
                                    exceptionMessage = AttachDocumentProperties(fileInfo, data, doc);
                                    docCount++;
                                    doc.URL = Path.Combine(documentsPath, fileInfo.FullName);
                                    documents.Add(doc);
                                }
                            }
                        }
                        if (exceptionMessage.Length > 0)
                        {
                            Logging.Write("FindDocuments", "Step 3.1", "Errors during file listing", exceptionMessage, data.Id);
                        }
                    }
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    //  These are the documents from the root folder for the nominated IRIS object
                    //  ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                    foreach (FileInfo fileInfo in dInfo.GetFiles())
                    {
                        EDRMSDocument doc = new EDRMSDocument();
                        exceptionMessage = AttachDocumentProperties(fileInfo, data, doc);
                        Logging.Write("FindDocuments", "Step 3.1", "exceptionMessage", exceptionMessage, data.Id);
                        doc.URL = Path.Combine(documentsPath, fileInfo.FullName);
                        docCount++;
                        documents.Add(doc);
                    }
                }
            }
            catch (Exception ex)
            {                
                Logging.Write("FindDocuments", "Error", null, ExceptionInformation.GetExceptionStack(ex));
                throw new FaultException<IRISServiceFaultContract>(new IRISServiceFaultContract(ex.Message), "Service Error");
            }
            return documents;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  #BP 19/11/2017  Scan Word, Excel and PDF documents for the Author and populate the Person Responsible field
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string AttachDocumentProperties(FileInfo fileInfo,IRISObject data,EDRMSDocument doc)
        {
            string exceptionMessage = "";
            //Use the next line of code if we want to default to the owner of the document in the file system when the author is not available
            //string user = System.IO.File.GetAccessControl(fileInfo.FullName).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
            string user = "";
            try
            {
                switch (fileInfo.Extension)
                {
                    case ".doc":
                    case ".xls":
                        DSOFile.OleDocumentProperties mdoc = new DSOFile.OleDocumentProperties();
                        mdoc.Open(fileInfo.FullName, false, dsoFileOpenOptions.dsoOptionDefault);
                        user = mdoc.SummaryProperties.Author;
                        mdoc.Close(true);
                        break;
                    case ".docx":
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(fileInfo.FullName, true))
                        {
                            var props = wordDoc.PackageProperties;
                            user = props.Creator;
                        }
                        break;
                    case ".xlsx":
                        using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(fileInfo.FullName, true))
                        {
                            var props = excelDoc.PackageProperties;
                            user = props.Creator;
                        }
                        break;
                    case ".pdf":
                        using (PdfDocument pdfDoc = PdfReader.Open(fileInfo.FullName))
                        {
                            user = pdfDoc.Info.Author;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                    exceptionMessage = ex.Message;
            }
            doc.DocumentID = GetDocumentIDFromPath(fileInfo.FullName, data, user);
            doc.CreatedDate = fileInfo.CreationTime;
            doc.CreatedBy = "";
            doc.Title = fileInfo.Name;
            doc.PersonResponsible = user;
            doc.Status = "";
            return exceptionMessage;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  #BP 19/11/2017  Lookup or create a unique ID for each document based on its full file path
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string GetDocumentIDFromPath(string DocumentFullPath, IRISObject data, String owner)
        {
            Document myDocument = new Document();
            DocumentVersion myVersion = new DocumentVersion();
            myVersion.VersionId = Guid.NewGuid();
            myVersion.CreatedDate = DateTime.Now;
            myVersion.CreatedBy = owner;
            myVersion.ModifiedDate = myVersion.CreatedDate;
            myVersion.ModifiedBy = myVersion.CreatedBy;
            myDocument.Versions.Add(myVersion);

            myDocument.IrisId = data.Id;
            myDocument.Name = Path.GetFileName(DocumentFullPath);
            myDocument.DocumentFullPath = DocumentFullPath;

            if (data.IRISObjectType != null)
            {
                //use BusinessID if it exists
                //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                //#BP 15/09/2017    Inserted code here to handle folders for Regime Activities
                //if (!string.IsNullOrEmpty(data.BusinessId))
                //{
                 //    myDocument.GetDocumentIDFromPath(DocumentFullPath,data.Id,owner);
                //}
            }
            return myDocument.Id.ToString();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string GetDocumentsPath(ProcessedMetadata processedMetadata, IRISObject data)
        {
            string IrisBasePathPath = ConfigurationManager.AppSettings["IrisBasePathPath"];
            string documentsPath = "";
            if (processedMetadata.ObjectType != null)
            {
                if (ReferenceDataCollection.IsContact(processedMetadata.ObjectType))
                {
                    //Contact subfolder. For Contacts use ContactID as folder name
                    string contactId = processedMetadata.ContactIDs != null ? processedMetadata.ContactIDs[0].ToString():"";
                    if (processedMetadata.ContactId > 0)  contactId=processedMetadata.ContactId.ToString();
                    documentsPath = Path.Combine(IrisBasePathPath, GetEDRMSFolderName(processedMetadata.ObjectType), contactId);
                }
                else
                {
                    //use BusinessID if it exists
                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    //#BP 15/09/2017    Inserted code here to handle folders for Regime Activities
                    if (!string.IsNullOrEmpty(data.BusinessId))
                    {
                        string folderName = data.BusinessId.ToString();
                        switch (processedMetadata.ObjectType.Id)
                        {
                            case 13:
                                folderName = data.BusinessId.ToString() + "\\" + data.ObjectSubType.ToString();
                                break;
                            default:
                                folderName = data.BusinessId.ToString();
                                break;
                        }
                        documentsPath = Path.Combine(IrisBasePathPath, GetEDRMSFolderName(processedMetadata.ObjectType), folderName);
                        //-------------------------------------------------------------------------------------------------------------------------------------------------
                        //documentsPath = Path.Combine(IrisBasePathPath, GetEDRMSFolderName(processedMetadata.ObjectType), data.BusinessId.ToString());
                    }
                    else
                    {
                        //what do we do when BusinessID is blank (use IRIS Id)
                        documentsPath = Path.Combine(IrisBasePathPath, GetEDRMSFolderName(processedMetadata.ObjectType), data.Id.ToString());
                        Logging.Write("FindDocuments", "ObjType: " + processedMetadata.ObjectType.Name, "No BusinessID found for " + data.Id);
                    }
                }
            }
            return documentsPath;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string GetEDRMSFolderName(ReferenceDataValue objectType){
            /*****
                ObjectTypeID	ObjectCode	                ObjectName
                *10	            Activity	                Activity
                *23	            AdHocData	                Ad Hoc Data
                *5	            Application	                Application
                *6	            Authorisation	            Authorisation
                -7	            AuthorisationGroup	        Authorisation Group
                -11	            ConditionSchedule	        Condition Schedule
                *1	            Contact	                    Contact
                *2	            ContactGroup	            Contact Group
                -21	            ContactGroupMember	        Contact Group Member
                *17	            DamRegister	                Dam Register
                *25	            Enforcement	                Enforcement
                *27	            EnforcementAction	        Enforcement Action
                *26	            EnforcementAllegedOffence	Enforcement Alleged Offence
                *22	            GeneralRegister	            General Register
                -28	            JointFinancialCustomer	    Joint Financial Customer
                *3	            Location	                Location
                *4	            LocationGroup	            Location Group
                *16 	        ManagementSite	            Management Site
                -9	            MapContext	                Map Context
                -14	            Observation	                Observation
                *15	            Programme	                Programme
                -24	            PropertyDataValuation	    Property Data Valuation
                *12	            Regime	                    Regime
                *13	            RegimeActivity	            Regime Activity
                *20	            Request	                    Request
                -19	            SampleResult	            Sample Result
                *18	            SelectedLandUseSite	        Selected Land Use Site
                -8	            User	                    User
             * ***/
            //string folderName = "";
            switch (objectType.Id)
            {
                case 1: //Contacts
                    return "contacts";
                case 2: //Contact Groups
                    return "contact groups";
                case 3: //Location
                    return "locations";
                case 4: //Location Groups
                    return "location groups";
                case 5: //Applications
                    return "applications";
                case 6: //Authorisation
              //case 7:
                    return "authorisations";
                case 10: //Activity
                    return "activity";
                case 12: //Regime
                case 13: //Regime Activity
                    return "regimes";
                case 15: //programme
                    return "programme";
                case 16: //Management Site
                case 18: //Selected Land Use Site
                    return "management sites";
                case 17: // DamRegister
                case 22: // General Register
                    return "registers";
                case 20: //Request 
                    return "requests";
                case 23: //Adhoc Data
                    return "adhoc data";
                case 25: //Enforcement
                case 26: //EnforcementAllegedOffence
                case 27: //EnforcementAction
                    return "enforcement";
                default:
                     return objectType.Name.ToLower();
            }

            //return objectType.Name.ToLower(); ;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private IRISObject RetrieveIRISObject(ProcessedMetadata processedMetadata)
        {
            IRISObject data = null;

            if (processedMetadata.ObjectType != null)
            {                
                if (ReferenceDataCollection.IsContact(processedMetadata.ObjectType))
                {
                    if (processedMetadata.ContactId.Equals(default(int)))
                    {
                        if (processedMetadata.ContactIDs.Count > 0)
                        {
                            Logging.Write("RetrieveIRISObject", "LoadFromContactId", "");
                            data = IRISObject.LoadFromContactId(processedMetadata.ContactIDs[0]);
                        }
                        else
                        {
                            //error
                            Logging.Write("RetrieveIRISObject", "Error*", null, string.Format("Object Type: {0} not found", processedMetadata.OriginalObjectType));
                        }
                    }
                    else
                    {
                        Logging.Write("RetrieveIRISObject", "LoadFromContactId", ""+ processedMetadata.ContactId);
                        data = IRISObject.LoadFromContactId(processedMetadata.ContactId);
                    }
                }
                else
                { //use businessID, replaces using EDRMSReference as we are not using this as present
                    Logging.Write("RetrieveIRISObject", "LoadFromBusinessID", processedMetadata.IrisId);
                    data = IRISObject.LoadFromBusinessID(processedMetadata.IrisId);
                    //data = IRISObject.LoadFromEDRMSReference(processedMetadata.EDRMSReference);
                    //Logging.Write("RetrieveIRISObject3", string.Empty, "IrisId: " + processedMetadata.IrisId.ToString() + " ContactId" + processedMetadata.ContactId.ToString() + "EDRMS Ref:" + processedMetadata.EDRMSReference);
                }
            }
            else
            {
                Logging.Write("RetrieveIRISObject", "Error", null, string.Format("Object Type: {0} not found", processedMetadata.OriginalObjectType));
            }
            return data;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public EDRMSDocuments FindDocumentsByDocumentIDs(DocumentIDs documentIDs)
        {
            EDRMSDocuments documents = new EDRMSDocuments();
            try
            {
                //find docs
                List<int> documentIdsConverted = documentIDs.Select(int.Parse).ToList();
                foreach (Document document in Document.Load(documentIdsConverted, false))
                {
                    documents.Add(AddDocument(document));
                }

                //log
                Logging.Write("FindDocumentsByDocumentIDs", documentIDs != null ? documentIDs.ToSvString() : string.Empty, string.Empty); 
            }
            catch (Exception ex)
            {
                Logging.Write("FindDocuments", "Error", null, ex.Message);
                throw new FaultException<IRISServiceFaultContract>(new IRISServiceFaultContract(ex.Message), "Service Error");
            }
            return documents;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ValidateDocumentIDOutcome ValidateDocumentID(string DocumentID)
        {
            ValidateDocumentIDOutcome outcome = new ValidateDocumentIDOutcome();
            outcome.Success = true;            
            try
            {
                //validate document            

                //log
                Logging.Write("ValidateDocumentID", DocumentID, string.Empty); 
            }
            catch (Exception ex)
            {
                
                throw new FaultException<IRISServiceFaultContract>(new IRISServiceFaultContract(ex.Message), "Service Error");
            }

            return outcome;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ValidateEDRMSReferenceOutcome ValidateEDRMSReference(string EDRMSReference, string validationType)
        {
            ValidateEDRMSReferenceOutcome outcome = new ValidateEDRMSReferenceOutcome();
            outcome.EDRMSReferenceDescription = "";
            try
            {
                //validate Validate EDRMS Reference                

                //log
                Logging.Write("ValidateEDRMSReference", string.Format("EDRMSReference: {0}, ValidationType: {1}", EDRMSReference, validationType), string.Empty); 
            }
            catch (Exception ex)
            {
                throw new FaultException<IRISServiceFaultContract>(new IRISServiceFaultContract(ex.Message), "Service Error");
            }

            return outcome;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private ProcessedMetadata ProcessMetadata(Metadata documentMetadata)
        {
            ProcessedMetadata document = new ProcessedMetadata();
            if (documentMetadata != null)
            {                
                foreach (KeyValuePair<string, string> prop in documentMetadata)
                {
                    if (prop.Key.Equals("IRISID", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.IrisId = prop.Value;
                    }
                    else if (prop.Key.Equals("EDRMSReference", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.EDRMSReference = prop.Value;
                    }
                    else if (prop.Key.Equals("ContactID", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.OriginalContactId= prop.Value;
                        int contactId = 0;
                        if (int.TryParse(prop.Value, out contactId))
                        {
                            document.ContactId = contactId;
                        }
                    }
                    else if (prop.Key.Equals("ObjectType", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.OriginalObjectType = prop.Value;                        
                        document.ObjectType = ReferenceDataCollection.LoadObjectTypeFromName(prop.Value);
                    }
                    else if (prop.Key.Equals("SubClassification1", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.SubClassification1 = prop.Value;
                    }
                    else if (prop.Key.Equals("SubClassification2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.SubClassification2 = prop.Value;
                    }
                    else if (prop.Key.Equals("SubClassification3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.SubClassification3 = prop.Value;
                    }
                        else if (prop.Key.Equals("ContactIDs", StringComparison.InvariantCultureIgnoreCase))
                        {
                            document.OriginalContactIDs = prop.Value;
                            document.ContactIDs = new List<int>(prop.Value.Split(',').Select(s => int.Parse(s)));
                        }
                        else if (prop.Key.Equals("ContactNames", StringComparison.InvariantCultureIgnoreCase))
                        {
                            document.ContactNames = prop.Value;
                        }
                        else if (prop.Key.Equals("DocumentName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.DocumentName = prop.Value;
                    }
                    else if (prop.Key.Equals("CreationDate", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.CreationDate = prop.Value;
                    }
                    else if (prop.Key.Equals("DocumentType", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.DocumentType = prop.Value;
                    }
                    else if (prop.Key.Equals("DocumentSubType", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.DocumentSubType = prop.Value;
                    }
                    else if (prop.Key.Equals("FileType", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.FileType = prop.Value;
                    }
                    else if (prop.Key.Equals("CreatedBy", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.CreatedBy = prop.Value;
                    }
                    else if (prop.Key.Equals("PersonResponsible", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.PersonResponsible = prop.Value;
                    }
                    else if (prop.Key.Equals("Comments", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.Comments = prop.Value;
                    }
                    else if (prop.Key.Equals("Description", StringComparison.InvariantCultureIgnoreCase))
                    {
                        document.Description = prop.Value;
                    }
                }
            }
            return document;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private EDRMSDocument AddDocument(Document document)
        {
            EDRMSDocument doc = new EDRMSDocument();
            doc.CreatedDate = document.CreatedDate;
            doc.CreatedBy = document.CreatedBy;
            doc.DocumentID = document.Id.ToString();            
            doc.URL = document.URL;
            doc.Title = document.Name;
            doc.PersonResponsible = document.Owner;
            doc.Status = document.Status.ToString();
            return doc;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void SaveDocument(IRISObject data, string documentName, string owner, string path, byte[] file)
        {
            Document document = new Document();

            document.DocumentFullPath = path;
            document.IrisId = data.Id;
            document.Name = documentName;
            document.Owner = owner;
            document.Status = DocumentStatus.Open;

            DocumentVersion version = new DocumentVersion();
            version.VersionId = Guid.NewGuid();
            version.CreatedDate = DateTime.Now;
            version.CreatedBy = owner;
            version.ModifiedDate = version.CreatedDate;
            version.ModifiedBy = version.CreatedBy;
            version.Document = file;
            document.Versions.Add(version);
            document.Save(version);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
