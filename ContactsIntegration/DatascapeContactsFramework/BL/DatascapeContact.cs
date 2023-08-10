using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using HRC.Common.Configuration;
using HRC.Contacts.BL;
using HRC.DatascapeContacts.DL;
using ContactInfo = HRC.DatascapeContacts.DL.ContactInfo;
using HRC.Framework.BL;
using Newtonsoft.Json;
using RestSharp;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeContact : Contact
    {
        public DatascapePerson Person { get; set; }
        public DatascapeOrganisation Organisation { get; set; }
        public List<DatascapeAddress> Addresses { get; set; }   
        public List<DatascapeCommunication> Communications { get; set; }

        public DatascapeContact() : base()
        {
        }
        public DatascapeContact(ContactType contactType) : base(contactType)
        {
        }

        public string WebsiteURL { get; set; }

        public string EmailAddress
        {
            get
            {
                if (this.Communications != null)
                {
                    DatascapeCommunication com = this.Communications.Find(delegate (DatascapeCommunication c)
                    {
                        return c.IsEmail;
                    });
                    return com == null ? null : com.Base.Email;
                }
                return null;
            }
        }

        public List<string> EmailAddresses
        {
            get
            {
                List<string> emails = new List<string>();
                if (this.Communications != null)
                {
                    foreach (DatascapeCommunication com in this.Communications)
                    {
                        if (com.IsEmail)
                        {
                            emails.Add(com.Base.Email);
                        }
                    }
                }
                return emails;
            }
        }

        public DatascapeAddress PreferredAddress
        {
            get
            {
                if (this.Addresses != null)
                {
                    return this.Addresses.Find(delegate (DatascapeAddress a)
                    {
                        return a.Base.IsCurrent;
                    });
                }
                return null;
            }
        }

        public DatascapeAddress BillingAddress
        {
            get
            {
                if (this.Addresses != null)
                {
                    return this.Addresses.Find(delegate (DatascapeAddress a)
                    {
                        // Billing address has to also be current
                        return a.Base.IsBilling && a.Base.IsCurrent;
                    });
                }
                return null;
            }
        }
        public DatascapeCommunication BillingEmail
        {
            get
            {
                if (this.Communications != null)
                {
                    return this.Communications.Find(delegate (DatascapeCommunication c)
                    {
                        // Billing address has to also be current
                        return c.EmailIsBilling && c.Base.IsCurrent && c.IsEmail;
                    });
                }
                return null;
            }
        }
        public DatascapeAddress PostalAddress
        {
            get
            {
                if (this.Addresses != null)
                {
                    return this.Addresses.Find(delegate (DatascapeAddress a)
                    {
                        // #BP 18/03/2016   Postal address just has to be IsPostal because user can end-date an IsPostal address
                        return a.Base.IsPostal; // && a.Base.IsCurrent;
                    });
                }
                return null;
            }
        }

        public string SaveToDatascape(ref string DatascapeContactID, ref string DatascapeErrorMessage)
        {
            string saveResultStatus = "";

            try
            {
                var JsonPayload = GenerateDatascapeContactJSON(DatascapeContactID);
                var request = PrepareCreateUpdateContactsRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISContactSubmission/SendMessage", JsonPayload);

                Logging.Write("SaveToDatascape", "JSON Payload", string.Empty, JsonConvert.SerializeObject(JsonPayload), IrisContactId);
                RestResponse response = GetResponse(request).Result;

                DatascapePostResult responseContent = ProcessPostResult(response.Content);


                // Check ResponseStatus
                switch (response.ResponseStatus)
                {
                    case ResponseStatus.Completed:
                        // Request was successfully executed, handle the response accordingly
                        break;
                    case ResponseStatus.Error:
                        // An error occurred during request execution, handle the error
                        saveResultStatus = "FAILED";
                        DatascapeErrorMessage = responseContent.Message ?? "";
                        throw response.ErrorException;
                    case ResponseStatus.TimedOut:
                        // Request timed out, handle the timeout scenario
                        saveResultStatus = "FAILED";
                        DatascapeErrorMessage = responseContent.Message ?? "";
                        throw new Exception($"{response.ErrorException.Message} {responseContent.Message ?? ""}");
                    default:
                        // Handle unknown or unsupported response status
                        saveResultStatus = "FAILED";
                        DatascapeErrorMessage = responseContent.Message ?? "";
                        throw new Exception($"{response.ErrorException.Message} {responseContent.Message ?? ""}");
                }

                // Check StatusCode
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        // Request was successful, handle the 200 OK scenario
                        DatascapeContactID = responseContent.ContactKey;
                        saveResultStatus = "SUCCESS";
                        break;
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case (HttpStatusCode)481: // Security Error
                    case (HttpStatusCode)482: // Validation Error
                    case (HttpStatusCode)483: // Execution Error
                                              // Handle specific status codes with common error handling logic
                        saveResultStatus = "FAILED";
                        DatascapeErrorMessage = responseContent.Message ?? "";
                        throw new Exception($"{response.StatusDescription} {responseContent.Message ?? ""}");
                    default:
                        // Handle other status codes
                        saveResultStatus = "FAILED";
                        DatascapeErrorMessage = responseContent.Message ?? "";
                        throw new Exception($"{response.StatusDescription} {responseContent.Message ?? ""}");
                }

                

            }
            catch (Exception e)
            {
                Logging.Write("SaveToDatascape", "Error Saving to Datascape", string.Empty, e.Message, IrisContactId);
                new Common.Email()
                    .SetFrom(new MailAddress(CommonConfig.Instance.MailFromEmail))
                    .AddTo(ConfigurationManager.AppSettings["ExceptionEmailTo"])
                    .SetSubject($"Datascape Contact Integration Error - IRIS ID: {IrisContactId} - {ConfigurationManager.AppSettings["DatascapeEnvironment"]}")
                    .SetBody(e.Message + e.StackTrace)
                    .Send();
            }

            return saveResultStatus;


        }

        public void ManageAccountModifications(ref string DatascapeContactID)
        {
            List<DatascapeAccount> Accounts = GetContactAccounts(DatascapeContactID).Result;
            if (!Accounts.Any()) return;
            foreach(DatascapeAddress Address in Addresses)
            {
                List<DatascapeAccount> SubLinkedAccounts = Accounts.Where(A => A.Addresses.Where(a => a.IRISContactAddressID == Address.Base.SourceId.ToString()).Any()).ToList();
                if (!SubLinkedAccounts.Any()) continue;
                //TODO: Capture update to contact address linked to account
                (DateTime Date, String User) ModifiedUser = DatascapeContactManager.ModifiedUser("ContactAddress", Address.Base.SourceId);
                if (ModifiedUser.Date < DateTime.Now.AddSeconds(-10)) continue;
                foreach(DatascapeAccount account in SubLinkedAccounts)
                {
                    DatascapeContactManager.LogAccountChange(long.Parse(DatascapeContactID), "ContactAddress", Address.Base.SourceId, account.AccountID, account.AccountNumber, account.AccountType, ModifiedUser.User);
                }
            }
            foreach(DatascapeCommunication Communication in Communications)
            {
                List<DatascapeAccount> SubLinkedAccountsCommunications = Accounts
                    .Where(C =>
                        (Communication.IsEmail && C.EmailAddresses.Any(e => e != null && e.IRISContactEmailID == Communication.Base.SourceId.ToString())) ||
                        (Communication.IsPhone && C.PhoneNumbers.Any(p => p != null && p.IRISContactPhoneID == Communication.Base.SourceId.ToString()))
                    )
                    .ToList();
                //TODO: Capture updates to contact emails and phone numbers linked to an account
                (DateTime Date, String User) ModifiedUser = DatascapeContactManager.ModifiedUser(Communication.IsEmail ? "Email" : "PhoneNumber", Communication.Base.SourceId);
                if (ModifiedUser.Date < DateTime.Now.AddSeconds(-10)) continue;
                foreach (DatascapeAccount account in SubLinkedAccountsCommunications)
                {
                    DatascapeContactManager.LogAccountChange(long.Parse(DatascapeContactID), Communication.IsEmail ? "Email" : "PhoneNumber", Communication.Base.SourceId, account.AccountID, account.AccountNumber, account.AccountType, ModifiedUser.User);
                }
            }
        }

        public void ManageContactInfoStatus(ref string DatascapeContactID)
        {
            try
            {
                ContactInfo.ContactInfoStatus Info = GetContactInfoStatus(DatascapeContactID).Result;


                List<ContactInfo.ContactAddress> ClosedAddresses = Info.ContactAddresses.Where(a => !a.ActiveOrFuture).ToList();
                List<DatascapeAddress> MatchingAddresses = Addresses
                    .Where(DSaddress => ClosedAddresses
                    .Any(address => address.AddressCustomInputValues.FirstOrDefault()
                    .IRISContactAddressID == DSaddress.Base.SourceId.ToString() && DSaddress.Base.IsCurrent)).ToList();

                foreach (var address in ClosedAddresses)
                {
                    ContactInfo.AddressCustomInputValue customInputValue = address.AddressCustomInputValues.FirstOrDefault();
                    DatascapeAddress matchingAddress = MatchingAddresses.FirstOrDefault(DSaddress =>
                        customInputValue.IRISContactAddressID == DSaddress.Base.SourceId.ToString());

                    if (matchingAddress != null)
                    { 
                        UpdateRecordStatus(customInputValue.CustomInputValueID, AddressID: address.ContactAddressID);
                    }
                }

                List<ContactInfo.ContactEmailAddress> ClosedEmails = Info.ContactEmailAddresses.Where(a => !a.ActiveOrFuture).ToList();
                List<DatascapeCommunication> MatchingEmails = Communications
                    .Where(DSCommunication => ClosedEmails
                    .Any(email => email.EmailCustomInputValues.FirstOrDefault()
                    .IRISContactEmailID == DSCommunication.Base.SourceId.ToString() && DSCommunication.Base.IsCurrent && DSCommunication.IsEmail)).ToList();

                foreach (var email in ClosedEmails)
                {
                    ContactInfo.EmailCustomInputValue customInputValue = email.EmailCustomInputValues.FirstOrDefault();
                    DatascapeCommunication matchingEmail = MatchingEmails.FirstOrDefault(DSCommunication =>
                        customInputValue.IRISContactEmailID == DSCommunication.Base.SourceId.ToString());

                    if (matchingEmail != null)
                    {
                        UpdateRecordStatus(customInputValue.CustomInputValueID, EmailAddressID: email.ContactEmailAddressID);
                    }
                }

                List<ContactInfo.ContactPhoneNumber> ClosedPhoneNumbers = Info.ContactPhoneNumbers.Where(a => !a.ActiveOrFuture).ToList();
                List<DatascapeCommunication> MatchingPhoneNumbers = Communications
                    .Where(DSCommunication => ClosedPhoneNumbers
                    .Any(Phone => Phone.PhoneCustomInputValues.FirstOrDefault()
                    .IRISContactPhoneNumberID == DSCommunication.Base.SourceId.ToString() && DSCommunication.IsPhone)).ToList();

                foreach (var phone in ClosedPhoneNumbers)
                {
                    ContactInfo.PhoneCustomInputValue customInputValue = phone.PhoneCustomInputValues.FirstOrDefault();
                    DatascapeCommunication matchingPhone = MatchingPhoneNumbers.FirstOrDefault(DSCommunication =>
                        customInputValue.IRISContactPhoneNumberID == DSCommunication.Base.SourceId.ToString());

                    if (matchingPhone != null)
                    {
                        UpdateRecordStatus(customInputValue.CustomInputValueID, PhoneNumberID: phone.ContactPhoneNumberID);
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            
        }


        public ContactPayload GenerateDatascapeContactJSON(string DatascapeContactID)
        {
            return DatascapeContactManager.GenerateDatascapeContactJSON(this, DatascapeContactID);
        }

        public RestRequest PrepareCreateUpdateContactsRequest(string endPoint, ContactPayload contacts)
        {
            return DatascapeContactManager.PrepareCreateUpdateContactsRequest(endPoint, contacts);
        }

        public async Task<RestResponse> GetResponse(RestRequest request)
        {
            return await DatascapeContactManager.GetResponse(request);
        }

        public static DatascapePostResult ProcessPostResult(string response)
        {
            return DatascapeContactManager.ProcessPostResult(response);
        }

        public async Task<List<DatascapeAccount>> GetContactAccounts(string ContactNumber)
        {
            return await DatascapeContactManager.GetContactAccounts(ContactNumber);
        }

        public async Task<ContactInfo.ContactInfoStatus> GetContactInfoStatus(string ContactNumber)
        {
            return await DatascapeContactManager.GetContactInfoStatus(ContactNumber);
        }

        public async void UpdateRecordStatus(long CustomInputValueID, long AddressID=0, long EmailAddressID=0, long PhoneNumberID=0)
        {
            DatascapeContactManager.UpdateRecordStatus(CustomInputValueID, AddressID, EmailAddressID, PhoneNumberID);
        }

        public void ManageAddressDuplicates()
        {
            Addresses = Addresses
                .GroupBy(x => new 
                { x.Base.Prologue,
                    Address = x.FormattedAddress(),  
                  x.Base.TownLocality, 
                  x.Base.Suburb, 
                  x.Base.PostalCode })
                .Select(group => group.OrderByDescending(x => x.Base.IsBilling).ThenByDescending(x => x.Base.IsPostal).First())
                .ToList();

        }
    }

    
}
