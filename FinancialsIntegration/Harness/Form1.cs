﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

using System.ServiceModel;
using FIN = Harness.FinancialsService;
using HRC.Common;
using Ozone = Harness.OzoneServiceClient;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Harness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FIN.FinancialsServiceClient client = new FIN.FinancialsServiceClient();            
            try
            {
                DateTime date = DateTime.Now.AddDays(-1000);
                string accountName = "SERVERS\\LNhari";
                FIN.TimeRecordingData data = client.GetTimeRecordingData(date, accountName);
            }
            catch (FaultException<FIN.IRISServiceFaultContract> ex)
            {
                MessageBox.Show(ex.Detail.FaultMessage);
            }
            catch (FaultException ex)
            {
                MessageBox.Show(ex.Message);
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FIN.FinancialsServiceClient client = new FIN.FinancialsServiceClient();
            try
            {
                DateTime date = DateTime.Now.AddDays(-10);
                string query = "";                
                FIN.Parameters parameters = new FIN.Parameters();
                parameters.Add("Key", "Value");
                string result = client.GetIRISData(query, parameters);
            }
            catch (FaultException<FIN.IRISServiceFaultContract> ex)
            {
                MessageBox.Show(ex.Detail.FaultMessage);
            }
            catch (FaultException ex)
            {
                MessageBox.Show(ex.Message);
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
        }

        private void btnSearchSelectData_Click(object sender, EventArgs e)
        {
            txtResults.Text = "";
            Ozone.OzoneSoapClient client = new Ozone.OzoneSoapClient();
            try
            {
                
                string token = null;
                //token = string.IsNullOrEmpty(token) ? client.AuthenticateWindows(ConfigurationManager.AppSettings["OzoneWebServiceURL"]) : token;
                token = string.IsNullOrEmpty(token) ? client.Authenticate(ConfigurationManager.AppSettings["OzoneWebServiceURL"], ConfigurationManager.AppSettings["IRISOzoneUser"], ConfigurationManager.AppSettings["IRISOzonePassword"]) : token;
                string busObject = txtBusObj.Text;
                string dict = txtDict.Text;
                string statement = txtStatement.Text;
                string format = "ORIGEN";

                string returnXmlSSD = client.SearchSelectData(token, busObject, dict, statement, format, true);
                txtResults.Text = returnXmlSSD + "\r\n" + token;

               

            }
            catch(Exception ex)
            {
                txtResults.Text = ex.Message;
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
        }

        private void btnGetTimeRecordingCodes_Click(object sender, EventArgs e)
        {
            string FinProjectCode = txtFinProjectCode.Text;

             txtResults.Text = "";
            Ozone.OzoneSoapClient client = new Ozone.OzoneSoapClient();
            try
            {                
                string token = null;
                //token = string.IsNullOrEmpty(token) ? client.AuthenticateWindows(ConfigurationManager.AppSettings["OzoneWebServiceURL"]) : token;
                token = string.IsNullOrEmpty(token) ? client.Authenticate(ConfigurationManager.AppSettings["OzoneWebServiceURL"], ConfigurationManager.AppSettings["IRISOzoneUser"], ConfigurationManager.AppSettings["IRISOzonePassword"]) : token;

                string busObject = "JCS.JOB.SUB";
                string dict = "JCS.SJ.ID|JCS.SJ.DESC";
                string statement = string.Format("JCS.SJ.ID1 LIKE \"{0}\" AND JCS.SJ.CLOSED = \"\"", FinProjectCode);
                                
                string format = "ORIGEN";

                string returnXmlSSD = client.SearchSelectData(token, busObject, dict, statement, format, true);
                txtResults.Text = returnXmlSSD;
            }
            catch(Exception ex)
            {
                txtResults.Text = ex.Message;
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
        }

        private void btnFindFinancialProject_Click(object sender, EventArgs e)
        {
            string searchText = txtSearchFinancialProject.Text;

            txtResults.Text = "";
            Ozone.OzoneSoapClient client = new Ozone.OzoneSoapClient();

            /***
             XmlSerializer xsSubmit = new XmlSerializer(client.GetType());

            StringWriter sww = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, client);

            var xml = sww.ToString(); // Your xml


            txtResults.Text = xml;
             * */

            try
            {
                string token = null;
                //token = string.IsNullOrEmpty(token) ? client.AuthenticateWindows(ConfigurationManager.AppSettings["OzoneWebServiceURL"]) : token;
                token = string.IsNullOrEmpty(token) ? client.Authenticate(ConfigurationManager.AppSettings["OzoneWebServiceURL"], ConfigurationManager.AppSettings["IRISOzoneUser"], ConfigurationManager.AppSettings["IRISOzonePassword"]) : token;

                string busObject = "JCS.JOB";
                string dict = "JCS.JOB.ID|JCS.JOB.SDESC|JCS.JOB.DESC";
                string statement = string.Format("JCS.JOB.ID LIKE \"...{0}...\" OR JCS.JOB.SDESC LIKE \"...{0}...\"  OR JCS.JOB.DESC LIKE \"...{0}...\"", searchText);
                string format = "ORIGEN";

                string returnXmlSSD = client.SearchSelectData(token, busObject, dict, statement, format, true);
                txtResults.Text = returnXmlSSD;

                string returnedData = "";
                XElement xmlData = XElement.Parse(returnXmlSSD);
                XElement myData = xmlData.Element("data");
                foreach (XElement item in myData.Elements())
                {
                    XElement id = item.Element("id");
                    returnedData += string.Format("\r\nid: {0} ", id.Value.ToString());
                    XElement itemGroup = item.Element("group");
                    foreach (XElement dictData in itemGroup.Elements("dictionary")){
                        returnedData += "\r\n - " + dictData.Attribute("id").Value.ToString() + ": " + dictData.Element("value").Value.ToString();
                    }
                }
                txtResults.Text = returnedData;
            }
            catch (Exception ex)
            {
                txtResults.Text = ex.Message;
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
        }

        private void btnCopyJob_Click(object sender, EventArgs e)
        {
            Ozone.OzoneSoapClient client = new Ozone.OzoneSoapClient();
            try
            {
                txtResults.Text = DateTime.Now.ToString() + System.Environment.NewLine;
                string jobId = txtJOBID.Text;
                string token = null;
                //token = string.IsNullOrEmpty(token) ? client.AuthenticateWindows(ConfigurationManager.AppSettings["OzoneWebServiceURL"]) : token;
                token = string.IsNullOrEmpty(token) ? client.Authenticate(ConfigurationManager.AppSettings["OzoneWebServiceURL"], ConfigurationManager.AppSettings["IRISOzoneUser"], ConfigurationManager.AppSettings["IRISOzonePassword"]) : token;

                string returnXmlSSD = client.CallFunction(token, "JCS.COPY.JOB", "CN0001|" + jobId + "|TEST JOB NAME GOES HERE|DESCRIPTION GOES HERE|");
                if (string.IsNullOrEmpty(returnXmlSSD)) returnXmlSSD="Success";
                txtResults.Text = txtResults.Text + returnXmlSSD + System.Environment.NewLine;
                txtResults.Text = txtResults.Text + DateTime.Now.ToString() + System.Environment.NewLine;
            }
            catch (Exception ex)
            {
                txtResults.Text = ex.Message;
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
        }
        

    }
}
