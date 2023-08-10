using HRC.DatascapeContacts.BL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRC.Contacts.BL;
using RestSharp;
using System.Net;
using System.Configuration;
using HRC.DatascapeContacts.DL.ContactInfo;
using System.Data.SqlClient;
using HRC.Common.Data;

namespace HRC.DatascapeContacts.DL
{
    internal class DatascapeContactManager
    {
        
        private static RestClient _client = new RestClient(new RestClientOptions(@"https://datascape.cloud/"));

        public static bool ManageContactDetailStatus(DatascapeContact contact)
        {
            bool Success = false;

            return Success;
        }

        public static ContactPayload GenerateDatascapeContactJSON(DatascapeContact contact, string DatascapeContactID)
        {

            ContactPayload Payload = new ContactPayload
            { IRISContact = new IRISContact
            {
                APIIRISContactId = contact.IrisContactId.ToString(),
                FirstName = contact.ContactType == ContactType.Person ? contact.Person.Base.FirstName : "",
                MiddleName = contact.ContactType == ContactType.Person ? contact.Person.Base.MiddleName : "",
                Confidential = contact.ContactType == ContactType.Person ? contact.Person.Confidential : false,
                Surname = contact.ContactType == ContactType.Person ? contact.Person.Base.Surname : $"{contact.Organisation.Base.Name} {contact.Organisation.Base.Division ?? ""}",
                //SurnameSuffix = "", // NOT IN IRIS
                DateOfBirth = contact.ContactType == ContactType.Person ? contact.Person.DateOfBirth?.ToString("dd/MM/yyyy") : "",
                Gender = contact.ContactType == ContactType.Person ? ContactGenderLookupAsync(contact.Person.Base.Gender).Result : null,
                //MaritalStatus = "", //NOT IN IRIS
                CorrespondenceMethod = contact.ContactType == ContactType.Person ? contact.Person.CorrespondenceMethod : contact.Organisation.CorrespondenceMethod,
                InteractionMethod = contact.ContactType == ContactType.Person ? contact.Person.InteractionMethod : contact.Organisation.InteractionMethod,
                Title = contact.ContactType == ContactType.Person ? ContactTitleLookupAsync(contact.Person.Base.Title).Result : null, // Need to map, NOT IN IRIS XML?
                Type = contact.ContactType == ContactType.Person ? 10 : 20,
                OpenDate = DatascapeContactID == "" ? DateTime.Now.ToString("dd/MM/yyyy") : "",
                //TODO: Close Date
                TradingAs = contact.ContactType == ContactType.Organization ? contact.Organisation.TradingAs : "",
                WebsiteURL = contact.WebsiteURL,
                OrganizationRegistrationNumber = contact.ContactType == ContactType.Organization ? contact.Organisation.OrganisationRegistrationNumber : "",
                Number = contact.IrisContactId.ToString(),
                GSTNumber = "",
                Aliases = contact.ContactType == ContactType.Person ? contact.Person.Aliases.Select(a => new Alias
                {
                    AliasName = a.Name,
                    APIIRISContactAliasID = a.SourceID.ToString()
                }).ToList() : null,
                EmailAddresses = contact.Communications.Where(e => e.IsEmail == true && e.Base.IsCurrent).Select(e => new EmailAddress
                {
                    APIIRISContactEmailAddressID = e.Base.SourceId.ToString(),
                    Default = contact.BillingEmail == null ? (e.Base.PrimaryFlag == true ? true : false) : e.EmailIsBilling,
                    Address = e.Base.Email,
                    //TODO: Open Date
                    CloseDate = e.Base.IsCurrent ? "" : DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList(),
                PhoneNumbers = contact.Communications.Where(p => p.IsPhone == true).Select(p => new PhoneNumber
                {
                    APIIRISContactPhoneNumberID = p.Base.SourceId.ToString(),
                    Default = p.Base.PrimaryFlag == true ? true : false,
                    Type = p.Base.CommunicationType,
                    Number = p.CountryCodeIsValid() ? $"{p.Base.CountryCode} {p.Base.Number}" : p.Base.Number,
                    //TODO: Open Date
                    //CloseDate = p.Base.
                }).ToList(),
                PostalAddresses = contact.Addresses.Where(a => a.Base.IsCurrent).Select(a => new PostalAddress
                {
                    StreetAddress = a.FormattedAddress(),
                    Locality = a.Base.TownLocality,
                    Suburb = a.Base.Suburb,
                    PostCode = a.Base.PostalCode,
                    Country = ContactAddressCountryLookupAsync(a.Country).Result.ToString(),
                    CareOf = a.Base.Prologue ?? null,
                    //DPID,
                    //State,
                    Default = contact.BillingAddress == null ? a.Base.IsPostal : a.Base.IsBilling,
                    //TODO: Open Date
                    CloseDate = a.Base.IsCurrent? "" : DateTime.Now.ToString("dd/MM/yyyy"),
                    APIIRISContactAddressID = a.Base.SourceId.ToString()
                }).ToList()
            }
            };

            return Payload;
        }


        public static RestRequest PrepareCreateUpdateContactsRequest(string endPoint, ContactPayload contacts)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"{endPoint}", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            var json = JsonConvert.SerializeObject(contacts);
            request.AddBody(json);
            return request;
        }

        public static async Task<int?> ContactGenderLookupAsync(string Gender)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            if (Gender == null) { return null; }
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISContactGenderLookup", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            List<DatascapeContactGender> DatascapeGenders = await _client.GetAsync<List<DatascapeContactGender>>(request);
            return DatascapeGenders.Where(g => g.Name == Gender & g.ActiveOrFuture == true).Select(g => g.ContactGenderId).FirstOrDefault();
        }

        public static async Task<int?> ContactTitleLookupAsync(string Title)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            if (Title == null) { return null; }
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISContactTitleLookup?ShowAll=true", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            List<DatascapeContactTitle> DatascapeTitles = await _client.GetAsync<List<DatascapeContactTitle>>(request);
            int TitleID = DatascapeTitles.Where(t => t.Name == Title & t.ActiveOrFuture == true).Select(t => t.ContactTitleId).FirstOrDefault();
            if (TitleID == 0) { return null; }
            else { return TitleID; }
        }

        public static async Task<int?> ContactAddressCountryLookupAsync(string Country)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            if (Country == null) { return null; }
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISContactCountryLookup?ShowAll=true", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            List<DatascapeContactAddressCountry> DatascapeCountries = await _client.GetAsync<List<DatascapeContactAddressCountry>>(request);
            return DatascapeCountries.Where(c => c.Name.ToUpper() == Country.ToUpper() & c.ActiveOrFuture == true).Select(c  => c.CountryID).FirstOrDefault();
        }

        public static async Task<List<DatascapeAccount>> GetContactAccounts(string ContactNumber)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISRatesContactLookup?Filter=Base.AccountHolders.Contact.Number eq `{ContactNumber}`", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            List<DatascapeAccount> DatascapeAccounts = await _client.GetAsync<List<DatascapeAccount>>(request);
            return DatascapeAccounts;
        }

        public static async Task<RestResponse> GetResponse(RestRequest request)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            return await _client.ExecuteAsync(request);
        }

        internal static DatascapePostResult ProcessPostResult(string response)
        {
            return JsonConvert.DeserializeObject<DatascapePostResult>(response);
        }

        public static async Task<ContactInfoStatus> GetContactInfoStatus(string ContactNumber)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISContactInfoStatus?Filter=Number eq `{ContactNumber}`", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            List<ContactInfoStatus> contactInfoStatus = await _client.GetAsync<List<ContactInfoStatus>>(request);
            return contactInfoStatus.FirstOrDefault();
        }

        public static async void UpdateRecordStatus(long CustomInputValueID, long? AddressID, long? EmailAddressID, long? PhoneNumberID)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISUpdateContactsRelatedRecordsState/SendMessage/", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            var json = JsonConvert.SerializeObject(new {AddressID = AddressID, CustomInputValueID = CustomInputValueID, EmailAddressID = EmailAddressID, PhoneNumberID = PhoneNumberID });
            request.AddBody(json);
            RestResponse response = _client.Execute(request);
            if (!response.IsSuccessful) throw new Exception($"Failed to update Record Status for Custom Input Value {CustomInputValueID} " + response.ErrorMessage);

        }

        public static (DateTime, string) ModifiedUser(string Table, long ID)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    if(Table == "ContactAddress")
                    {
                        command.CommandText = $@"SELECT CASE WHEN CAD.LastModified > ADR.LastModified THEN CAD.ModifiedBy ELSE ADR.ModifiedBy END ModifiedBy,
                                                        CASE WHEN CAD.LastModified > ADR.LastModified THEN CAD.LastModified ELSE ADR.LastModified END LastModified
                                                FROM ContactAddress			CAD
                                                LEFT JOIN Address ADR
                                                ON CAD.AddressID = ADR.ID WHERE CAD.ID = {ID}";
                    }
                    else command.CommandText = $"SELECT ModifiedBy, LastModified FROM {Table} WHERE ID = {ID}";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ((DateTime)reader["LastModified"],(string)reader["ModifiedBy"]);
                        }
                        else
                        {
                            throw new Exception("Failed to retrieve Last Modified information");
                        }
                    }
                }

            }
        }

        public static void LogAccountChange(long ContactID, string LinkedType, long LinkedID, int AccountID, string AccountNumber, string Type, string User)
        {
            string sql = @"insert into DatascapeAccountChangeLog([ContactID], [LinkedType], [LinkedID], [DatascapeAccountID], [DatascapeAccountNumber], [DatascapeAccountType], [Timestamp], [ModifiedBy])
                values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}) ";
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.Logging))
            {
                SqlResult result = CommandHelper.ExecuteSqlQuery(sql, con,
                    ContactID,
                    LinkedType,
                    LinkedID,
                    AccountID,
                    AccountNumber,
                    Type,
                    DateTime.Now,
                    User
                    );
            }
        }

    }

    internal class DatascapeContactGender
    {
        public bool ActiveOrFuture { get; set; }
        public string CloseDate { get; set; }
        public string Name { get; set; }
        public string OpenDate { get; set; }
        public int ContactGenderId { get; set; }
    }

    public class DatascapeContactTitle
    {
        public bool ActiveOrFuture { get; set; }
        public string CloseDate { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public string OpenDate { get; set; }
        public int ContactTitleId { get; set; }
    }

    public class DatascapeContactAddressCountry
    {
        public bool ActiveOrFuture { get; set; }
        public string AddressFormat { get; set; }
        public string AddressKeyFormat { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CountryID { get; set; }
    }
}
