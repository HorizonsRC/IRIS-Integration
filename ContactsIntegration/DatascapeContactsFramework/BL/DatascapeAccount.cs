using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeAccount
    {
        [JsonProperty("ContactID")]
        public int ContactID { get; set; }

        [JsonProperty("ContactNumber")]
        public string ContactNumber { get; set; }

        [JsonProperty("AccountID")]
        public int AccountID { get; set; }

        [JsonProperty("AccountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("AccountType")]
        public string AccountType { get; set; }

        [JsonProperty("IRISContactID")]
        public string IRISContactID { get; set; }

        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("EmailAddresses")]
        public List<AccountEmailAddress> EmailAddresses { get; set; }

        [JsonProperty("PhoneNumbers")]
        public List<AccountPhoneNumber> PhoneNumbers { get; set; }

        [JsonProperty("Addresses")]
        public List<AccountAddress> Addresses { get; set; }
    }

    public class AccountAddress
    {
        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("IRISContactAddressID")]
        public string IRISContactAddressID { get; set; }
    }

    public class AccountEmailAddress
    {
        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("IRISContactEmailID")]
        public string IRISContactEmailID { get; set; }
    }

    public class AccountPhoneNumber
    {
        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("PhoneNumberType")]
        public string PhoneNumberType { get; set; }

        [JsonProperty("IRISContactPhoneID")]
        public string IRISContactPhoneID { get; set; }
    }
}
