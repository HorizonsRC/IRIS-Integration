using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
using HRC.Common.Data;
using HRC.Common.Configuration;
using HRC.Contacts.BL;
using HRC.OzoneContacts.BL;
using System.Text.RegularExpressions;
namespace HRC.OzoneContacts.DL
{
    internal class OzoneContactManager
    {
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static string AddTab(string text, int tabs)
        {
            return text.PadLeft(text.Length + (tabs * 4));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static string AddValue(Tuple<string, object> value)
        {
            //special case for dialers
            if (value.Item1.StartsWith("CONTACT.DIALER"))
            {
                if (value.Item1.Replace("CONTACT.DIALER", "") == "")
                {
                    return string.Format(@"<value dict=""{0}"">{1}</value>", "CONTACT.DIALER", value.Item2);
                }
                else
                {
                    return string.Format(@"<value dict=""{0}"" line=""{1}"">{2}</value>", "CONTACT.DIALER", value.Item1.Replace("CONTACT.DIALER", ""), value.Item2);
                }
            }
            if (value.Item1 != "CONTACT.EMAIL.TYPE" && value.Item1.StartsWith("CONTACT.EMAIL"))
            {
                return string.Format(@"<value dict=""{0}"" line=""{1}"">{2}</value><value dict=""CONTACT.EMAIL.TYPE"" line=""{1}"">E</value>", "CONTACT.EMAIL", value.Item1.Replace("CONTACT.EMAIL", ""), value.Item2);
            }

            if (value.Item1 != "CONTACT.EMAIL.TYPE" && value.Item1.StartsWith("CONTACT.WEBSITE"))
            {
                return string.Format(@"<value dict=""{0}"" line=""{1}"">{2}</value><value dict=""CONTACT.EMAIL.TYPE"" line=""{1}"">W</value>", "CONTACT.EMAIL", value.Item1.Replace("CONTACT.WEBSITE", ""), value.Item2);
            }

            if (value.Item1 != "CONTACT.PADDR" && value.Item1.StartsWith("CONTACT.PADDR"))
            {
                return string.Format(@"<value dict=""CONTACT.PADDR"" line=""{1}"">{2}</value>", "CONTACT.PADDR", value.Item1.Replace("CONTACT.PADDR", ""), value.Item2);
            }

            if (value.Item1.StartsWith("CONTACT.VANITY"))
            {
                return string.Format(@"<value dict=""CONTACT.VANITY"" line=""{0}"">{1}</value>", value.Item1.Replace("CONTACT.VANITY", ""), value.Item2);
            }

            return string.Format(@"<value dict=""{0}"">{1}</value>", value.Item1, value.Item2);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddHeader(ref StringBuilder writer, string ozoneUser)
        {
            writer.AppendLine(@"<?xml version=""1.0""?>");
            writer.AppendLine(@"<origenxml version=""2.0"" type=""data"">");
            writer.AppendLine(AddTab(string.Format(@"<data user=""{0}"">", ozoneUser), 1));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddFooter(ref StringBuilder writer)
        {
            writer.AppendLine(AddTab(@"</data>", 1));
            writer.AppendLine(@"</origenxml>");
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddItem(ref StringBuilder writer, string Id, string businessObject, List<Tuple<string, object>> fields)
        {
            writer.AppendLine(AddTab(@"<item>", 2));
            if (Id != "0")
            {
                writer.AppendLine(AddTab(string.Format(@"<id>{0}</id>", Id), 3));
            }
            writer.AppendLine(AddTab(string.Format(@"<filename>{0}</filename>", businessObject), 3));
            writer.AppendLine(AddTab(string.Format(@"<values>", Id), 3));
            foreach (Tuple<string, object> field in fields)
            {
                writer.AppendLine(AddTab(AddValue(field), 4));
            }
            writer.AppendLine(AddTab(string.Format(@"</values>", Id), 3));
            writer.AppendLine(AddTab(@"</item>", 2));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //This generates the XML blob that will be handed off to the Ozone service for processing (create or update the identified contact)
        public static string GeneratePostXml(OzoneContact contact, string ozoneContactId)
        {
            StringBuilder writer = new StringBuilder();
            
            string ozoneUser = ConfigurationManager.AppSettings["IRISOZONEUSER"];
            if (string.IsNullOrEmpty(ozoneUser)) ozoneUser="IRISREPORTUSER";

            AddHeader(ref writer, ozoneUser);

            List<Tuple<string, object>> contactFields = new List<Tuple<string, object>>();
            contactFields.Add(new Tuple<string, object>("CONTACT.LEGACY.ID.1", contact.IrisContactId.ToString()));
            contactFields.Add(new Tuple<string, object>("CONTACT.LEGACY.ID.2", "IRIS"));
            if (contact.ContactType == ContactType.Person)
            {
                contactFields.Add(new Tuple<string, object>("CONTACT.TYPE", "I"));
                contactFields.Add(new Tuple<string, object>("CONTACT.SURNAME", contact.Person.Base.Surname.Trim()));
                contactFields.Add(new Tuple<string, object>("CONTACT.GIVEN", $"{contact.Person.Base.FirstName} {contact.Person.Base.MiddleName}".Trim()));
                contactFields.Add(new Tuple<string, object>("CONTACT.TITLE", contact.Person.Base.Title));
                contactFields.Add(new Tuple<string, object>("CONTACT.ALIAS", contact.Person.Base.KnownBy));
                contactFields.Add(new Tuple<string, object>("CONTACT.INIT", contact.Person.Base.Initials)); //this appears to be computed

                if (contact.Person.DateOfBirth.HasValue)
                {
                    contactFields.Add(new Tuple<string, object>("CONTACT.DOB.DISP", contact.Person.DateOfBirth.Value.ToString("dd/MM/yyyy")));
                }
                string confidFlag = string.IsNullOrEmpty(contact.Person.ConfidentialReason) ? "N" : "Y";
                contactFields.Add(new Tuple<string, object>("CONTACT.ADDRCONFID", confidFlag));
                contactFields.Add(new Tuple<string, object>("CONTACT.NAMECONFID", confidFlag));
            }
            else 
            {
                contactFields.Add(new Tuple<string, object>("CONTACT.TYPE", "O"));
                contactFields.Add(new Tuple<string, object>("CONTACT.TRADINGNAME", contact.Organization.Base.Name.Trim()));
                contactFields.Add(new Tuple<string, object>("CONTACT.SURNAME", contact.Organization.Base.Name.Trim()));
            }
            // #BP First see if there is a billing address (these take priority)
            var ozoneAddress = contact.BillingAddress;
            // #BP If there is no billing address, then use the preferred (postal) address (regardless of whether or not it is current (not end-dated))
            // #BP 20/03/2015 Remove the requirement for the preferred address to be current - IRIS allows the current address to be end-dated. If this occurs
            // we want to continue to send the preferred address until a replacement is identified
            //if (ozoneAddress == null && ozoneAddress.Base.IsCurrent) ozoneAddress = contact.PostalAddress;
            if (ozoneAddress == null) ozoneAddress = contact.PostalAddress;
            // #BP So long as the address is one of these types, add it to the XML
            if (ozoneAddress != null)
            {
                contactFields.Add(new Tuple<string, object>("CONTACT.ATYPE", ozoneAddress.Base.AddressType));
                // #BP This next bit is about the vanity. Building it up line by line, starting with the Prologue (if any), then Unit, Floor and Building
                // The initial tuple has no line number and is interpreted in the AddItem routine as <value dict="CONTACT.VANITY" line="">@CLEAR</value> 
                // which is used by Ozone to clear the entire dictionary field. Each subsequent tuple has a line number and an associated value which Ozone
                // uses to build the new value
                contactFields.Add(new Tuple<string, object>("CONTACT.VANITY", "@CLEAR"));

                //#BP 03/03/2016    Modified to enforce 60-character limit for any given line in the Vanity
                int iVanity = 1;
                if (ozoneAddress.Base.Prologue != null && ozoneAddress.Base.Prologue.Length > 0)
                {
                    // In case the Prologue has multi-lines, split it out by CrLf making one line each in the Vanity)
                    string[] result = ozoneAddress.Base.Prologue.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string vanity in result)
                    {
                        //contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), vanity));
                        contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), Truncate(vanity, 60)));
                        iVanity++;
                    }
                }
                // If there's a Unit Type specified then add it as a new line in the Vanity
                if (ozoneAddress.Base.UnitType != null && ozoneAddress.Base.UnitType.Length > 0)
                {
                    //contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), string.Format("{0} {1}", ozoneAddress.Base.UnitType,ozoneAddress.Base.UnitId)));
                    contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), Truncate(string.Format("{0} {1}", ozoneAddress.Base.UnitType, ozoneAddress.Base.UnitId), 60)));
                    iVanity++;
                }
                // If there's a Floor Type specified then add it as a new line in the Vanity
                if (ozoneAddress.Base.FloorType != null && ozoneAddress.Base.FloorType.Length > 0)
                {
                    //contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), string.Format("{0} {1}", ozoneAddress.Base.FloorType, ozoneAddress.Base.FloorId)));
                    contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), Truncate(string.Format("{0} {1}", ozoneAddress.Base.FloorType, ozoneAddress.Base.FloorId), 60)));
                    iVanity++;
                }
                // If there's a Building specified then add it as a new line in the Vanity
                if (ozoneAddress.Base.BuildingPropertyName != null && ozoneAddress.Base.BuildingPropertyName.Length > 0)
                {
                    //contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), ozoneAddress.Base.BuildingPropertyName));
                    contactFields.Add(new Tuple<string, object>("CONTACT.VANITY" + iVanity.ToString().Trim(), Truncate(ozoneAddress.Base.BuildingPropertyName,60)));
                    iVanity++;
                }

                if (ozoneAddress.Base.AddressTypeEnum == AddressType.PrivateBag ||
                    ozoneAddress.Base.AddressTypeEnum == AddressType.POBox)
                {
                    //#BP 03/03/2016    If the previous address type was Street, then there may be a residual RD number needing to be removed
                    contactFields.Add(new Tuple<string, object>("CONTACT.RDNO", ""));
                    // In Ozone the HouseNo holds the delivery service type, and Street holds the delivery service identifier (box or bag number)
                    contactFields.Add(new Tuple<string, object>("CONTACT.HOUSENO", (ozoneAddress.Base.AddressTypeEnum == AddressType.POBox) ? "PO BOX" : "PRIVATE BAG"));
                    contactFields.Add(new Tuple<string, object>("CONTACT.STREET", ozoneAddress.Base.AddressNumberText));
                }
                else
                {
                    if (ozoneAddress.Base.AddressTypeEnum == AddressType.Street || ozoneAddress.Base.AddressTypeEnum == AddressType.Overseas)
                    {
                        contactFields.Add(new Tuple<string, object>("CONTACT.HOUSENO", ozoneAddress.Base.AddressNumberText));
                    }
                    else
                    {
                        contactFields.Add(new Tuple<string, object>("CONTACT.HOUSENO", string.Format("{0}{1}",ozoneAddress.Base.HouseNumber,ozoneAddress.Base.StreetAlpha)));
                    }
                    contactFields.Add(new Tuple<string, object>("CONTACT.STREET", ozoneAddress.Base.StreetName));
                    if (ozoneAddress.Base.AddressTypeEnum == AddressType.Rural)
                    {
                        contactFields.Add(new Tuple<string, object>("CONTACT.RDNO", ozoneAddress.Base.AddressNumberText));
                    }
                    else
                    {
                        contactFields.Add(new Tuple<string, object>("CONTACT.RDNO", ""));
                    }
                }

                contactFields.Add(new Tuple<string, object>("CONTACT.SUBURB", ozoneAddress.Base.Suburb));
                contactFields.Add(new Tuple<string, object>("CONTACT.POSTCODE.NOV", ozoneAddress.Base.PostalCode));
                contactFields.Add(new Tuple<string, object>("CONTACT.TOWN", ozoneAddress.Base.TownLocality));

                if (ozoneAddress.Base.AddressTypeEnum == AddressType.OtherDelivery ||
                    ozoneAddress.Base.AddressTypeEnum == AddressType.Overseas)
                {
                    //contactFields.Add(new Tuple<string, object>("CONTACT.PADDR", ozoneAddress.FullAddress));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR1", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine1)) ? ozoneAddress.Base.AddressLine1 : " "));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR2", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine2)) ? ozoneAddress.Base.AddressLine2 : " "));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR3", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine3)) ? ozoneAddress.Base.AddressLine3 : " "));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR4", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine4)) ? ozoneAddress.Base.AddressLine4 : " "));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR5", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine5)) ? ozoneAddress.Base.AddressLine5 : " "));
                    contactFields.Add(new Tuple<string, object>("CONTACT.PADDR6", (!string.IsNullOrEmpty(ozoneAddress.Base.AddressLine6)) ? ozoneAddress.Base.AddressLine6 : " "));
                }
            }
            int iDialers = 1;
            foreach (string dialer in contact.Dialers)
            {
                if (contact.Dialers.Count > 1)
                {
                    contactFields.Add(new Tuple<string, object>("CONTACT.DIALER" + iDialers.ToString().Trim(), dialer));
                }
                else
                {
                    contactFields.Add(new Tuple<string, object>("CONTACT.DIALER", dialer));
                }
                iDialers++;
            }
           // contactFields.Add(new Tuple<string, object>("CONTACT.DIALER", contact.FullDialer));
            int iEmails = 1;
            if (contact.EmailAddress != null)
            {
                foreach (string email in contact.EmailAddresses)
                {
                    contactFields.Add(new Tuple<string, object>("CONTACT.EMAIL" + iEmails.ToString().Trim(), email));
                    iEmails++;
                }

                //contactFields.Add(new Tuple<string, object>("CONTACT.EMAIL.TYPE", "E"));
            }
            if (contact.Website != null)
            {
                foreach (string website in contact.Websites)
                {
                    contactFields.Add(new Tuple<string, object>("CONTACT.WEBSITE" + iEmails.ToString().Trim(), website));
                    iEmails++;
                }
                //contactFields.Add(new Tuple<string, object>("CONTACT.EMAIL", contact.Website));
                //contactFields.Add(new Tuple<string, object>("CONTACT.EMAIL.TYPE", "W")); 
            }
            
            AddItem(ref writer, ozoneContactId, "CS.CONTACTS", contactFields);

            AddFooter(ref writer);
            return Regex.Replace(writer.ToString(), "&(?!(amp|apos|quot|lt|gt);)", "&amp;");
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //#BP 03/03/2016    Added Truncate function (needed to restrict character-length in each row of the Vanity)
        public static string Truncate(string value, int length)
        {
            if (value.Length > length) return value.Substring(0, length);
            return value;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal static OzonePostResult ProcessPostResult(string xml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);
            OzonePostResult result = new OzonePostResult();
            foreach (XmlNode dataNode in xd.DocumentElement.SelectNodes("data"))
            {
                foreach (XmlNode errorAndItemNode in dataNode.ChildNodes)
                {
                    if (errorAndItemNode.Name == "item")
                    {
                        OzonePostResultItem item = new OzonePostResultItem();
                        foreach (XmlAttribute itemAttribute in errorAndItemNode.Attributes)
                        {
                            if (itemAttribute.Name == "id")
                            {
                                item.Id = itemAttribute.Value;
                            }
                            else if (itemAttribute.Name == "filename")
                            {
                                item.Filename = itemAttribute.Value;
                            }
                            else if (itemAttribute.Name == "status")
                            {
                                item.Status = itemAttribute.Value;
                            }
                        }
                        foreach (XmlNode fieldNode in errorAndItemNode.ChildNodes)
                        {
                            if (fieldNode.Attributes.Count > 0)
                            {
                                OzonePostResultValue value = new OzonePostResultValue();
                                foreach (XmlAttribute fieldAttribute in fieldNode.Attributes)
                                {
                                    if (fieldAttribute.Name == "dict")
                                    {
                                        value.Dict = fieldAttribute.Value;
                                    }
                                    else if (fieldAttribute.Name == "status")
                                    {
                                        value.Status = fieldAttribute.Value;
                                    }
                                    else if (fieldAttribute.Name == "value")
                                    {
                                        value.Value = fieldAttribute.Value;
                                    }
                                }
                                item.Values.Add(value);
                            }
                        }
                        result.Items.Add(item);
                    }
                    else if (errorAndItemNode.Name == "errors")
                    {
                        foreach (XmlNode errorNode in errorAndItemNode.ChildNodes)
                        {
                            result.Errors.Add(errorNode.Value);
                        }
                    }
                }
            }
            return result;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal static void SavePostXml(string xml, long IrisId)
        {
            string sql = "INSERT OzoneContactsQueue([MESSAGE], [STATUS], [IRISId]) SELECT {0}, {1}, {2}";                                
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
            {
                CommandHelper.ExecuteSqlQuery(sql, con, xml, (byte)OzoneQueueStatus.New, IrisId);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal static List<OzoneQueueItem> LoadPostXml()
        {
            List<OzoneQueueItem> items = new List<OzoneQueueItem>();
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"
                      SELECT o.[Id], o.[DateTime], o.[Message], o.[Status], o.[IrisId] 
                        FROM OzoneContactsQueue o (NOLOCK) 
                       WHERE O.[STATUS] IN ({0})",
                        (byte)OzoneQueueStatus.New);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OzoneQueueItem item = new OzoneQueueItem();
                            item.DateTime = CommandHelper.GetValueOrDefault<DateTime>(reader["DateTime"]);
                            item.Id = CommandHelper.GetValueOrDefault<int>(reader["Id"]);
                            item.Message = CommandHelper.GetValueOrDefault<string>(reader["Message"]);
                            item.Status = (OzoneQueueStatus)CommandHelper.GetValueOrDefault<byte>(reader["Status"]);
                            item.IrisId = CommandHelper.GetValueOrDefault<long>(reader["IrisId"]);
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal static int GetMigrationId(long Id)
        {            
            int migrationId = default(int);
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    string DbIris = ConfigurationManager.AppSettings["IRISDatabase"];
                    string DbPowerBuilder = ConfigurationManager.AppSettings["PowerBuilderDatabase"];
                    
                    command.CommandText = string.Format(@"
                      SELECT cast(cp.agent_id as int) as ID
			            FROM [{1}].dbo.Contact c (NOLOCK) 
			            JOIN [{2}].dbo.[cont_person] cp (NOLOCK) ON cp.agent_id = c.MigrationSourceID
		               WHERE c.Id = {0}
	                ORDER BY cp.agent_id",
                        Id,
                        DbIris,
                        DbPowerBuilder);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            migrationId = CommandHelper.GetValueOrDefault<int>(reader["ID"]);
                        }
                    }
                }
            }
            return migrationId;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal static void SavePostResult(OzonePostResult postResult, string xml, int queueId)
        {                                    
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
            {
                string sql = "INSERT OzoneContactsQueueEvent([QueueId], [Message], [Status]) SELECT {0}, {1}, {2}";
                SqlResult result = CommandHelper.ExecuteSqlQuery(sql, con, queueId, xml, (byte)postResult.Status);

                if (result.Successful)
                {
                    sql = "UPDATE C SET C.[Status] = {1} FROM OzoneContactsQueue C (ROWLOCK) WHERE C.ID = {0}";
                    result = CommandHelper.ExecuteSqlQuery(sql, con, queueId, (byte)postResult.Status);
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
