using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using Contacts = FinancialsIntegration.ContactsService;

namespace FinancialsIntegration
{
    [ServiceContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public interface IFinancialsIntegrationService
    {
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        CreateFinancialCustomerOutcome CreateFinancialCustomer([MessageParameter(Name = "ContactDetails")] Contacts.ContactDetails contactDetails);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        CreateFinancialProjectOutcome CreateFinancialProject([MessageParameter(Name = "RecordData")] RecordData recordData);

	    [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        CreateFinancialProjectOutcome CreateFinancialProjectWRC([MessageParameter(Name = "RecordDataWRC")] RecordDataWRC recordDataWRC);

	    [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        FinancialProjects FindFinancialProject([MessageParameter(Name = "SearchCriteria")] FinancialProjectSearchCriteria searchCriteria);
 
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        FinancialTimeCodes GetTimeRecordingCodes([MessageParameter(Name = "FINProjectCode")] string FINProjectCode);
 
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        FinancialProjectOrganisations GetProjectOrganisations();
 
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        void UpdateOfficerResponsible([MessageParameter(Name = "OfficerResponsibleDetailsCollection")] OfficerResponsibleDetailsCollection officerResponsibleDetailsCollection);
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class CreateFinancialCustomerOutcome
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
    
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/")]
    public class IRISServiceFaultContract
    {
        [DataMember]
        public int FaultCode;

        [DataMember]
        public string FaultMessage;
    }

    [DataContract(Name = "RecordData", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class RecordData
    {
        [DataMember]
        public string IrisID { get; set; }
 
        [DataMember]
        public string ObjectType { get; set; }
 
        [DataMember]
        public string Subclassification1 { get; set; }
 
        [DataMember]
        public string Subclassification2 { get; set; }
 
        [DataMember]
        public string Subclassification3 { get; set; }
 
        [DataMember]
        public string FINCustomerCode { get; set; }
 
        [DataMember]
        public string FINProjectCode { get; set; }
 
        [DataMember]
        public string OfficerResponsible { get; set; }

        [DataMember]    
        public string Description { get; set; }
    }

    [DataContract(Name = "RecordDataWRC", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class RecordDataWRC
    {
        [DataMember]
        public string IrisID { get; set; }
 
        [DataMember]
        public string ObjectType { get; set; }
 
        [DataMember]
        public string Subclassification1 { get; set; }
 
        [DataMember]
        public string Subclassification2 { get; set; }
 
        [DataMember]
        public string Subclassification3 { get; set; }
 
        [DataMember]
        public string FINCustomerCode { get; set; }
 
        [DataMember]
        public string FINProjectCode { get; set; }
 
        [DataMember]
        public string OfficerResponsible { get; set; }
 
        [DataMember]
        public string ProjectName { get; set; }
 
        [DataMember]
        public string ProjectProgrammeCode { get; set; }
 
        [DataMember]
        public string ProjectDescription { get; set; }
 
        [DataMember]
        public string PurchaseOrderNumber { get; set; }
 
        [DataMember]
        public string DomicileRegionalCouncilCode { get; set; }
 
        [DataMember]
        public long ContactID { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class CreateFinancialProjectOutcome
    {
        [DataMember]
        public bool Success { get; set; }
 
        [DataMember]
        public string ErrorMessage { get; set; }
    }


    public class FinancialProjectSearchCriteriaKey
    {
        public const string FINCustomerCode = "FINCustomerCode";
        public const string SearchText = "SearchText";
        public const string ObjectType = "ObjectType";
        public const string SubClassification1 = "SubClassification1";
        public const string SubClassification2 = "SubClassification2";
        public const string SubClassification3 = "SubClassification3";
    }

    [CollectionDataContract(Name = "FinancialProjectSearchCriteria", ItemName = "Item", KeyName = "Field", ValueName = "Value", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialProjectSearchCriteria : Dictionary<string, string>
    {
        public override string ToString()
        {
            string data = "Key/value pairs:\n";
            foreach (var entry in this)
            {
                data += string.Format("Key:\t{0}\tValue:\t{1}\n", entry.Key, entry.Value);
            }
            return data;
        }
    }


    [CollectionDataContract(Name = "FinancialProjects", ItemName = "FinancialProject", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialProjects : List<FinancialProject> 
    { 
    }
 
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialProject
    {
        [DataMember]
        public string ProjectCode { get; set; }
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string FINCustomerCode { get; set; }
        [DataMember]
        public string CustomerName { get; set; }
        [DataMember]
        public string Details { get; set; }
    }

    [CollectionDataContract(Name = "FinancialTimeCodes", ItemName = "FinancialTimeCode", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialTimeCodes : List<FinancialTimeCode> 
    { 
    }
     
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialTimeCode
    {
        [DataMember]
        public long TimeCodeNumber { get; set; }
        [DataMember]
        public string TimeCodeName { get; set; }
    }

    [CollectionDataContract(Name = "FinancialProjectOrganisations", ItemName = "FinancialProjectOrganisation", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialProjectOrganisations : List<FinancialProjectOrganisation> 
    { 
    }
 
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class FinancialProjectOrganisation
    {
        [DataMember]
        public long OrganisationID { get; set; }
        [DataMember]
        public string OrganisationName { get; set; }
    }

    [CollectionDataContract(Name = "OfficerResponsibleDetailsCollection", ItemName = "ChangedOfficerResponsibles", Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class OfficerResponsibleDetailsCollection: List<OfficerResponsibleDetails> 
    { 
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Financials/")]
    public class OfficerResponsibleDetails
    {
        [DataMember]
        public string FINProjectCode { get; set; }

        [DataMember]
        public string OfficerResponsible { get; set; }

        [DataMember]
        public DateTime DateTimeOfUpdate { get; set; }

        [DataMember]
        public string IrisID { get; set; }
    }






    



}
