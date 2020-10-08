using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ContactsIntegration.ContactsService;
using HRC.PowerBuilderContacts.BL;
using HRC.Common;
using HRC.Framework.BL;
using HRC.Common.Validators;
using HRC.Contacts.BL;
using HRC.OzoneContacts.BL;
using System.Text.RegularExpressions;

namespace ContactsIntegration.BL
{
    public class Map
    {
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static OzoneContact IRISContactToOzoneContact(ContactDetails contactDetails)
        {
            ContactType contactType = contactDetails.ContactType.ToUpper().Equals("PERSON") ? ContactType.Person : ContactType.Organization;
            OzoneContact contact = new OzoneContact(contactType);
            contact.IrisContactId = contactDetails.ContactID;

            if (contact.ContactType == ContactType.Person)
            {
                contact.Person = new OzonePerson();
                bool active = false;
                contact.Person.Base = MapCommonPerson(contactDetails, ref active);
                contact.Person.DateOfBirth = contactDetails.ContactPersonDetails.PersonDateOfBirth;
                contact.Person.ConfidentialReason = contactDetails.ContactPersonDetails.ConfidentialReason;
                contact.Person.Base.Id = OzoneContact.GetMigrationId(contactDetails.ContactID);
            }
            else
            {
                contact.Organization = new OzoneOrganization();
                bool active = false;
                contact.Organization.Base = MapCommonOrganization(contactDetails, ref active);
            }

            contact.Addresses = new List<OzoneAddress>();
            foreach (ContactAddress contactAddress in contactDetails.ContactAddresses)
            {
                OzoneAddress address = new OzoneAddress();
                address.Base = MapCommonAddress(contactAddress, "Ozone");
       
                switch (address.Base.AddressTypeEnum)
                {
                    case AddressType.Street: address.Base.AddressType = "S"; break;
                    case AddressType.Rural: address.Base.AddressType = "R"; break;
                    case AddressType.POBox: address.Base.AddressType = "PO"; break;
                    case AddressType.PrivateBag: address.Base.AddressType = "PR"; break;
                    case AddressType.OtherDelivery: address.Base.AddressType = "TE"; break;
                    case AddressType.Overseas: address.Base.AddressType = "OV"; break;
                }
               
                contact.Addresses.Add(address);
            }

            contact.Communications = new List<OzoneCommunication>();
            
            foreach (ContactPhoneNumber phone in contactDetails.ContactPhoneNumbers)
            {
                OzoneCommunication communication = new OzoneCommunication();
                communication.Base = MapCommonPhone(phone);
                communication.IsPhone = true;
                
                switch (phone.Type.ToLower())
                { //Mapping from IRIS PhoneType to Ozone PhoneType
                    //NOTE: If no script is added to a case then execution falls through to the next script if a match is found
                    //      For example; all of 'accounts', 'business', 'customer services', 'dispatch' and 'main office' map to 'D'
                    case "accounts":
                    case "business":
                    case "customer services":
                    case "dispatch":
                    case "main office":
                        communication.PhoneType = 'D';
                        break;
                    case "after hours":
                    case "emergency":
                    case "home":
                        communication.PhoneType = 'A';
                        break;
                    case "fax":
                        communication.PhoneType = 'F';
                        break;
                    case "horizons mobile":
                    case "mobile (business)":
                    case "mobile (personal)":
                        communication.PhoneType = 'C';
                        break;
                    default:
                        communication.PhoneType = 'I';
                        break;
                }
                              
                if (phone.IsPreferred && communication.PhoneType != 'I')
                {
                    contact.Communications.Add(communication);
                }
            }

            foreach (ContactEmail email in contactDetails.ContactEmails)
            {
                OzoneCommunication communication = new OzoneCommunication();
                communication.Base = MapCommonEmail(email);
                communication.IsEmail = true;
                contact.Communications.Add(communication);
            }

            foreach (ContactWebsite website in contactDetails.ContactWebsites)
            {
                OzoneCommunication communication = new OzoneCommunication();
                communication.Base = MapCommonWebsite(website);
                communication.IsWebsite = true;
                contact.Communications.Add(communication);
            }

            return contact;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static PBContact IRISContactToPowerBuilderContact(ContactDetails contactDetails)
        {
            ContactType contactType = contactDetails.ContactType.ToUpper().Equals("PERSON") ? ContactType.Person : ContactType.Organization;
            PBContact contact = new PBContact(contactType);
            contact.IrisContactId = contactDetails.ContactID;

            if (contact.ContactType == ContactType.Person)
            {
                contact.Person = new PBPerson();
                bool active = false;
                contact.Person.Base = MapCommonPerson(contactDetails, ref active);
                contact.Person.Active = active;
                contact.Person.Review = contactDetails.Review;
            }
            else
            {
                contact.Organization = new PBOrganization();
                bool active = false;
                contact.Organization.Base = MapCommonOrganization(contactDetails, ref active);
                contact.Organization.Active = active;
                contact.Organization.Review = false; //default to false
            }

            contact.Addresses = new List<PBAddress>();
            foreach (ContactAddress contactAddress in contactDetails.ContactAddresses)
            {
                PBAddress address = new PBAddress();
                address.Base = MapCommonAddress(contactAddress);

                switch (address.Base.AddressTypeEnum)
                {
                    case AddressType.Street: address.AddressFormat = 'S'; break;
                    case AddressType.Rural: address.AddressFormat = 'R'; break;
                    case AddressType.POBox: address.AddressFormat = 'P'; break;
                    case AddressType.PrivateBag: address.AddressFormat = 'B'; break;
                    case AddressType.OtherDelivery: address.AddressFormat = 'O'; break;
                    case AddressType.Overseas: address.AddressFormat = 'I'; break;
                }
                
                //#BP 03/11/2015 Set the initial value for the AddressPrologue property (otherwise we lose the prologue from IRIS)
               if (address.Base.Prologue != null)
               { address.AddressPrologue = address.Base.Prologue; }
               else
               { address.AddressPrologue = ""; }

                if (address.Base.AddressTypeEnum == AddressType.Rural)
                {
                    string streetAddress = string.Format("{0}{1} {2} {3}",
                        contactAddress.ContactUrbanRuralAddress.StreetNumber,
                        contactAddress.ContactUrbanRuralAddress.StreetAlpha,
                        contactAddress.ContactUrbanRuralAddress.StreetName.SafeTrimOrEmpty(),
                        contactAddress.ContactUrbanRuralAddress.StreetDirection.SafeTrimOrEmpty()).SafeTrimOrEmpty();
                    //#BP 03/11/2015 In case the AddressPrologue has an initial value, append to it
                    address.AddressPrologue = address.AddressPrologue + (address.AddressPrologue.Length > 0 ? " " : "") + streetAddress;
                }

                if (address.Base.AddressTypeEnum == AddressType.Overseas)
                { //create addressPrologue;
                    string addressPrologue = address.Base.AddressLine1;
                    addressPrologue += (addressPrologue.Length > 0 ? "\r\n" : "") + address.Base.AddressLine2;
                    addressPrologue += (addressPrologue.Length > 0 ? "\r\n" : "") + address.Base.AddressLine3;
                    addressPrologue += (addressPrologue.Length > 0 ? "\r\n" : "") + address.Base.AddressLine4;
                    addressPrologue += (addressPrologue.Length > 0 ? "\r\n" : "") + address.Base.AddressLine5;
                    //#BP 03/11/2015 In case the AddressPrologue has an initial value, append to it
                    address.AddressPrologue = address.AddressPrologue + (address.AddressPrologue.Length > 0 ? " " : "") + addressPrologue;
                }
               
                contact.Addresses.Add(address);
            }

            contact.Communications = new List<PBCommunication>();
            foreach (ContactPhoneNumber phone in contactDetails.ContactPhoneNumbers)
            {
                PBCommunication communication = new PBCommunication();
                communication.Base = MapCommonPhone(phone);

                string PhoneType = "";
                switch (phone.Type.ToLower())
                { //PowerBuilder PhoneType Mapping
                    case "accounts":
                    case "business":
                    case "dispatch":
                    case "customer services":
                    case "main office":
                        PhoneType = "Work Phone";
                        break;
                    case "after hours":
                    case "emergency":
                        PhoneType = "After Hours";
                        break;
                    case "fax":
                        PhoneType = "Fax Number";
                        break;
                    case "home":
                        PhoneType = "Home Phone";
                        break;
                    case "horizons mobile":
                    case "mobile (business)":
                        PhoneType = "Mobile Phone";
                        break;
                    case "mobile (personal)":
                        PhoneType = "Mobile Phone (private)";
                        break;
                    default:
                        PhoneType = "IGNORE";
                        break;
                }
                if (PhoneType != "IGNORE")
                {
                    communication.Base.CommunicationType = PhoneType;
                    string phoneNumber = string.Format("{0} {1}{2}", communication.Base.CountryCode, communication.Base.AreaCode, communication.Base.Number).SafeTrimOrEmpty();
                    communication.Content = phoneNumber;
                    if (phone.IsPreferred) 
                    {
                        communication.Base.PrimaryFlag = true;
                        contact.Communications.Add(communication); 
                    }
                }
            }

            foreach (ContactEmail email in contactDetails.ContactEmails)
            {
                PBCommunication communication = new PBCommunication();
                communication.Base = MapCommonEmail(email);
                communication.Base.CommunicationType = "Email Address";
                communication.Content = communication.Base.Email;
                if (email.IsPreferred) {
                    communication.Base.PrimaryFlag = true;
                    contact.Communications.Add(communication); 
                }
            }

            foreach (ContactWebsite website in contactDetails.ContactWebsites)
            {
                PBCommunication communication = new PBCommunication();
                communication.Base = MapCommonWebsite(website);

                communication.Content = communication.Base.Website;
                if (website.IsPreferred)
                {
                    communication.Base.PrimaryFlag = true;
                    contact.Communications.Add(communication);
                }
            }

            return contact;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Person MapCommonPerson(ContactDetails contactDetails, ref bool active)
        {
            Person person = new Person();
            ContactName contactName=null;

            //attempt to find billing name
            contactName = contactDetails.ContactNames.Where(c => c.IsBilling).FirstOrDefault();
            if (contactName == null) //if no billing name use fullname
            {//find FullName
                contactName = contactDetails.ContactNames.Where(c => c.NameType.ToLower()=="full name").FirstOrDefault();
            }

            if (contactName == null) //if no fullname name use preferred name
            {//find FullName
                contactName = contactDetails.ContactNames.Where(c => c.IsPreferred).FirstOrDefault();
              
            }
            if (contactName != null)
            {
                
                person.FirstName = (contactName.ContactPersonName.PersonFirstName + " " + contactName.ContactPersonName.PersonMiddleNames).Truncate(50);
                person.Surname = contactName.ContactPersonName.PersonLastName.Truncate(50);
                person.Initials = getInitials(contactName.ContactPersonName.PersonFirstName, contactName.ContactPersonName.PersonMiddleNames);
                person.Title = contactName.ContactPersonName.PersonTitle.Truncate(10);
                person.Gender = contactDetails.ContactPersonDetails.PersonGender.Truncate(1);

                person.SourceId = contactDetails.ContactID;

                active = true;
                if (contactDetails.ContactPersonDetails.PersonIsDeceased) active = false;
            }
            else
            {//Original Code
              //  Logging.Write("MapCommonPerson", "OriginalCode", string.Empty);
                foreach (ContactName name in contactDetails.ContactNames)
                {

                    //Logging.Write("MapCommonPerson", "OriginalCode", name.ContactPersonName.PersonFirstName + " " + name.ContactPersonName.PersonMiddleNames);
                    if (name.IsPreferred)
                    {
                        person.FirstName = (name.ContactPersonName.PersonFirstName + " " + name.ContactPersonName.PersonMiddleNames).Truncate(50);
                        person.Surname = name.ContactPersonName.PersonLastName.Truncate(50);
                        person.Initials = getInitials(name.ContactPersonName.PersonFirstName, name.ContactPersonName.PersonMiddleNames);
                        person.Title = name.ContactPersonName.PersonTitle.Truncate(10);
                        person.Gender = contactDetails.ContactPersonDetails.PersonGender.Truncate(1);

                        person.SourceId = contactDetails.ContactID;

                        active = true;
                        if (contactDetails.ContactPersonDetails.PersonIsDeceased) active = false;
                    }
                }
            }
            return person;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static string getInitials(string firstNames, string middleNames)
        {
            Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
            string init = initials.Replace(firstNames + " " + middleNames, "$1");
            return init;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Organization MapCommonOrganization(ContactDetails contactDetails, ref bool active)
        {
            Organization organization = new Organization();
            foreach (ContactName name in contactDetails.ContactNames)
            {
                if (name.IsPreferred)
                {
                    organization.Name = name.ContactOrganisationName.OrganisationName;
                    organization.Division = name.ContactOrganisationName.OrganisationDivisionName;

                    organization.SourceId = contactDetails.ContactID;
                }
            }
            return organization;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Address MapCommonAddress(ContactAddress contactAddress, string syncType = "")
        {
            Address address = new Address();
            List<string> addressLines = new List<string>();
            address.AddressType = contactAddress.ContactAddressType;

            address.SourceId = contactAddress.ContactAddressID;

            address.IsCurrent = contactAddress.IsCurrent;
            address.IsCareOf = contactAddress.IsCareOf;
            address.IsPostal = contactAddress.IsPostal;
            address.IsBilling = contactAddress.IsBilling;
            address.Prologue = contactAddress.Prologue;
            if (contactAddress.ContactUrbanRuralAddress != null)
            {
                address.BuildingPropertyName = contactAddress.ContactUrbanRuralAddress.BuildingName;
                address.StreetType = contactAddress.ContactUrbanRuralAddress.StreetType;
                address.StreetName = contactAddress.ContactUrbanRuralAddress.StreetName;
                address.StreetSuffix = contactAddress.ContactUrbanRuralAddress.StreetDirection;

                
                address.HouseNumber = contactAddress.ContactUrbanRuralAddress.StreetNumber.GetValueOrDefault(0);

                string streetAddress = string.Format("{0}{1} {2} {3}",
                    contactAddress.ContactUrbanRuralAddress.StreetNumber,
                    contactAddress.ContactUrbanRuralAddress.StreetAlpha,
                    contactAddress.ContactUrbanRuralAddress.StreetName.SafeTrimOrEmpty(),
                    contactAddress.ContactUrbanRuralAddress.StreetDirection.SafeTrimOrEmpty()).SafeTrimOrEmpty();

                int ruralId;
                if (int.TryParse(contactAddress.ContactUrbanRuralAddress.RuralDeliveryIdentifier, out ruralId))
                {
                    address.AddressNumberText = ruralId.ToString();
                    address.AddressTypeEnum = AddressType.Rural;                    
                }
                else
                {
                    int? streetNumber = contactAddress.ContactUrbanRuralAddress.StreetNumber;
                    address.AddressNumberText = (streetNumber.HasValue ? streetNumber.Value.ToString() : string.Empty) + contactAddress.ContactUrbanRuralAddress.StreetAlpha;                    
                    address.AddressTypeEnum = AddressType.Street;
                }
                address.StreetAlpha = contactAddress.ContactUrbanRuralAddress.StreetAlpha;

                address.TownLocality = contactAddress.ContactUrbanRuralAddress.TownCity;
                address.Suburb = contactAddress.ContactUrbanRuralAddress.Suburb;
                address.PostalCode = contactAddress.ContactUrbanRuralAddress.PostCode;
                address.FloorType = contactAddress.ContactUrbanRuralAddress.FloorType;
                address.FloorId = contactAddress.ContactUrbanRuralAddress.FloorIdentifier;

                address.UnitId = contactAddress.ContactUrbanRuralAddress.UnitIdentifier;
                address.UnitType = contactAddress.ContactUrbanRuralAddress.UnitType;

                if (!string.IsNullOrEmpty(contactAddress.ContactUrbanRuralAddress.FloorType))
                {
                    //floor 
                    addressLines.Add(contactAddress.ContactUrbanRuralAddress.FloorType);
                }
                else if (!string.IsNullOrEmpty(contactAddress.ContactUrbanRuralAddress.UnitType))
                {
                    //unit
                    addressLines.Add(string.Format("{0} {1}", contactAddress.ContactUrbanRuralAddress.UnitType, contactAddress.ContactUrbanRuralAddress.UnitIdentifier).SafeTrimOrEmpty());
                }

                //building name
                if (!string.IsNullOrEmpty(contactAddress.ContactUrbanRuralAddress.BuildingName))
                {
                    addressLines.Add(contactAddress.ContactUrbanRuralAddress.BuildingName);
                }

                //rural delivery
                //Logging.Write("RD", contactAddress.ContactUrbanRuralAddress.RuralDeliveryIdentifier, "");
                if (!string.IsNullOrEmpty(contactAddress.ContactUrbanRuralAddress.RuralDeliveryIdentifier))
                {
                    addressLines.Add(string.Format("RD{0}", contactAddress.ContactUrbanRuralAddress.RuralDeliveryIdentifier.SafeTrimOrEmpty()));
                }

                //street
                addressLines.Add(streetAddress);

                if (!string.IsNullOrEmpty(contactAddress.ContactUrbanRuralAddress.Suburb))
                {
                    addressLines.Add(contactAddress.ContactUrbanRuralAddress.Suburb);
                }
                addressLines.Add(string.Format("{0} {1}", contactAddress.ContactUrbanRuralAddress.TownCity, contactAddress.ContactUrbanRuralAddress.PostCode).SafeTrimOrEmpty());
            }
            else if (contactAddress.ContactDeliveryAddress != null)
            {
                address.AddressNumberText = contactAddress.ContactDeliveryAddress.DeliveryServiceIdentifier;
                address.Suburb = contactAddress.ContactDeliveryAddress.BoxLobby;

                switch (contactAddress.ContactDeliveryAddress.DeliveryAddressType)
                {
                    case "PO Box": address.AddressTypeEnum = AddressType.POBox; break; 
                    case "Private Bag": address.AddressTypeEnum = AddressType.PrivateBag; break;
                    default: address.AddressTypeEnum = AddressType.OtherDelivery; break; 
                }

                address.PostalCode = contactAddress.ContactDeliveryAddress.PostCode;
                address.TownLocality = contactAddress.ContactDeliveryAddress.TownCity;

                string localAddress = string.Format("{0} {1}", contactAddress.ContactDeliveryAddress.DeliveryAddressType, address.AddressNumberText).SafeTrimOrEmpty();
                if (!string.IsNullOrEmpty(localAddress))
                {
                    addressLines.Add(localAddress);
                }
                localAddress = address.Suburb;
                if (!string.IsNullOrEmpty(localAddress))
                {
                    addressLines.Add(localAddress);
                }
                localAddress = string.Format("{0} {1}", contactAddress.ContactDeliveryAddress.TownCity, contactAddress.ContactDeliveryAddress.PostCode).SafeTrimOrEmpty();
                if (!string.IsNullOrEmpty(localAddress))
                {
                    addressLines.Add(localAddress);
                }
            }
            else if (contactAddress.ContactOverseasAddress != null)
            {
                if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine1))
                {
                    addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine1);
                }
                if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine2))
                {
                    addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine2);
                }
                if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine3))
                {
                    addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine3);
                }
                Logging.Write("SaveToOzone", "Sync Type: " + syncType, string.Empty);
                if (syncType == "Ozone")
                {
                    if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine4))
                    {
                        addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine4);
                    }
                    if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine5))
                    {
                        addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine5);
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine4))
                    {
                        if (string.IsNullOrEmpty(contactAddress.ContactOverseasAddress.AddressLine5))
                        {
                            addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine4);
                        }
                        else
                        {
                            addressLines.Add(contactAddress.ContactOverseasAddress.AddressLine4 + ", " + contactAddress.ContactOverseasAddress.AddressLine5);
                        }
                    }
                }
                
                addressLines.Add(contactAddress.ContactOverseasAddress.Country);

                
                
                address.AddressTypeEnum = AddressType.Overseas;
            }
            else
            {
                //error verbosity to be determined 
            }

            if (addressLines.Count > 0) address.AddressLine1 = addressLines[0];
            if (addressLines.Count > 1) address.AddressLine2 = addressLines[1];
            if (addressLines.Count > 2) address.AddressLine3 = addressLines[2];
            if (addressLines.Count > 3) address.AddressLine4 = addressLines[3];
            if (addressLines.Count > 4) address.AddressLine5 = addressLines[4];
            if (addressLines.Count > 5 && syncType == "Ozone") address.AddressLine6 = addressLines[5];
            
           
            return address;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Communication MapCommonWebsite(ContactWebsite website)
        {
            Communication communication = new Communication();

            communication.SourceId = website.ContactWebsiteID;

            communication.BasicCommunicationType = "website";
            communication.CommunicationType = website.Type;
            communication.Website = website.URL;
            return communication;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Communication MapCommonEmail(ContactEmail email)
        {
            Communication communication = new Communication();

            communication.SourceId = email.ContactEmailID;

            communication.BasicCommunicationType = "email";
            communication.CommunicationType = email.Type;
            communication.Comments = email.Comment;
            communication.Email = email.Value;
            return communication;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Communication MapCommonPhone(ContactPhoneNumber phone)
        {
            Communication communication = new Communication();

            communication.SourceId = phone.ContactPhoneNumberID;

            communication.BasicCommunicationType = "phone";
            //communication.CommunicationType = phone.Type;
            communication.Comments = phone.Comment;
            communication.Extension = phone.Extension.SafeTrimOrEmpty();
            if (string.IsNullOrEmpty(phone.CountryCode)) { phone.CountryCode = "0064"; }
            communication.CountryCode = phone.CountryCode;
            communication.AreaCode = "";                        //Default area code
            communication.Number = phone.Number;                //Default phone number

            //#BP 03/03/2016    Added code to handle formatting of phone numbers for selected countries
            switch (phone.CountryCode)
            { //We're handling some country codes individually and have researched the internal area codes for those countries
                case "001": //America
                    if (("|205|251|256|938|334|907|480|520|602|623|928|479|501|870|209|213|310|424|323|408|669|415|510|530|559|562|619|626|"+
                          "650|661|707|714|657|760|442|805|818|747|831|858|909|916|925|949|951|303|720|719|970|203|475|860|302|202|710|239|"+
                          "305|786|321|352|386|407|321|561|727|772|813|850|863|904|941|954|754|229|404|678|470|478|706|762|770|678|470|912|"+
                          "808|208|217|309|312|618|630|331|708|464|773|815|779|847|224|219|260|317|574|765|812|930|319|515|563|641|712|316|"+
                          "620|785|913|270|364|502|606|869|225|318|337|504|985|207|301|240|410|443|413|508|774|617|857|781|339|978|351|231|"+
                          "248|947|269|313|586|616|717|734|810|906|989|218|320|507|612|651|763|952|228|601|769|662|314|417|573|636|660|816|"+
                          "406|308|402|531|702|725|775|603|201|551|609|732|848|856|908|973|862|505|575|252|336|704|980|828|910|919|984|701|"+
                          "212|917|646|315|516|518|585|607|631|716|718|917|347|845|914|216|330|234|419|567|440|513|614|740|937|405|580|918|"+
                          "539|503|971|541|215|267|412|878|570|272|610|484|724|878|814|401|803|843|864|605|423|615|731|865|901|931|210|214|"+
                          "972|469|254|325|361|409|432|512|713|281|832|346|817|682|830|903|430|915|936|940|956|979|435|801|385|802|276|434|"+
                          "540|703|571|757|804|206|253|360|425|509|304|681|262|414|608|715|920|307|800|822|833|844|855|866|877|880|881|882|"+
                          "883|884|885|886|887|888|889|900|").Contains("|" + phone.Number.SafeSubstring(0, 3) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 3);
                        communication.Number = phone.Number.SafeSubstring(3, phone.Number.Length - 3).Replace(" ", "");
                    }
                    else if (("|1800|").Contains("|" + phone.Number.SafeSubstring(0, 4) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 4);
                        communication.Number = phone.Number.SafeSubstring(4, phone.Number.Length - 4).Replace(" ", "");
                    }
                    break;
                case "0044": //United Kingdom
                    if (("|1224|1244|1382|1387|1429|1482|1539|1582|1670|1697|1730|1736|1772|1793|1854|1947|1204|1208|1254|1276|1297|1298|1363|1364|1384|1386|1404|1420"+
                         "|1460|1461|1480|1488|1524|1527|1562|1566|1606|1629|1635|1647|1659|1695|1726|1744|1750|1768|1827|1837|1884|1900|1905|1935|1946|1949|1963|1995|").Contains("|" + phone.Number.SafeSubstring(0, 4) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 4);
                        communication.Number = phone.Number.SafeSubstring(4, phone.Number.Length - 4).Replace(" ", "");
                    }
                    else if (("|121|131|141|151|161|171|181|191|113|114|115|116|117|118|").Contains("|" + phone.Number.SafeSubstring(0, 3) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 3);
                        communication.Number = phone.Number.SafeSubstring(3, phone.Number.Length - 3).Replace(" ", "");
                    }
                    else if (("|20|23|24|28|29|").Contains("|" + phone.Number.SafeSubstring(0, 2) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 2);
                        communication.Number = phone.Number.SafeSubstring(2, phone.Number.Length - 2).Replace(" ", "");
                    }
                    else if (("|13873|15242|15394|15395|15396|16973|16974|16977|17683|17684|17687|19467|").Contains("|" + phone.Number.SafeSubstring(0, 5) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 5);
                        communication.Number = phone.Number.SafeSubstring(5, phone.Number.Length - 5).Replace(" ", "");
                    }
                    break;
                case "0061": //Australia
                    if (("|2|3|4|7|8|").Contains("|" + phone.Number.SafeSubstring(0, 1) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 1);
                        communication.Number = phone.Number.SafeSubstring(1,phone.Number.Length-1).Replace(" ","");
                    }
                    break;
                case "0064": //New Zealand
                    if (("|02|03|04|06|07|09|").Contains("|" + phone.Number.SafeSubstring(0, 2) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(1, 2);
                        communication.Number = phone.Number.SafeSubstring(2, phone.Number.Length - 2).Replace(" ", "");
                    }
                    else if (("|021|022|023|024|025|026|027|028|029|").Contains("|" + phone.Number.SafeSubstring(0, 3) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(1, 3);
                        communication.Number = phone.Number.SafeSubstring(3, phone.Number.Length - 3).Replace(" ", "");
                    }
                    else if (("|0201|0202|0203|0204|0205|0206|0508|0800|0900|").Contains("|" + phone.Number.SafeSubstring(0, 4) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(1, 4);
                        communication.Number = phone.Number.SafeSubstring(4, phone.Number.Length - 4).Replace(" ", "");
                    }
                    break;
                case "0086": //China
                    if (("|130|131|132|133|135|136|137|138|139|145|147|150|151|152|153|155|156|157|158|159|176|177|178|180|181|182|183|184|185|186|187|188|189|").Contains("|" + phone.Number.SafeSubstring(0, 3) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 3);
                        communication.Number = phone.Number.SafeSubstring(3,phone.Number.Length-3).Replace(" ","");
                    }
                    else if (("|1340|1341|1342|1343|1344|1345|1346|1347|1348|1349|1700|1705|1709|").Contains("|" + phone.Number.SafeSubstring(0, 4) + "|"))
                    {
                        communication.AreaCode = phone.Number.SafeSubstring(0, 4);
                        communication.Number = phone.Number.SafeSubstring(4, phone.Number.Length - 4).Replace(" ", "");
                    }
                    break;
            }

            //string phoneNumber = PhoneValidator.Validate(phone.Number.SafeTrimOrEmpty());
            //bool splitAreaCode = true;

            //if (splitAreaCode)
            //{
            //    int areaCodeLength = communication.CommunicationType.Contains("Mobile", StringComparison.InvariantCultureIgnoreCase) ? 3 : 2;
            //    communication.Number = phoneNumber.SafeSubstring(0, 1).Equals("0") ? phoneNumber.SafeSubstring(areaCodeLength, phoneNumber.Length).SafeTrimOrEmpty() : phoneNumber;
            //    communication.AreaCode = phoneNumber.SafeSubstring(0, 1).Equals("0") ? phoneNumber.SafeSubstring(0, areaCodeLength).SafeTrimOrEmpty() : string.Empty;
            //}
            //else
            //{
            //    communication.Number = phoneNumber;
            //}
            //communication.Extension = phone.Extension.SafeTrimOrEmpty();
            //communication.CountryCode = phone.CountryCode.SafeTrimOrEmpty();

            return communication;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
