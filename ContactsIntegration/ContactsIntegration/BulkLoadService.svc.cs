using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using HRC.Framework.BL;
using System.Data.SqlClient;
using HRC.Common.Data;
using ContactsIntegration.ContactsService;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using HRC.Common.Exceptions;
using System.Threading;

namespace ContactsIntegration
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BulkLoadService" in code, svc and config file together.
    public class BulkLoadService : IBulkLoadService
    {
        public void BulkLoadContacts(string sourceID)
        {
            
            Logging.Write("BulkLoadContacts", "SourceID:" + sourceID, string.Empty, string.Empty);
            try
            {
                string xmlContacts = GetBulkLoadXml(sourceID);

                ProcessContactsXml(xmlContacts, sourceID);
            }

            catch (Exception ex)
            {
                Logging.Write("BulkLoadContacts", "SourceID:" + sourceID, "Error Occured", ex.Message);
                Logging.Write("BulkLoadContacts", "SourceID:" + sourceID, "Error Details", ex.InnerException.ToString());
            }
          
            return;
        }

        private string GetBulkLoadXml(string sourceID)
        {
            Logging.Write("BulkLoadContacts", "GetXML:" + sourceID, string.Empty);
            string xmlContactsCollection=string.Empty;
            try
            {
                using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.EDEStarGate))
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "SELECT [XML] FROM IRIS_Upload WHERE [SourceID]=@SouceID";
                        command.Parameters.AddWithValue("@SouceID", sourceID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                xmlContactsCollection = (string)reader["XML"];
                            }
                        }
                    }

                }
                
            }
            catch (Exception ex)
            {
                Logging.Write("BulkLoadContacts", "SourceID:" + sourceID, "Error Occured", ex.Message);
                Logging.Write("BulkLoadContacts", "SourceID:" + sourceID, "Error Details", ex.InnerException.ToString());
            }

            return xmlContactsCollection;
        }


        private string ProcessContactsXml(string xml, string sourceID)
        {

            Logging.Write("BulkLoadContacts", "ProcessContactsXMl:" + sourceID, string.Empty, xml);
            int bulkLoadOutcomeSuccess = -1;
            string errorMessage = "";
            string contactsInErrorXml = "";

            try
            {
                List<ContactsIntegration.ContactsService.ContactDetails> contactDetailsCollection = new List<ContactsIntegration.ContactsService.ContactDetails>();

                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xml);
                XmlSerializer xsSubmit = new XmlSerializer((new ContactsIntegration.ContactsService.ContactDetails()).GetType());
                foreach (XmlNode dataNode in xd.DocumentElement.SelectSingleNode("ContactDetailsCollection").SelectNodes("ContactDetails"))
                {
                    //txtXml.Text = dataNode.OuterXml;
                    string contactXml = dataNode.OuterXml;
                    contactXml = contactXml.Replace("<FirstName>", "<PersonFirstName>");
                    contactXml = contactXml.Replace("</FirstName>", "</PersonFirstName>");
                    contactXml = contactXml.Replace("<LastName>", "<PersonLastName>");
                    contactXml = contactXml.Replace("</LastName>", "</PersonLastName>");
                    contactXml = contactXml.Replace("<MiddleNames>", "<PersonMiddleNames>");
                    contactXml = contactXml.Replace("</MiddleNames>", "</PersonMiddleNames>");
                    contactXml = contactXml.Replace("<Name>", "<OrganisationName>");
                    contactXml = contactXml.Replace("</Name>", "</OrganisationName>");
        
                    StreamReader reader = new StreamReader(GenerateStreamFromString(contactXml));
                    ContactsIntegration.ContactsService.ContactDetails contactDetails = new ContactsIntegration.ContactsService.ContactDetails();
                    contactDetails = (ContactsIntegration.ContactsService.ContactDetails)xsSubmit.Deserialize(reader);
                    contactDetailsCollection.Add(contactDetails);

                    reader.Close();

                   
                }
                Logging.Write("BulkLoadContacts", "No Of Contacts:" + contactDetailsCollection.Count().ToString(), string.Empty);

                ContactsServiceClient client = new ContactsServiceClient();
                //ContactsIntegration.ContactsService.BulkLoadContactsOutcome bulkLoadOutcome = new ContactsService.BulkLoadContactsOutcome();
                ContactsIntegration.ContactsService.BulkLoadContactsOutcome bulkLoadOutcome = client.BulkLoadContacts(contactDetailsCollection.ToArray());

                //XmlSerializer xsSubmit3 = new XmlSerializer(contactDetailsCollection.ToArray().GetType());
                //StringWriter sww3 = new StringWriter();
                //XmlWriter writer3 = XmlWriter.Create(sww3);
                //xsSubmit3.Serialize(writer3, contactDetailsCollection.ToArray());
                //var xmlcontactDetailsCollection = sww3.ToString(); // Your xml

                
                XmlSerializer xsSubmit4 = new XmlSerializer(bulkLoadOutcome.ContactInErrorCollection.GetType());
                StringWriter sww4 = new StringWriter();
                XmlWriter writer4 = XmlWriter.Create(sww4);
                xsSubmit4.Serialize(writer4, bulkLoadOutcome.ContactInErrorCollection);
                contactsInErrorXml = sww4.ToString(); // Your xml
                if (bulkLoadOutcome.Success.ToLower() == "pass")
                {
                    bulkLoadOutcomeSuccess = 0;
                }
                else if (bulkLoadOutcome.Success.ToLower() == "fail")
                {
                    bulkLoadOutcomeSuccess = 1;
                }
                else
                {
                    bulkLoadOutcomeSuccess = 2;
                }
                Logging.Write("BulkLoadContacts", bulkLoadOutcome.Success.ToLower(), bulkLoadOutcomeSuccess.ToString());
                errorMessage = bulkLoadOutcome.ErrorMessage;                
            }
            catch (Exception ex)
            {
                Logging.Write("BulkLoadContacts", "ProcessContactsXml", "Error Occured", ExceptionInformation.GetExceptionStack(ex));
                contactsInErrorXml = string.Empty; 
                bulkLoadOutcomeSuccess=1;
                errorMessage = ExceptionInformation.GetExceptionStack(ex);
            }


            try
            {
                Logging.Write("BulkLoadContacts", "ProcessContactsXml", "Error Occured", string.IsNullOrEmpty(errorMessage)?"":errorMessage);
                string sql = @"UPDATE [IRIS_Upload] SET [Outcome]=@Outcome, [ErrorMessage]=@ErrorMessage, 
                        [ContactsInError]=@ContactsInError, [ServiceReturnTime]=GETDATE() WHERE [SourceID]=@SouceID";
                using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.EDEStarGate))
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        
                        //command.CommandText = string.Format(sql, sourceID, bulkLoadOutcomeSuccess, errorMessage.Replace("'","''"), contactsInErrorXml);
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@SouceID", sourceID);
                        command.Parameters.AddWithValue("@Outcome", bulkLoadOutcomeSuccess);
                        command.Parameters.AddWithValue("@ErrorMessage", string.IsNullOrEmpty(errorMessage)?"":errorMessage);
                        command.Parameters.AddWithValue("@ContactsInError", string.IsNullOrEmpty(contactsInErrorXml) ? "" : contactsInErrorXml);

                        int x = command.ExecuteNonQuery();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Write("BulkLoadContacts", "ProcessContactsXml", "Error Occured", ExceptionInformation.GetExceptionStack(ex));
            }     

            return "";
        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        public IAsyncResult BeginAsyncBulkLoadContacts(string sourceID, AsyncCallback callback, object asyncState)
        {
            Console.WriteLine("BeginServiceAsyncMethod called with: \"{0}\"", sourceID);
            BulkLoadContacts(sourceID);

            var result = new CompletedAsyncResult<string>(sourceID);
            return result ;
        }

        public string EndAsyncBulkLoadContacts(IAsyncResult r)
        {
            CompletedAsyncResult<string> result = r as CompletedAsyncResult<string>;
            Console.WriteLine("EndServiceAsyncMethod called with: \"{0}\"", result.Data);
            return result.Data;
        }
        


    }

    // Simple async result implementation.
    class CompletedAsyncResult<T> : IAsyncResult
    {
        T data;

        public CompletedAsyncResult(T data)
        { this.data = data; }

        public T Data
        { get { return data; } }

        #region IAsyncResult Members
        public object AsyncState
        { get { return (object)data; } }

        public WaitHandle AsyncWaitHandle
        { get { throw new Exception("The method or operation is not implemented."); } }

        public bool CompletedSynchronously
        { get { return true; } }

        public bool IsCompleted
        { get { return true; } }
        #endregion
    }
}
