using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using HRC.Common.Data;
using System.ServiceModel;
using HRC.PowerBuilderContacts.BL;
using ContactsIntegration.BL;
using Iris=ContactsIntegration.ContactsService;
using HRC.OzoneContacts.BL;
using HRC.Framework.BL;
using HRC.Common.Exceptions;

namespace ContactsIntegration
{
    public partial class ProcessContactsQueue : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Retry Contact Sync at " + DateTime.Now.ToString() + "<br /><br />");
            string IrisDb = ConfigurationManager.AppSettings["IRISDatabase"];
            try
            {
                using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
                {

                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = string.Format(@"
                                       SELECT * FROM OzoneContactsQueue WHERE [IrisDB]='{0}' AND Status=0",
                            IrisDb);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long irisId = (long)reader["IrisId"];
                                int PowerBuilderId = (int)reader["PowerBuilderId"];
                                int OzoneContactId = (int)reader["OzoneContactId"];

                                sb.AppendLine("Processed : " + irisId + "");
                                Iris.ContactDetails contactDetails = null;
                                Iris.ContactsServiceClient client = new Iris.ContactsServiceClient();
                                try
                                {
                                  contactDetails = client.GetContactDetails(irisId);
                                }
                                catch (FaultException<IRISServiceFaultContract> ex)
                                {
                                    // MessageBox.Show(ex.Detail.FaultMessage);
                                    Logging.Write("ProcessContactsQueue", "Service Fault", "Error Occured", ExceptionInformation.GetExceptionStack(ex));
                                }
                                catch (FaultException ex)
                                {
                                    //MessageBox.Show(ex.Message);
                                    Logging.Write("ProcessContactsQueue", "", "Error Occured", ExceptionInformation.GetExceptionStack(ex));
                                }
                                finally
                                {
                                    if (client.State == CommunicationState.Faulted)
                                    {
                                        client.Abort();
                                    }
                                    else
                                    {
                                        client.Close();
                                    }
                                }

                                if (contactDetails != null)
                                {
                                    //Update Powerbuilder

                                    //Update Ozone

                                    //if updates where successful remove from queue
                                  //  SqlCommand commandUpdate = con.CreateCommand();
                                    string sql = "";
                                    
                                        if (UpdateContact(  contactDetails , PowerBuilderId, OzoneContactId))
                                        {
                                            //mark as processed
                                            sql = "UPDATE OzoneContactsQueue SET Status=1 WHERE [IrisDB]='{0}' AND IrisId={1}";
                                        }
                                        else
                                        {
                                            //increase retryCount
                                            sql = "UPDATE OzoneContactsQueue SET [RetryCount]=[RetryCount] + 1 WHERE [IrisDB]='{0}' AND IrisId={1}";

                                        }       
                                   
                                        try
                                        {
                                            using (SqlConnection con2 = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
                                            {
                                                using (SqlCommand command2 = con2.CreateCommand())
                                                {
                                                    command2.CommandText = string.Format(sql, IrisDb, irisId);
                                                    int x = command2.ExecuteNonQuery();
                                                    //sb.Append("3. - " + x + " - " + IrisDb + " - " + irisId + "<br />" + string.Format(sql, IrisDb, irisId) + "<br />");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                           // sb.Append("4<br />");
                                            Response.Write(ex.Message);
                                        }                                  
                                    //sb.AppendLine( " -  " + contactDetails.ContactType);
                                }
                                sb.Append("<br />");
                            }
                        }
                    }
                }
                Response.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        private bool UpdateContact(Iris.ContactDetails contactDetails, int powerBuilderId, int ozoneId)
        {
            bool retrySucceeded = false;
            
            //Logging.Write("UpdateContact", "Powerbuilder 1.2 (Both)", string.Empty);
            PBContact contact = Remap.IRISContactToPowerBuilderContact(contactDetails);
            int id = contact.Save(false, powerBuilderId.ToString());
            
         //   Response.Write("UpdateContact PB: " + id + "," + powerBuilderId);

            #region UpdateOzoneContact
            // UpdateOzoneContact(contactDetails, ozoneContactId, ref outcome); 

            OzoneContact contactOzone = Remap.IRISContactToOzoneContact(contactDetails);
            string ozoneContactId = ozoneId.ToString();

           // Logging.Write("ReSaveToOzone", "Ozone b4Save", string.Empty, contactOzone.GeneratePostXml(ozoneContactId));
            string ozoneErrorMessage = "";
            string ozoneSaveStatus = contactOzone.SaveToOzone(ref ozoneContactId, ref ozoneErrorMessage, contactDetails.ContactID);
            if (ozoneSaveStatus.ToUpper() == "FAILED")
            {
                Logging.Write("ReSaveToOzone", "Ozone Failed:" + ozoneSaveStatus, string.Empty, ozoneErrorMessage, contactDetails.ContactID);
                //Response.Write("<br />Retry Ozone Failed:" + ozoneErrorMessage + "<br />");
            }
            else
            {

            }
            #endregion




            if (id == powerBuilderId && ozoneSaveStatus.ToUpper() != "FAILED")
            {
                retrySucceeded = true;
                //Response.Write("<br />Retry Succeeded PB: " + id + "," + powerBuilderId + "<br />");
            }

            return retrySucceeded;
        }
    }
}