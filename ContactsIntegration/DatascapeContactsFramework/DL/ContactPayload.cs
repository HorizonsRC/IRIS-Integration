using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.DL
{
    public class ContactPayload
    {
        [JsonProperty("IRISContact")]
        public IRISContact IRISContact { get; set; }
    }

    public class IRISContact
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        [JsonProperty("Confidential")]
        public bool Confidential { get; set; }

        [JsonProperty("APIIRISContactId")]
        public string APIIRISContactId { get; set; }

        [JsonProperty("Surname")]
        public string Surname { get; set; }

        [JsonProperty("SurnameSuffix")]
        public string SurnameSuffix { get; set; }

        [JsonProperty("DateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("Gender")]
        public int? Gender { get; set; }

        [JsonProperty("MaritalStatus")]
        public string MaritalStatus { get; set; }

        [JsonProperty("CorrespondenceMethod")]
        public string CorrespondenceMethod { get; set; }

        [JsonProperty("InteractionMethod")]
        public string InteractionMethod { get; set; }

        [JsonProperty("Title")]
        public int? Title { get; set; }

        [JsonProperty("Type")]
        public int Type { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("Number")]
        public string Number { get; set; }

        [JsonProperty("TradingAs")]
        public string TradingAs { get; set; }

        [JsonProperty("WebsiteURL")]
        public string WebsiteURL { get; set; }

        [JsonProperty("OrganizationRegistrationNumber")]
        public string OrganizationRegistrationNumber { get; set; }

        [JsonProperty("GSTNumber")]
        public string GSTNumber { get; set; }

        [JsonProperty("Aliases")]
        public List<Alias> Aliases { get; set; }

        [JsonProperty("EmailAddresses")]
        public List<EmailAddress> EmailAddresses { get; set; }

        [JsonProperty("PhoneNumbers")]
        public List<PhoneNumber> PhoneNumbers { get; set; }

        [JsonProperty("PostalAddresses")]
        public List<PostalAddress> PostalAddresses { get; set; }
    }

    public class Alias
    {
        [JsonProperty("AliasName")]
        public string AliasName { get; set; }

        [JsonProperty("Note")]
        public string Note { get; set; }

        [JsonProperty("APIIRISContactAliasID")]
        public string APIIRISContactAliasID { get; set; }
    }

    public class EmailAddress
    {
        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("Default")]
        public bool Default { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("APIIRISContactEmailAddressID")]
        public string APIIRISContactEmailAddressID { get; set; }
    }

    public class PhoneNumber
    {
        [JsonProperty("Number")]
        public string Number { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Default")]
        public bool Default { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("APIIRISContactPhoneNumberID")]
        public string APIIRISContactPhoneNumberID { get; set; }
    }

    public class PostalAddress
    {
        [JsonProperty("StreetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty("Locality")]
        public string Locality { get; set; }

        [JsonProperty("Suburb")]
        public string Suburb { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("CareOf")]
        public string CareOf { get; set; }

        [JsonProperty("DPID")]
        public string DPID { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Default")]
        public bool Default { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("APIIRISContactAddressID")]
        public string APIIRISContactAddressID { get; set; }
    }
}
