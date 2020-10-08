using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using HRC.Common.Enums;
using HRC.IRIS.BL;

namespace EDRMSIntegration
{
    [ServiceContract(Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public interface IEDRMSIntegrationService
    {
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        EDRMSDocuments FindDocuments([MessageParameter(Name = "DocumentMetadata")] Metadata documentMetadata);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        string UploadDocument([MessageParameter(Name = "Document")] byte[] document, [MessageParameter(Name = "DocumentMetadata")] Metadata documentMetadata);

        [OperationContract]  
        [FaultContract(typeof(IRISServiceFaultContract))]
        EDRMSDocuments FindDocumentsByDocumentIDs([MessageParameter(Name = "DocumentIDs")] DocumentIDs documentIDs);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        ValidateEDRMSReferenceOutcome ValidateEDRMSReference([MessageParameter(Name = "EDRMSReference")] string EDRMSReference, [MessageParameter(Name = "ValidationType")] string validationType);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))] ValidateDocumentIDOutcome ValidateDocumentID([MessageParameter(Name = "DocumentID")] string DocumentID);
    }    

    [CollectionDataContract(Name = "Metadata", ItemName = "Item", KeyName = "Field", ValueName = "Value", Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class Metadata : Dictionary<string, string> { } 

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class IRISServiceFaultContract
    {
        public IRISServiceFaultContract(string errorMessage)
        {
            this.FaultCode = 1;
            this.FaultMessage = errorMessage;
        }

        [DataMember]
        public int FaultCode;

        [DataMember]
        public string FaultMessage;
    }

    public class ProcessedMetadata
    {        
        public string IrisId { get; set; }
        public string EDRMSReference { get; set; }

        internal string OriginalContactId { get; set; }
        public int ContactId { get; set; }
              
        public ReferenceDataValue ObjectType { get; set; }

        internal string OriginalObjectType { get; set; }        
        
        public string SubClassification1 { get; set; }
        public string SubClassification2 { get; set; }
        public string SubClassification3 { get; set; }

        public int DocumentReferenceNumber { get; set; }

        public List<int> ContactIDs { get; set; }
        public string OriginalContactIDs { get; set; }     
        public string ContactNames { get; set; }   

        public string DocumentName { get; set; }
        public string CreationDate { get; set; }   
        public string DocumentType { get; set; }   
        public string DocumentSubType { get; set; }
        public string FileType { get; set; }
        public string CreatedBy { get; set; }
        public string PersonResponsible { get; set; }
        public string Comments { get; set; }
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class EDRMSDocument
    {
        [DataMember]
        public string DocumentID { get; set; }
        
        [DataMember]
        public string Title { get; set; }
        
        [DataMember]
        public DateTime CreatedDate { get; set; }
        
        [DataMember]
        public string CreatedBy { get; set; }
        
        [DataMember]
        public string PersonResponsible { get; set; }
        
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string URL { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class ValidateEDRMSReferenceOutcome
    {
        [DataMember]
        public string EDRMSReferenceDescription { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [CollectionDataContract(Name = "EDRMSDocuments", ItemName = "Item", Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class EDRMSDocuments : List<EDRMSDocument> { }

    [CollectionDataContract(Name = "DocumentIDs", ItemName = "Item", Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class DocumentIDs : List<string> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/EDRMS/")]
    public class ValidateDocumentIDOutcome
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; } 
    }



}
