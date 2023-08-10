using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using HRC.Common;
using HRC.Common.Data;
using HRC.Framework.BL;
using Contacts = FinancialsIntegration.ContactsService;
using System.Configuration;
using System.Xml.Linq;
using HRC.Common.Exceptions;
using HRC.Common.Configuration;
using System.Net.Mail;
using HRC.IRIS.BL;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using HRC.DatascapeFinancials;
using HRC.DatascapeFinancials.DL;
using HRC.DatascapeFinancials.BL;

namespace FinancialsIntegration
{    
    public class FinancialsIntegrationService : IFinancialsIntegrationService
    {
        private static RestClient _client = new RestClient(new RestClientOptions(@"https://datascape.cloud/"));
        public CreateFinancialCustomerOutcome CreateFinancialCustomer(Contacts.ContactDetails contactDetails)
        {
            Logging.Write("CreateFinancialCustomer", string.Empty, string.Empty);

            CreateFinancialCustomerOutcome outcome = new CreateFinancialCustomerOutcome();

            outcome.Success = true;
            outcome.ErrorMessage = null;
                                                                                                     
            return outcome;
        }

        public CreateFinancialProjectOutcome CreateFinancialProject(RecordData recordData)
        {
            #region DebugrecordDataObject
            XmlSerializer xsSubmit1 = new XmlSerializer(recordData.GetType());
            StringWriter sww1 = new StringWriter();
            XmlWriter writer1 = XmlWriter.Create(sww1);
            xsSubmit1.Serialize(writer1, recordData);
            var xml1 = sww1.ToString(); // Your xml
            Logging.Write("CreateFinancialProject", "RecordData XML", "BEGIN", xml1);
            #endregion

            CreateFinancialProjectOutcome outcome = new CreateFinancialProjectOutcome();
            Logging.Write("CreateFinancialProject", string.Empty, string.Empty);
            if (recordData.FINProjectCode.Length != 6)
            {
                outcome.Success = false;
                outcome.ErrorMessage = string.Format(@"An Error has occurred while processing the Create Financial Project Request for {0}.                                
                            The proposed Job Number (Project Code) has failed Validation and cannot be created.  
                            If this error occurs again, contact your System Support team.", recordData.IrisID);                
                return outcome;
            }

            //string sDesc = recordData.IrisID + " " + recordData.Subclassification1 + " " + recordData.Subclassification2;
            //#BP 15/06/2016 Due to constraints on field length we can't use 'Resource Consent' so fallback is 'Regulatory'
            string sDesc = recordData.IrisID + " Regulatory";
            string jobGroup = "";
            string error001 = string.Format(@"An Error has occurred while processing the Create Financial Project Request for {0}.                                
                            Please refresh your browser and try again. If this error occurs again, contact your System Support team.", recordData.IrisID);
            string warning001 = @"A Financial Project has already been created to represent this Activity.                                
                                Please use the ‘Select Financial Project’ to locate the applicable Project Number.";
            string warning002 = string.Format(@"Create Financial Project has not been configured for object type {0}.                                
                            Please contact your System Support team if you believe this is a mistake.", recordData.ObjectType);
            try
            {



                switch (recordData.ObjectType)
                {
                    case "5": //Application
                    case "Application":
                        if (recordData.Subclassification1 == "33" || recordData.Subclassification1 == "Resource Consent")
                        {
                            jobGroup = "RCP";
                        }
                        else
                        {   //Generate WARNING.001
                            jobGroup = "WARNING.002";
                        }
                        break;
                    case "16":// Management Site
                    case "Management Site":
                    case "15": //Programme
                    case "Programme":
                    case "20": //Request
                    case "Request":
                        jobGroup = "WARNING.002";
                        break;
                    case "12": //Regime
                    case "Regime":
                        jobGroup = "RCM";
                        break;
                    case "6": //Authorisations
                    case "Authorisations":
                    case "Authorisation":
                    case "13": //Regime Activity
                    case "Regime Activity":
                    case "25": //Enforcement
                    case "Enforcement":
                    case "27": //Enfocement Action
                    case "Enfocement Action":
                    default:
                        //Generate Warning.001
                        jobGroup = "WARNING.002";
                        break;
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                RestRequest LookupRequest = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISFinancialProjectLookup?Filter=UniqueKeyValue%20like%20%60{recordData.FINProjectCode}%60", Method.Get);
                LookupRequest.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
                
                List<WorkOrder> WorkOrders = _client.GetAsync<List<WorkOrder>>(LookupRequest).Result;
                if (WorkOrders.Any()) jobGroup = "WARNING.001";

                if (jobGroup == "WARNING.001")
                { //failed business rules
                    outcome.Success = false;
                    outcome.ErrorMessage = warning001;
                    Logging.Write("CreateFinancialProject","WARNING.001",string.Empty,warning001);
                    return outcome;
                }
                if (jobGroup == "WARNING.002")
                { //has not been configured in Datascape
                    outcome.Success = false;
                    outcome.ErrorMessage = warning002;
                    Logging.Write("CreateFinancialProject", "WARNING.002", string.Empty, warning002);
                    return outcome;
                }
                else if (jobGroup == "ERROR.001")
                { //error occurred
                    outcome.Success = false;
                    outcome.ErrorMessage = error001;
                    Logging.Write("CreateFinancialProject", "ERROR.001", string.Empty, error001);
                    return outcome;
                }
                else
                { 
                    string jobId = recordData.FINProjectCode;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISFinancialProjectSubmission/SendMessage", Method.Post);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
                    var json = JsonConvert.SerializeObject(new FinanceProjectPayload {
                        FinanceProject = new FinanceProject {
                            DatascapeJobGroup = jobGroup,
                            Description = sDesc,
                            LongDescription = recordData.Description,
                            ProjectCode = jobId
                        } 
                    });
                    request.AddBody(json);
                    RestResponse response = _client.ExecuteAsync(request).Result;

                    string responseContent = response.Content;
                    string saveResultStatus = "";
                    string DatascapeErrorMessage = "";


                    switch (response.ResponseStatus)
                    {
                        case ResponseStatus.Completed:
                            // Request was successfully executed, handle the response accordingly
                            break;
                        case ResponseStatus.Error:
                            // An error occurred during request execution, handle the error
                            saveResultStatus = "FAILED";
                            DatascapeErrorMessage = responseContent ?? "";
                            throw response.ErrorException;
                        case ResponseStatus.TimedOut:
                            // Request timed out, handle the timeout scenario
                            saveResultStatus = "FAILED";
                            DatascapeErrorMessage = responseContent ?? "";
                            throw new Exception($"{response.ErrorException.Message} {responseContent ?? ""}");
                        default:
                            // Handle unknown or unsupported response status
                            saveResultStatus = "FAILED";
                            DatascapeErrorMessage = responseContent ?? "";
                            throw new Exception($"{response.ErrorException.Message} {responseContent ?? ""}");
                    }

                    // Check StatusCode
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            // Request was successful, handle the 200 OK scenario
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
                            DatascapeErrorMessage = responseContent ?? "";
                            throw new Exception($"{response.StatusDescription} {responseContent ?? ""}");
                        default:
                            // Handle other status codes
                            saveResultStatus = "FAILED";
                            DatascapeErrorMessage = responseContent ?? "";
                            throw new Exception($"{response.StatusDescription} {responseContent ?? ""}");
                    }
                    if (saveResultStatus == "FAILED")
                    {
                        outcome.Success = false;
                        outcome.ErrorMessage = error001;

                        Logging.Write("CreateFinancialProject", "ERROR", "Job Copy Failed", outcome.ErrorMessage);

                        string jobCopyFailEmailTo = ConfigurationManager.AppSettings["JobCopyFailEmailTo"];
                        string body = "The following error was returned as part of creating a financial project for " + recordData.IrisID + Environment.NewLine + Environment.NewLine
                            + string.Format(@"{0}", outcome.ErrorMessage) ;
                        string applicationName = ConfigurationManager.AppSettings["ApplicationName"];
                        //string displayName = string.Format("{0} Logs", Application.ProductName);
                        string subject = "Error on Job Copy function for " + recordData.IrisID;
                        new Email()
                            .SetFrom(new MailAddress(CommonConfig.Instance.MailFromEmail))
                            .AddTo(jobCopyFailEmailTo)
                            .SetSubject(subject)
                            .SetBody(body)
                            .Send();

                        return outcome;
                    }
                    else
                    {
                        outcome.Success = true;
                        try
                        {
                            Logging.Write("SetIsFinProjectCodeConfirmed", recordData.IrisID, string.Empty);
                            IRISObject.SetIsFinProjectCodeConfirmed(recordData.IrisID, true);
                        }
                        catch (Exception ex)
                        {
                            Logging.Write("SetIsFinProjectCodeConfirmed", string.Empty, string.Empty, ExceptionInformation.GetExceptionStack(ex));
                        }
                        
                        return outcome;
                    }

                    
                }
            }
            catch (Exception ex)
            {
                outcome.Success = false;
                outcome.ErrorMessage = error001;
                Logging.Write("CreateFinancialProject", "ERROR.001", string.Empty, ExceptionInformation.GetExceptionStack(ex));
                //TODO Log error
                //TODO return an IRISServiceFaultContract
            }


            Logging.Write("CreateFinancialProject", "Template: " + jobGroup, string.Empty, "ObjectType: " + recordData.ObjectType
                + ", SubClass1: " + recordData.Subclassification1 + ", SubClass2: " +recordData.Subclassification2 + ", IrisId: " + recordData.IrisID
                + ", FINCProjectCode: " + recordData.FINProjectCode);
    
            return outcome;
        }

        public CreateFinancialProjectOutcome CreateFinancialProjectWRC(RecordDataWRC recordDataWRC)
        {
            Logging.Write("CreateFinancialProjectWRC", string.Empty, string.Empty);

            CreateFinancialProjectOutcome outcome = new CreateFinancialProjectOutcome();
            return outcome;
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public FinancialProjects FindFinancialProject(FinancialProjectSearchCriteria searchCriteria)
        {

            Logging.Write("FindFinancialProject", "Data Received", searchCriteria.ToString());
            string criteria = "";

            string finCustomerCode = searchCriteria[FinancialProjectSearchCriteriaKey.FINCustomerCode];
            string searchText = searchCriteria[FinancialProjectSearchCriteriaKey.SearchText];

            criteria += "FINCustomerCode: " + finCustomerCode;
            criteria += ", SearchText: " + searchText;

            Logging.Write("FindFinancialProject", "Search Criteria", criteria);

            string customerName = "";
            FinancialProjects projects = new FinancialProjects();
            if (string.IsNullOrEmpty(searchText)) return projects;
            string returnedData = "";


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISFinancialProjectLookup?Filter=UniqueKeyValue%20like%20%60{searchText}%60%20or%20Description%20like%20%60{searchText}%60%20or%20%23Custom.IRISRCMLongDescription.Value%20like%20%60{searchText}%60%20or%20%23Custom.IRISRCPLongDescription.Value%20like%20%60{searchText}%60%20or%20%23Custom.IRISRCPAdditionalCodes.Value2%20like%20%60{searchText}%60%20or%20%23Custom.IRISRCMAdditionalCodes.Value2%20like%20%60{searchText}%60", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");
            try
            {
                List<WorkOrder> WorkOrders = _client.GetAsync<List<WorkOrder>>(request).Result;
                foreach (WorkOrder WO in WorkOrders)
                {
                    FinancialProject finProject = new FinancialProject
                    {
                        ProjectCode = WO.JobKey.Split('.')[1],
                        ProjectName = WO.Description,
                        FINCustomerCode = "",
                        CustomerName = "",
                        Details = WO.RCM.LongDescription == "" ? WO.RCP.LongDescription : WO.RCM.LongDescription

                    };

                    projects.Add(finProject);
                }
            }
            catch (Exception ex)
            {
                Logging.Write("FindFinancialProject", "An error has occured", string.Empty, ex.Message);
            }

            return projects;

        }

        private string GetSafeString(string value){
            return string.IsNullOrEmpty(value) ? "" : value;
        }

        public FinancialTimeCodes GetTimeRecordingCodes(string FINProjectCode)
        {
            Logging.Write("GetTimeRecordingCodes", "FINProjectCode: " + FINProjectCode, string.Empty);

            FinancialTimeCodes codes = new FinancialTimeCodes();
            if (string.IsNullOrEmpty(FINProjectCode.Trim()) ||  string.Empty.Equals(FINProjectCode))
            {
                Logging.Write("GetTimeRecordingCodes", "Error - no FINProjectCode supplied", string.Empty);
                return codes;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            RestRequest request = new RestRequest($"/{ConfigurationManager.AppSettings["DatascapeEnvironment"]}/Custom/IRISJobActivityLookup?Filter=%28Job.%23Custom.IRISRCPAdditionalCodes.Value2%20eq%20%60{FINProjectCode}%60%20or%20Job.%23Custom.IRISRCMAdditionalCodes.Value2%20eq%20%60{FINProjectCode}%60%29%20or%20%28Job.Number%20eq%20%60{FINProjectCode}%60%29", Method.Get);
            request.AddHeader("Authorization", "Basic SVJJU0FQSVVzZXI6elVJVk9Xc0k=");

            try
            {
                List<JobActivity> JobActivities = _client.GetAsync<List<JobActivity>>(request).Result;
                foreach (JobActivity Activity in JobActivities)
                {
                    FinancialTimeCode TimeCode = new FinancialTimeCode
                    {
                        TimeCodeNumber = Convert.ToInt64(Activity.Code.Split('.')[0]),
                        TimeCodeName = Activity.Name

                    };

                    codes.Add(TimeCode);
                }
            }
            catch (Exception ex)
            {
                Logging.Write("GetTimeRecordingCodes", "An error has occured", string.Empty, ex.Message);
            }


            Logging.Write("GetTimeRecordingCodes", "FINProjectCode: " + GetSafeString(FINProjectCode), "NoOfCodes:" + codes.Count);
            return codes;
        }

        public FinancialProjectOrganisations GetProjectOrganisations()
        {
            Logging.Write("GetProjectOrganisations", string.Empty, string.Empty);
            FinancialProjectOrganisations orgs = new FinancialProjectOrganisations();
            return orgs;
        }
        
        public void UpdateOfficerResponsible(OfficerResponsibleDetailsCollection officerResponsibleDetailsCollection)
        {
            Logging.Write("UpdateOfficerResponsible", string.Empty, string.Empty);            
        }

        private IRISServiceFaultContract IRISServiceFaultContract(string errorMessage, int faultCode = 1)
        {
            IRISServiceFaultContract serviceFaultContract = new IRISServiceFaultContract();
            serviceFaultContract.FaultCode = faultCode;            
            serviceFaultContract.FaultMessage = errorMessage;
            return serviceFaultContract;
        }

    }
}
