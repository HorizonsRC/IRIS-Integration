using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.DL.ContactInfo
{
    public class ContactInfoStatus
    {
        [JsonProperty("ContactKey")]
        public string ContactKey { get; set; }

        [JsonProperty("ContactFullName")]
        public string ContactFullName { get; set; }

        [JsonProperty("Number")]
        public string Number { get; set; }

        [JsonProperty("ContactID")]
        public int ContactID { get; set; }

        [JsonProperty("ContactCustomInputValues")]
        public List<ContactCustomInputValue> ContactCustomInputValues { get; set; }

        [JsonProperty("ContactAddresses")]
        public List<ContactAddress> ContactAddresses { get; set; }

        [JsonProperty("ContactPhoneNumbers")]
        public List<ContactPhoneNumber> ContactPhoneNumbers { get; set; }

        [JsonProperty("ContactEmailAddresses")]
        public List<ContactEmailAddress> ContactEmailAddresses { get; set; }
    }

    

    public class ContactAddress
    {
        [JsonProperty("ContactAddressID")]
        public int ContactAddressID { get; set; }

        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("AddressKey")]
        public string AddressKey { get; set; }

        [JsonProperty("CareOf")]
        public string CareOf { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("IsDefaultAddress")]
        public bool IsDefaultAddress { get; set; }

        [JsonProperty("InlineFormattedAddress")]
        public string InlineFormattedAddress { get; set; }

        [JsonProperty("LinkedTo")]
        public string LinkedTo { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Suburb")]
        public string Suburb { get; set; }

        [JsonProperty("TownCity")]
        public string TownCity { get; set; }

        [JsonProperty("AddressType")]
        public string AddressType { get; set; }

        [JsonProperty("AddressCustomInputValues")]
        public List<AddressCustomInputValue> AddressCustomInputValues { get; set; }
    }

    public class ContactPhoneNumber
    {
        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("IsDefaultPhoneNumber")]
        public bool IsDefaultPhoneNumber { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("PhoneNumberType")]
        public string PhoneNumberType { get; set; }

        [JsonProperty("ContactPhoneNumberID")]
        public int ContactPhoneNumberID { get; set; }

        [JsonProperty("PhoneCustomInputValues")]
        public List<PhoneCustomInputValue> PhoneCustomInputValues { get; set; }
    }
    public class ContactEmailAddress
    {
        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("IsDefaultEmailAddress")]
        public bool IsDefaultEmailAddress { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("LinkedTo")]
        public string LinkedTo { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("ContactEmailAddressID")]
        public int ContactEmailAddressID { get; set; }

        [JsonProperty("EmailCustomInputValues")]
        public List<EmailCustomInputValue> EmailCustomInputValues { get; set; }
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------
    public class ContactCustomInputValue : CustomInputValue
    {
        [JsonProperty("IRISContactID")]
        public string IRISContactID { get; set; }
    }

    public class AddressCustomInputValue : CustomInputValue
    {
        [JsonProperty("IRISContactAddressID")]
        public string IRISContactAddressID { get; set; }
    }

    public class EmailCustomInputValue : CustomInputValue
    {
        [JsonProperty("IRISContactEmailID")]
        public string IRISContactEmailID { get; set; }
    }

    public class PhoneCustomInputValue : CustomInputValue
    {
        [JsonProperty("IRISContactPhoneNumberID")]
        public string IRISContactPhoneNumberID { get; set; }
    }

    public class CustomInputValue
    {
        [JsonProperty("CustomInputID")]
        public int CustomInputID { get; set; }

        [JsonProperty("EntityType")]
        public string EntityType { get; set; }

        [JsonProperty("OpenDate")]
        public string OpenDate { get; set; }

        [JsonProperty("CloseDate")]
        public string CloseDate { get; set; }

        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("CustomInputValueID")]
        public int CustomInputValueID { get; set; }
    }
}

