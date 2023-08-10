﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using ContactsIntegration.BL;
using HRC.Framework.BL;
using HRC.PowerBuilderContacts.BL;
using HRC.OzoneContacts.BL;
using HRC.DatascapeContacts.BL;
using HRC.Common.Exceptions;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using HRC.Common.Data;
using System.Threading.Tasks;

namespace ContactsIntegration
{    
    internal enum ContactSaveMethod
    {
        None,
        PowerBuilder,
        Ozone,
        Datascape,
        All
    }
    internal enum OtherIdentifierContext{
        PowerBuilderContactId,
        OzoneContactId,
        DatascapeContactId
    }

    public class ContactsIntegrationService : IContactsIntegrationService
    {
        private static ContactSaveMethod contactSaveMethod = ContactSaveMethod.All;

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public CreateContactOutcome CreateContact(ContactDetails contactDetails)
        {

            #region DebugContactDetailsObject
            XmlSerializer xsSubmit1 = new XmlSerializer(contactDetails.GetType());
            StringWriter sww1 = new StringWriter();
            XmlWriter writer1 = XmlWriter.Create(sww1);
            xsSubmit1.Serialize(writer1, contactDetails);
            var xml1 = sww1.ToString(); // Your xml
            Logging.Write("CreateContact", "ContactDetails XML", "BEGIN", xml1, contactDetails.ContactID);
            #endregion

            getContactSaveMethod();
            CreateContactOutcome outcome = new CreateContactOutcome();
            
            outcome.ErrorMessage = string.Empty;
            try
            {
                if (contactSaveMethod == ContactSaveMethod.PowerBuilder)  //save directly to PowerBuilder only
                {
                    Logging.Write("CreateContact", "PowerBuilder Only", string.Empty, string.Empty, contactDetails.ContactID);
                    outcome = AddPowerBuilderContact(contactDetails, ref outcome);
            
                }
                else if (contactSaveMethod == ContactSaveMethod.Datascape) //save directly to Datascape only
                {
                    Logging.Write("CreateContact", "Datascape Only", string.Empty, string.Empty, contactDetails.ContactID);
                    AddDatascapeContact(contactDetails, ref outcome);
                    //outcome.FINCustomerCode = "";
                    
                }
                else if (contactSaveMethod == ContactSaveMethod.Ozone) //save directly to Ozone only
                {
                    Logging.Write("CreateContact", "Ozone Only", string.Empty, string.Empty, contactDetails.ContactID);
                    AddOzoneContact(contactDetails, ref outcome);
                    //outcome.FINCustomerCode = "";
                    outcome.Success = true;

                }
                else if (contactSaveMethod == ContactSaveMethod.All) //save to PowerBuilder, Ozone and Datascape               
                {
                    Logging.Write("CreateContact", "PowerBuilder 4.2 (Both)", string.Empty, string.Empty, contactDetails.ContactID);
                    outcome = AddPowerBuilderContact(contactDetails, ref outcome);
                    AddOzoneContact(contactDetails, ref outcome);
                    AddDatascapeContact(contactDetails, ref outcome);              
                }
                //outcome.Success = true;
                

            }
            catch (Exception ex)
            {
                outcome.Success = false;
                outcome.ErrorMessage = "CreateContact Error: " + ExceptionInformation.GetExceptionStack(ex);
                Logging.Write("CreateContact", "Error", ex.Message.Substring(0, ex.Message.Length > 200 ? 200 : ex.Message.Length), ExceptionInformation.GetExceptionStack(ex), contactDetails.ContactID);
                //??? IRISServiceFaultContract?

                /* *
                 *  Where the web service fails to generate the Contact or update the applicable details, 
                 *  an email notification should be sent to requests@horizons.govt.nz with the details of the error. 
                 *  Ideally the user should be notified of the error, however it is not anticipated that full error details are sent to the end user
                 * */
            }

            XmlSerializer xsSubmit = new XmlSerializer(outcome.GetType());

            StringWriter sww = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, outcome);
            var xml = sww.ToString(); // Your xml
            if (!string.IsNullOrEmpty(outcome.ErrorMessage)){//error during save
                //log contact for retry;
                AddContactToRetryQueue(contactDetails.ContactID, "","", outcome.ErrorMessage);
            }
            Logging.Write("CreateContact", outcome.Success ? "SUCCESS" : "FAILED", "END", string.Empty, contactDetails.ContactID);
            return outcome;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static CreateContactOutcome AddPowerBuilderContact(ContactDetails contactDetails, ref CreateContactOutcome outcome)
        {
            PBContact contactPB = Map.IRISContactToPowerBuilderContact(contactDetails);
            int PBContactId = contactPB.Save(true,"");
            if (PBContactId > 0)
            {
                addOtherIdentifier(ref outcome, OtherIdentifierContext.PowerBuilderContactId, PBContactId.ToString());
            }
            else
            {
                outcome.ErrorMessage += string.IsNullOrEmpty(outcome.ErrorMessage) ? "" : "," + " [FAILED to Save Contact to Powerbuilder]";
            }
            Logging.Write("CreateContact", "PowerBuilder AddedContact", PBContactId.ToString(), contactDetails.ContactID);
            return outcome;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddOzoneContact(ContactDetails contactDetails, ref CreateContactOutcome outcome)
        {
            Logging.Write("CreateContact", "AddOzoneContact", string.Empty, contactDetails.ContactID);
            OzoneContact contactOzone = Map.IRISContactToOzoneContact(contactDetails);

           string ozoneContactId = "NEW";
           string ozoneErrorMessage = ""; 
           string ozoneSaveStatus = contactOzone.SaveToOzone(ref ozoneContactId, ref ozoneErrorMessage, contactDetails.ContactID);
           Logging.Write("CreateContact", "Ozone SaveStatus", ozoneSaveStatus, contactOzone.GeneratePostXml(ozoneContactId), contactDetails.ContactID);
            
            if (ozoneSaveStatus.ToUpper() == "FAILED")
            {
                outcome.ErrorMessage += string.IsNullOrEmpty(outcome.ErrorMessage)?"":"," + " [FAILED to Save Contact to Ozone] ";
            }
            else
            {
                addOtherIdentifier(ref outcome, OtherIdentifierContext.OzoneContactId, ozoneContactId);
                outcome.FINCustomerCode = ozoneContactId; //will be the same as the Ozone Contact ID
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddDatascapeContact(ContactDetails contactDetails, ref CreateContactOutcome outcome)
        {
            //TODO
            Logging.Write("CreateContact", "AddDatascapeContact", string.Empty, contactDetails.ContactID);
            DatascapeContact ContactDatascape = Map.IRISContactToDatascapeContact(contactDetails);

            string DatascapeContactID = "";
            string DatascapeErrorMessage = "";
            string DatascapeSaveStatus = ContactDatascape.SaveToDatascape(ref DatascapeContactID, ref DatascapeErrorMessage);
            Logging.Write("CreateContact", "Datascape Save Status", DatascapeSaveStatus, string.Empty, contactDetails.ContactID);

            if (DatascapeSaveStatus.ToUpper() == "FAILED")
            {
                outcome.ErrorMessage += (string.IsNullOrEmpty(outcome.ErrorMessage) ? "" : ",") + " [FAILED to Save Contact to Datascape]";
                Logging.Write("CreateContact", "Datascape Error Message",  string.Empty, DatascapeErrorMessage, contactDetails.ContactID);
            }
            else
            {
                Logging.Write("CreateContact", $"DatascapeID: {DatascapeContactID}", string.Empty, string.Empty, contactDetails.ContactID);
                addOtherIdentifier(ref outcome, OtherIdentifierContext.DatascapeContactId, DatascapeContactID);
                outcome.Success = true;
            }
            
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public UpdateContactOutcome UpdateContact(ContactDetails contactDetails, OtherIdentifiers contactOtherIdentifiers, CDFs contactCDFs)
        {

            #region DebugContactDetailsObject
            XmlSerializer xsSubmit1 = new XmlSerializer(contactDetails.GetType());
            StringWriter sww1 = new StringWriter();
            XmlWriter writer1 = XmlWriter.Create(sww1);
            xsSubmit1.Serialize(writer1, contactDetails);
            var xml1 = sww1.ToString(); // Your xml
            Logging.Write("UpdateContact", "ContactDetails XML", "BEGIN", xml1, contactDetails.ContactID);
            #endregion
            #region DebugOtherIdentifierObject
            XmlSerializer xsSubmit2 = new XmlSerializer(contactOtherIdentifiers.GetType());
            StringWriter sww2 = new StringWriter();
            XmlWriter writer2 = XmlWriter.Create(sww2);
            xsSubmit2.Serialize(writer2, contactOtherIdentifiers);
            var xml2 = sww2.ToString(); // Your xml
            Logging.Write("UpdateContact", "ContactOtherIdentifiers XML", "BEGIN", xml2, contactDetails.ContactID);
            #endregion


            UpdateContactOutcome outcome = new UpdateContactOutcome();
            string PBContactId = "";
            string ozoneContactId = "NEW";
            string DatascapeNumber = "";
            outcome.ErrorMessage = string.Empty; 
            try
            {
                getContactSaveMethod();
                
                if (contactDetails.FINCustomerCode != null && contactDetails.FINCustomerCode.Length > 0)
                {
                    ozoneContactId = contactDetails.FINCustomerCode;
                }
                
                foreach (OtherIdentifier otherId in contactOtherIdentifiers)
                {
                    if (otherId.Context == "PowerBuilderID")
                    {
                        PBContactId = otherId.Value;
                    }
                    if (otherId.Context == "DatascapeID")
                    {
                        DatascapeNumber = otherId.Value;
                    }
                    else if (otherId.Context == "OzoneID")
                    {
                        if (ozoneContactId == "NEW" || ozoneContactId.Trim() == string.Empty)
                        {
                            ozoneContactId = otherId.Value;
                        }
                    }
                }

                if (contactSaveMethod == ContactSaveMethod.PowerBuilder)
                {
                    Logging.Write("UpdateContact", "Powerbuilder Only", string.Empty, contactDetails.ContactID);
                    //save directly to PowerBuilder Contacts
                    PBContact contact = Map.IRISContactToPowerBuilderContact(contactDetails);

                    Logging.Write("UpdateContact", "Powerbuilder ContactId", PBContactId.ToString(), contactDetails.ContactID);
                    contact.Save(false, PBContactId);

                    //determine outcome
                    outcome.Success = true;
                    
                }
                else if (contactSaveMethod == ContactSaveMethod.Ozone)
                {
                    Logging.Write("UpdateContact", "Ozone Only", string.Empty, contactDetails.ContactID);
                    OtherIdentifier ozoneIdentifier = contactOtherIdentifiers.Where(i => i.Context.Equals(OtherIdentifierContext.OzoneContactId)).First();
                    if (ozoneIdentifier != null)
                    {
                        Logging.Write("UpdateContact", "Ozone Only", string.Empty, ozoneIdentifier.Value, contactDetails.ContactID);
                    }

                    UpdateOzoneContact(contactDetails, ozoneContactId, ref outcome);
                    outcome.Success = true;
                    
                    //determine outcome
                }
                else if (contactSaveMethod == ContactSaveMethod.Datascape)
                {
                    Logging.Write("UpdateContact", "Datascape Only", string.Empty, contactDetails.ContactID);

                    UpdateDatascapeContact(contactDetails, DatascapeNumber, ref outcome);
                    if (string.IsNullOrEmpty(outcome.ErrorMessage))
                    {
                        outcome.Success = true;
                    }
                    else
                    {
                        outcome.Success = false;
                    }

                    //determine outcome
                }
                else if (contactSaveMethod == ContactSaveMethod.All)
                {
                    //save directly to PowerBuilder Contacts
                    Logging.Write("UpdateContact", "All", string.Empty, contactDetails.ContactID);
                    PBContact contact = Map.IRISContactToPowerBuilderContact(contactDetails);
                   

                    int id = contact.Save(false, PBContactId);
                    XmlSerializer xsSubmit = new XmlSerializer(contact.GetType());
                    StringWriter sww = new StringWriter();
                    XmlWriter writer = XmlWriter.Create(sww);
                    xsSubmit.Serialize(writer, contact);
                    var xml = sww.ToString(); // Your xml
                    Logging.Write("UpdateContact", "Updated PB Id", id.ToString() + " - " + PBContactId, xml, contactDetails.ContactID);
                    UpdateOzoneContact(contactDetails, ozoneContactId, ref outcome);
                    UpdateDatascapeContact(contactDetails, DatascapeNumber, ref outcome);
                    
                    if (string.IsNullOrEmpty(outcome.ErrorMessage)){
                        outcome.Success = true;         
                    }else{
                        outcome.Success=false;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                outcome.Success = false;
                outcome.ErrorMessage = "UpdateContact Error: " + ex.StackTrace;//ex.Message;
                Logging.Write("UpdateContact", "Error", string.Empty, ExceptionInformation.GetExceptionStack(ex), contactDetails.ContactID);
                //??? IRISServiceFaultContract?

                /* *
                 *  Where the web service fails to generate the Contact or update the applicable details, 
                 *  an email notification should be sent to requests@horizons.govt.nz with the details of the error. 
                 *  Ideally the user should be notified of the error, however it is not anticipated that full error details are sent to the end user
                 * */
            }
            //if (string.IsNullOrEmpty(outcome.ErrorMessage))
            //{//force queue testing
            //    outcome.ErrorMessage = "TESTING RETRY QUEUE";

            //}
            if (!string.IsNullOrEmpty(outcome.ErrorMessage))
            {//error during save log contact for retry;
                Logging.Write("AddToRetryQueue", "ContactID", contactDetails.ContactID.ToString(), outcome.ErrorMessage, contactDetails.ContactID);
                AddContactToRetryQueue(contactDetails.ContactID, PBContactId, ozoneContactId, outcome.ErrorMessage);
            }
            Logging.Write("UpdateContact", outcome.Success?"SUCCESS":"FAILED", "END",string.Empty, contactDetails.ContactID);
            return outcome;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void UpdateOzoneContact(ContactDetails contactDetails, string ozContactId, ref UpdateContactOutcome outcome)
        {
            OzoneContact contact = Map.IRISContactToOzoneContact(contactDetails);
            string ozoneContactId = ozContactId;

            Logging.Write("SaveToOzone", "Ozone Post Xml", string.Empty, contact.GeneratePostXml(ozoneContactId), contactDetails.ContactID);
            string ozoneErrorMessage = "";
            string ozoneSaveStatus = contact.SaveToOzone(ref ozoneContactId, ref ozoneErrorMessage, contactDetails.ContactID);

            if (ozoneSaveStatus.ToUpper() == "FAILED")
            {
                
                outcome.ErrorMessage =  outcome.ErrorMessage + (string.IsNullOrEmpty(outcome.ErrorMessage) ? "" : ", ") + " [Failed to Update Ozone Contact] - " + ozoneErrorMessage;
                Logging.Write("UpdateContact", "Ozone SaveStatus:" + ozoneSaveStatus, string.Empty, outcome.ErrorMessage, contactDetails.ContactID);
            }
            else
            {
                Logging.Write("UpdateContact", "Ozone SaveStatus:" + ozoneSaveStatus + "*", string.Empty, outcome.ErrorMessage, contactDetails.ContactID);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void UpdateDatascapeContact(ContactDetails contactDetails, string DatascapeContactNumber, ref UpdateContactOutcome outcome)
        {

            DatascapeContact contact = Map.IRISContactToDatascapeContact(contactDetails);
            string DatascapeContactID = DatascapeContactNumber;

            //contact.ManageAccountModifications(ref DatascapeContactNumber);
            contact.ManageContactInfoStatus(ref DatascapeContactID);

            Logging.Write("SaveToDatascape", "Update Datascape Contact", string.Empty, DatascapeContactNumber);
            string DatascapeErrorMessage = "";
            string DatascapeSaveStatus = contact.SaveToDatascape(ref DatascapeContactID, ref DatascapeErrorMessage);

            if (DatascapeSaveStatus.ToUpper() == "FAILED")
            {
                outcome.ErrorMessage = outcome.ErrorMessage + (string.IsNullOrEmpty(outcome.ErrorMessage) ? "" : ", ") + "[Failed to Update Datascape Contact] - " + DatascapeErrorMessage;
                Logging.Write("UpdateContact", "Datascape SaveStatus:" + DatascapeSaveStatus + "*", string.Empty, outcome.ErrorMessage, contactDetails.ContactID);
            }
            else
            {
                Logging.Write("UpdateContact", "Datascape SaveStatus:" + DatascapeSaveStatus + "*", string.Empty, outcome.ErrorMessage, contactDetails.ContactID);
            }

        
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void addOtherIdentifier(ref CreateContactOutcome outcome, OtherIdentifierContext context, string value)
        {
            OtherIdentifier otherId = new OtherIdentifier();
            switch (context)
            {
                case OtherIdentifierContext.PowerBuilderContactId:
                    otherId.Context = "PowerBuilderID";
                    break;
                case OtherIdentifierContext.OzoneContactId:
                    otherId.Context = "OzoneID";
                    break;
                case OtherIdentifierContext.DatascapeContactId:
                    otherId.Context = "DatascapeID";
                        break;
                default:
                    otherId.Context = "Unknown Identifier";
                    break;
            }
            otherId.Value = value;
            if (outcome.OtherIdentifiers == null)
            {
                outcome.OtherIdentifiers = new OtherIdentifiers();
            }
            outcome.OtherIdentifiers.Add(otherId);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private IRISServiceFaultContract IRISServiceFaultContract(string errorMessage, int faultCode = 1)
        {
            IRISServiceFaultContract serviceFaultContract = new IRISServiceFaultContract();
            serviceFaultContract.FaultCode = -1;
            serviceFaultContract.FaultMessage = errorMessage;
            return serviceFaultContract;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void getContactSaveMethod()
        {
            string contactSaveMethodConfig = ConfigurationManager.AppSettings["ContactSaveMethod"];
            if (contactSaveMethodConfig.ToLower() == "all") contactSaveMethod = ContactSaveMethod.All;
            if (contactSaveMethodConfig.ToLower() == "powerbuilder") contactSaveMethod = ContactSaveMethod.PowerBuilder;
            if (contactSaveMethodConfig.ToLower() == "ozone") contactSaveMethod = ContactSaveMethod.Ozone;
            if (contactSaveMethodConfig.ToLower() == "datascape") contactSaveMethod = ContactSaveMethod.Datascape;
            if (contactSaveMethodConfig.ToLower() == "none") contactSaveMethod = ContactSaveMethod.None;

        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void AddContactToRetryQueue(long IrisId, string PowerBuilderId, string OzoneContactId, string message)
        {
            string IrisDb = ConfigurationManager.AppSettings["IRISDatabase"];
            string sql = @"IF  ((SELECT COUNT(*) FROM OzoneContactsQueue WHERE [IrisID]={2} AND Status=0 AND [IrisDB]={3})=0) 
                        BEGIN
                        INSERT OzoneContactsQueue([MESSAGE], [STATUS], [IRISId],[IrisDB],[PowerBuilderID],[OzoneContactId]) 
                        SELECT {0}, {1}, {2}, {3},{4},{5}
                        END
                        ELSE
                        UPDATE OzoneContactsQueue SET [FailedCount]=[FailedCount]+1 WHERE  [IrisID]={2} AND Status=0 AND [IrisDB]={3}";
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.OzoneContactsQueue))
            {
                
                CommandHelper.ExecuteSqlQuery(sql, con, message, (byte)OzoneQueueStatus.New, IrisId, IrisDb, PowerBuilderId, OzoneContactId);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
