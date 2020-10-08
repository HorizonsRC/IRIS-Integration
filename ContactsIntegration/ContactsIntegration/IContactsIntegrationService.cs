using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ContactsIntegration.ContactsService;

namespace ContactsIntegration
{
    [ServiceContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public interface IContactsService
    {
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        ContactDetails GetContactDetails([MessageParameter(Name = "ContactID")]
                                         long contactID);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        ContactsCollection GetChangedContacts(
                             [MessageParameter(Name = "SinceDateTime")] 
                             DateTime sinceDateTime);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        BulkLoadContactsOutcome BulkLoadContacts(
                             [MessageParameter(Name = "ContactDetailsCollection")] 
                             List<ContactDetails> contactDetailsCollection);

    }

    


    //This is a common FaultContract used across all EDRMS, Financials and Contacts interfaces
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/")]
    public class IRISServiceFaultContract
    {
        [DataMember]
        public int FaultCode;

        [DataMember]
        public string FaultMessage;
    }

    [ServiceContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public interface IContactsIntegrationService
    {
        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        CreateContactOutcome CreateContact([MessageParameter(Name = "ContactDetails")] ContactDetails contactDetails);

        [OperationContract]
        [FaultContract(typeof(IRISServiceFaultContract))]
        UpdateContactOutcome UpdateContact([MessageParameter(Name = "ContactDetails")] ContactDetails contactDetails,
            [MessageParameter(Name = "ContactOtherIdentifiers")] OtherIdentifiers contactOtherIdentifiers,
            [MessageParameter(Name = "ContactCDFs")] CDFs contactCDFs);
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactDetails
    {
        [DataMember]
        public long ContactID { get; set; }

        [DataMember]
        public string ContactType { get; set; }

        [DataMember]
        public ContactPersonDetails ContactPersonDetails { get; set; }

        [DataMember]
        public ContactOrganisationDetails ContactOrganisationDetails
        { get; set; }

        [DataMember]
        public ContactNames ContactNames { get; set; }

        [DataMember]
        public ContactAddresses ContactAddresses { get; set; }

        [DataMember]
        public ContactPhoneNumbers ContactPhoneNumbers { get; set; }

        [DataMember]
        public ContactEmails ContactEmails { get; set; }

        [DataMember]
        public ContactWebsites ContactWebsites { get; set; }

        [DataMember]
        public string FINCustomerCode { get; set; }

        [DataMember]
        public string HistoricID { get; set; }

        [DataMember]
        public bool IsDuplicate { get; set; }

        [DataMember]
        public bool Review { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactPersonDetails
    {

        [DataMember]
        public string PersonGender { get; set; }

        [DataMember]
        public bool PersonIsDeceased { get; set; }

        [DataMember]
        public DateTime? PersonDateOfBirth { get; set; }

        [DataMember]
        public string WarningComments { get; set; }

        [DataMember]
        public string ConfidentialReason { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactOrganisationDetails
    {

        [DataMember]
        public string OrganisationType { get; set; }

        [DataMember]
        public long? OrganisationCompanyNumber { get; set; }

        [DataMember]
        public string OrganisationStatus { get; set; }

    }

    [CollectionDataContract(Name = "ContactNames", ItemName = "ContactName",
Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactNames : List<ContactName> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactName
    {
        [DataMember]
        public long ContactNameID { get; set; }

        [DataMember]
        public string NameType { get; set; }

        [DataMember]
        public bool IsPreferred { get; set; }

        [DataMember]
        public bool IsBilling { get; set; }

        [DataMember]
        public ContactPersonName ContactPersonName { get; set; }

        [DataMember]
        public ContactOrganisationName ContactOrganisationName { get; set; }

        [DataMember]
        public ContactJFCName ContactJFCName { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactPersonName
    {
        [DataMember]
        public string PersonTitle { get; set; }

        [DataMember]
        public string PersonFirstName { get; set; }

        [DataMember]
        public string PersonMiddleNames { get; set; }

        [DataMember]
        public string PersonLastName { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactOrganisationName
    {
            [DataMember]
            public string OrganisationName { get; set; }
 
            [DataMember]
            public string OrganisationDivisionName { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactJFCName
    {
            [DataMember]
            public string JFCName { get; set; }
    }

    [CollectionDataContract(Name = "ContactAddresses", ItemName = "ContactAddress", Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactAddresses : List<ContactAddress> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactAddress
    {
        [DataMember]
        public long ContactAddressID { get; set; }

        [DataMember]
        public string ContactAddressType { get; set; }

        [DataMember]
        public bool IsCurrent { get; set; }

        [DataMember]
        public bool IsPostal { get; set; }

        [DataMember]
        public bool Invalid { get; set; }

        [DataMember]
        public bool IsBilling { get; set; }

        [DataMember]
        public bool IsCareOf { get; set; }

        [DataMember]
        public string Prologue { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public ContactUrbanRuralAddress ContactUrbanRuralAddress { get; set; }

        [DataMember]
        public ContactDeliveryAddress ContactDeliveryAddress { get; set; }

        [DataMember]
        public ContactOverseasAddress ContactOverseasAddress { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactDeliveryAddress
    {
        [DataMember]
        public string DeliveryAddressType { get; set; }

        [DataMember]
        public string DeliveryServiceIdentifier { get; set; }

        [DataMember]
        public string BoxLobby { get; set; }

        [DataMember]
        public string TownCity { get; set; }

        [DataMember]
        public string PostCode { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactOverseasAddress
    {
        [DataMember]
        public string AddressLine1 { get; set; }

        [DataMember]
        public string AddressLine2 { get; set; }

        [DataMember]
        public string AddressLine3 { get; set; }

        [DataMember]
        public string AddressLine4 { get; set; }

        [DataMember]
        public string AddressLine5 { get; set; }

        [DataMember]
        public string Country { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactUrbanRuralAddress
    {

        [DataMember]
        public string UnitType { get; set; }

        [DataMember]
        public string UnitIdentifier { get; set; }

        [DataMember]
        public string FloorType { get; set; }

        [DataMember]
        public string FloorIdentifier { get; set; }

        [DataMember]
        public string BuildingName { get; set; }

        [DataMember]
        public int? StreetNumber { get; set; }

        [DataMember]
        public string StreetAlpha { get; set; }

        [DataMember]
        public string StreetName { get; set; }

        [DataMember]
        public string StreetType { get; set; }

        [DataMember]
        public string StreetDirection { get; set; }

        [DataMember]
        public string RuralDeliveryIdentifier { get; set; }

        [DataMember]
        public string Suburb { get; set; }

        [DataMember]
        public string TownCity { get; set; }

        [DataMember]
        public string PostCode { get; set; }
    }

    [CollectionDataContract(Name = "ContactPhoneNumbers",
    ItemName = "ContactPhoneNumber",
    Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactPhoneNumbers : List<ContactPhoneNumber> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactPhoneNumber
    {
        [DataMember]
        public long ContactPhoneNumberID { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool IsPreferred { get; set; }

        [DataMember]
        public string CountryCode { get; set; }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public string Extension { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }


    [CollectionDataContract(Name = "ContactEmails", ItemName = "ContactEmail", 
    Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactEmails : List<ContactEmail> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactEmail
    {
        [DataMember]
        public long ContactEmailID { get; set; }
 
        [DataMember]
        public string Type { get; set; }
 
        [DataMember]
        public bool IsPreferred { get; set; }
 
        [DataMember]
        public bool IsBilling { get; set; }
 
        [DataMember]
        public bool IsCurrent { get; set; }
        
 
        [DataMember]
        public bool Invalid { get; set; }
 
        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Comment { get; set; }

    }

    [CollectionDataContract(Name = "ContactWebsites", ItemName = "ContactWebsite", Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactWebsites : List<ContactWebsite> { }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactWebsite
    {
        [DataMember]
        public long ContactWebsiteID { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool IsPreferred { get; set; }

        [DataMember]
        public string URL { get; set; }
    }

    [CollectionDataContract(Name = "ContactsCollection",
    ItemName = "ChangedContact",
    Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ContactsCollection : List<ChangedContact> { }


    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class ChangedContact
    {
        [DataMember]
        public ContactDetails ContactDetails { get; set; }

        [DataMember]
        public bool IsNew { get; set; }
    }








    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class CreateContactOutcome 
    {
        [DataMember]
        public bool Success;

        [DataMember]
        public string ErrorMessage;

        [DataMember]
        public string FINCustomerCode;

        [DataMember]
        public OtherIdentifiers OtherIdentifiers;
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class UpdateContactOutcome
    {
        [DataMember]
        public bool Success;

        [DataMember]
        public string ErrorMessage;
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/")]
    public class OtherIdentifier
    {
        [DataMember]
        public string Context { get; set; }
        [DataMember]
        public string Value { get; set; }
    }


    [CollectionDataContract(Name = "OtherIdentifiers", ItemName = "OtherIdentifier", Namespace = "http://www.datacom.co.nz/IRIS/Common/")]
    public class OtherIdentifiers : List<OtherIdentifier>
    {
    }


    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class CDFs
    {
        [DataMember]
        public FormLists FormLists { get; set; }

        [DataMember]
        public TableLists TableLists { get; set; }
    }


    [CollectionDataContract(Name = "FormLists", ItemName = "FormList", Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class FormLists : List<FormList>
    {
    }

    [CollectionDataContract(Name = "TableLists", ItemName = "TableList", Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class TableLists : List<TableList>
    {
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    [KnownType(typeof(FormList))]
    [KnownType(typeof(TableList))]
    public abstract class CDFListBase
    {
        [DataMember]
        public string ListName { get; set; }

        [DataMember]
        public string ObjectType { get; set; }

        [DataMember]
        public string Subclassification1 { get; set; }

        [DataMember]
        public string Subclassification2 { get; set; }

        [DataMember]
        public string Subclassification3 { get; set; }

    }



    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class FormList : CDFListBase
    {
        [DataMember]
        public FieldList FieldList { get; set; }

    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class TableList : CDFListBase
    {
        [DataMember]
        public FieldLists FieldLists { get; set; }
    }


    [CollectionDataContract(Name = "FieldLists", ItemName = "FieldList", Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class FieldLists : List<FieldList>
    {
    }

    [CollectionDataContract(Name = "FieldList", ItemName = "Field", Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class FieldList : List<Field>
    {
    }


    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/CDF/")]
    public class Field
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Contacts/")]
    public class BulkLoadContactsOutcome
    {
        [DataMember]
        public string Success { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public List<ContactInError> contactsInErrorCollection { get; set; }
    }
    [DataContract(Namespace = "http://www.datacom.co.nz/IRIS/Common/")]
    public class ContactInError
    {
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public ContactDetails ContactDetails { get; set; }
    }


}
