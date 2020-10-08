using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using HRC.Common;
using HRC.Common.Validators;
using HRC.Contacts.BL;
using HRC.PowerBuilderContacts.BL;
using HRC.OzoneContacts.BL;

using ContactsIntegration.ContactsService;
using ContactsIntegration.BL;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Collections;
using System.DirectoryServices;
using HRC.Common.Exceptions;
using HRC.Framework.BL;

namespace Harness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //testing values
            //string dave = "40133";
            //string chris = "30598";
            //string POBox = "94";
            //string phoneNumber = "54701";
            string org = "10";
                        
            string contactId = string.IsNullOrEmpty(textBox1.Text)
                ? org
                : textBox1.Text;

            long Id;

            if (long.TryParse(contactId, out Id))
            {
                SaveContact(Id);
            }
        }

        private void SaveContact(long Id)
        {
            ContactDetails contactDetails = null;
            ContactsServiceClient client = new ContactsServiceClient();
            try
            {
                contactDetails = client.GetContactDetails(Id);
            }
            catch (FaultException<IRISServiceFaultContract> ex)
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

            if (contactDetails != null)
            {

                XmlSerializer xsSubmit = new XmlSerializer(contactDetails.GetType());

                StringWriter sww = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sww);
                xsSubmit.Serialize(writer, contactDetails);
               
                var xml = sww.ToString(); // Your xml
                txtXml.Text = xml;
                //save the contact to PowerBuilder
                //PBContact contact = Map.IRISContactToPowerBuilderContact(contactDetails);                
                //contact.Save(false);

                //save the contact to our Ozone queue
                //OzoneContact contact = MapIRISContactToOzoneContact(contactDetails);
                //contact.SavePostXml();
            }
        }
        
        private ContactsCollection GetChangedContacts()
        {
            ContactsServiceClient client = new ContactsServiceClient();
            ContactsCollection contactInformation = null;
            try
            {
                contactInformation = client.GetChangedContacts(DateTime.Now.AddDays(-1));
            }
            catch (FaultException<IRISServiceFaultContract> ex)
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

            XmlSerializer xsSubmit = new XmlSerializer(contactInformation.GetType());

            StringWriter sww = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, contactInformation);

            var xml = sww.ToString(); // Your xml
            txtXml.Text = xml;
            return contactInformation;            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetChangedContacts();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtFeedback.Text = "";
            Application.UseWaitCursor = true;

            ProcessContactsXml(txtXml.Text);
            Application.UseWaitCursor = false;
            MessageBox.Show("Complete");
           // bulkLoadContactsTest(txtXml.Text);
        }
         
        private void bulkLoadContactsTest(string xmlContacts){

            ContactDetails contactDetails=new ContactDetails();
            //check structure of xml
            
            //convert into array or collection of contactDetails objects

            List<ContactDetails> contactDetailsCollection = new List<ContactDetails>();
            
            XmlSerializer xsSubmit = new XmlSerializer(contactDetails.GetType());

            //StringWriter sww = new StringWriter();
            //XmlWriter writer = XmlWriter.Create(sww);
            //xsSubmit.Serialize(writer, contactDetails);

            //var xml = sww.ToString(); // Your xml
            //txtXml.Text = xml;

            StreamReader reader = new StreamReader(GenerateStreamFromString(xmlContacts));
            //StreamReader reader = new StreamReader("text.xml");
            contactDetails = (ContactDetails)xsSubmit.Deserialize(reader);
            reader.Close();
            


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

        private string ProcessContactsXml(string xml)
        {
            try
            {
                List<ContactDetails> contactDetailsCollection = new List<ContactDetails>();
                ContactDetails[] contactDetailArray;
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xml);
                XmlSerializer xsSubmit = new XmlSerializer((new ContactDetails()).GetType());
                StringBuilder sbOutput = new StringBuilder();



                //xd.DocumentElement.SelectSingleNode("ContactDetailsCollection").SelectNodes("ContactDetails")
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
                    //contactXml.Replace("", "");


                    StreamReader reader = new StreamReader(GenerateStreamFromString(contactXml));
                    //StreamReader reader = new StreamReader("text.xml");
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = (ContactDetails)xsSubmit.Deserialize(reader);
                    contactDetailsCollection.Add(contactDetails);

                    reader.Close();

                    //sbOutput.AppendLine(contactDetails.ContactType);
                    if (contactDetails.ContactType.ToUpper() == "PERSON")
                    {
                        foreach (ContactName name in contactDetails.ContactNames)
                        {
                            sbOutput.AppendLine(contactDetails.ContactType + ": " + name.ContactPersonName.PersonFirstName);
                        }
                    }
                    else
                    {
                        foreach (ContactName name in contactDetails.ContactNames)
                        {
                            sbOutput.AppendLine(contactDetails.ContactType + ": " + name.ContactOrganisationName.OrganisationName);
                        }

                    }

                    txtFeedback.Text = sbOutput.ToString();
                }
                sbOutput.AppendLine();
                sbOutput.AppendLine("No of Contact:= " + contactDetailsCollection.Count().ToString());
                txtFeedback.Text = sbOutput.ToString();

                ContactsServiceClient client = new ContactsServiceClient();
                BulkLoadContactsOutcome bulkLoadOutcome = client.BulkLoadContacts(contactDetailsCollection.ToArray());

                //XmlSerializer xsSubmit3 = new XmlSerializer(contactDetailsCollection.ToArray().GetType());
                //StringWriter sww3 = new StringWriter();
                //XmlWriter writer3 = XmlWriter.Create(sww3);
                //xsSubmit3.Serialize(writer3, contactDetailsCollection.ToArray());
                //var xmlcontactDetailsCollection = sww3.ToString(); // Your xml

                sbOutput.AppendLine();
                sbOutput.AppendLine("Success: " + bulkLoadOutcome.Success);
                sbOutput.AppendLine("ErrorMessage: " + bulkLoadOutcome.ErrorMessage);
                sbOutput.AppendLine("ContactInErrorCollection.Length: " + bulkLoadOutcome.ContactInErrorCollection.Length);
                sbOutput.AppendLine();
                sbOutput.AppendLine();

                int i = 1;
                foreach (ContactInError contactInError in bulkLoadOutcome.ContactInErrorCollection)
                {
                    sbOutput.AppendLine();
                    sbOutput.AppendLine("ContactInErrorCollection #: " + i);
                    sbOutput.AppendLine("ErrorMessage: " + contactInError.ErrorMessage);
                    sbOutput.AppendLine("---ContactDetails ---");
                    XmlSerializer xsSubmit2 = new XmlSerializer(contactInError.ContactDetails.GetType());
                    StringWriter sww = new StringWriter();
                    XmlWriter writer = XmlWriter.Create(sww);
                    xsSubmit2.Serialize(writer, contactInError.ContactDetails);
                    var xmlContactInErrorCollection = sww.ToString(); // Your xml
                    sbOutput.AppendLine(xmlContactInErrorCollection);
                    sbOutput.AppendLine("--------------");
                    sbOutput.AppendLine();
                    i++;
                }

                //XmlSerializer xsSubmit4 = new XmlSerializer(bulkLoadOutcome.GetType());
                //StringWriter sww4 = new StringWriter();
                //XmlWriter writer4 = XmlWriter.Create(sww4);
                //xsSubmit4.Serialize(writer4, bulkLoadOutcome);
                //var xmlContactsInError = sww4.ToString(); // Your xml
                

                txtFeedback.Text = sbOutput.ToString();
            }
            catch (Exception ex)
            {
                txtFeedback.Text = ex.Message +  " --------- " + ex.InnerException;

            }
            return "";
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void btnCreateOU_Click(object sender, EventArgs e)
        {
            BindToADAM();
        }

       
        private void CreateOU( string strOU,  string strOUDescription){
            DirectoryEntry objADAM;  // Binding object.
            DirectoryEntry objOU;    // Organizational unit.
            //string strDescription;   // Description of OU.
            //string strOU;            // Organiztional unit.
            string strPath;          // Binding path.


            //TODO: get binding string from configurations, construct the binding string
            strPath = "LDAP://devapserver/DC=HORIZONS,DC=GOVT,DC=NZ";

            // Get AD LDS object.
            try
            {
                objADAM = new DirectoryEntry(strPath);
                objADAM.RefreshCache();
            }
            catch (Exception ex)
            {
                Logging.Write("DEV", "Get AD LDS Object", string.Empty, ExceptionInformation.GetExceptionStack(ex));
                return;
            }

            // Specify Organizational Unit.
            strOU = "OU=TestOU";
            strOUDescription = "AD LDS Test Organizational Unit";
          

            // Create Organizational Unit.
            try
            {
                objOU = objADAM.Children.Add(strOU,
                    "OrganizationalUnit");
                objOU.Properties["description"].Add(strOUDescription);
                objOU.CommitChanges();
            }
            catch (Exception ex)
            {
                Logging.Write("DEV", "Get AD LDS Object", string.Empty, ExceptionInformation.GetExceptionStack(ex));
                return;
            }


        }

        /// <summary>
        /// Create a single Contact.
        /// </summary>

        private bool CreateContact(
            DirectoryEntry objADAMPath,
            string strCN,
            string strSN,
            string strTitle,
            string strTelephoneNumber,
            string strGivenName,
            string strMail,
            string strManager)
        {
            DirectoryEntry objNewContact;  // New Contact to create.

            // Create new contact and set attributes.
            try
            {
                objNewContact = objADAMPath.Children.Add(
                    strCN, "user");
                if (strSN != "" || strSN != String.Empty)
                    objNewContact.Properties["sn"].Add(strSN);
                if (strTitle != "" || strTitle != String.Empty)
                    objNewContact.Properties["title"].Add(strTitle);
                if (strTelephoneNumber != "" ||
                    strTelephoneNumber != String.Empty)
                    objNewContact.Properties[
                        "telephoneNumber"].Add(strTelephoneNumber);
                if (strGivenName != "" || strGivenName != String.Empty)
                    objNewContact.Properties[
                        "givenName"].Add(strGivenName);
                if (strMail != "" || strMail != String.Empty)
                    objNewContact.Properties["mail"].Add(strMail);
                if (strManager != "" || strManager != String.Empty)
                    objNewContact.Properties["manager"].Add(strManager);
                objNewContact.CommitChanges();
            }
            catch (Exception e)
            {
                txtFeedback.Text= string.Format("Error:   {0}", e.ToString());
                return false;
            }

            txtFeedback.Text = "Success:  Contact created.";
            return true;
        }


        private void BindToADAM()
        {

            StringBuilder sbOutput = new StringBuilder();
            DirectoryEntry objADAM;  // Directory object.
            string strObject="";        // DN of object to bind to.
            string strPath;          // Bind path.
            string strPort;          // Optional TCP port.
            string strServer;        // DNS Name of the computer with the
            // AD LDS installation

            // In this example, the AD LDS installation is on a local computer
            strServer = "devapserver";

            // TCP Port to use
            strPort = "50000";

            // Distinguished name of the object.
            //strObject = "o=Microsoft,c=US";
            strObject = "DC=HORIZONS,DC=GOVT,DC=NZ";
            // Construct the binding string.
            strPath = "LDAP://";
            if (strServer.Equals("") || strServer.Equals(String.Empty))
            {
                sbOutput.AppendLine("Error: Invalid hostname specified.");
                return;
            }
            else
            {
                strPath = String.Concat(strPath, strServer);
            }

            if (!strPort.Equals("") && !strPort.Equals(String.Empty))
            {
                strPath = String.Concat(strPath, ":", strPort);
            }

            if (!strObject.Equals("") && !strObject.Equals(String.Empty))
            {
                strPath = String.Concat(strPath, "/", strObject);
            }

            sbOutput.AppendLine(String.Format("Bind to: {0}", strPath));

            // Get the specified object.
            try
            {
                objADAM = new DirectoryEntry(strPath);
                objADAM.RefreshCache();
          
            }
            catch (Exception e)
            {
                sbOutput.AppendLine("Error:   Bind failed.");
                sbOutput.AppendLine(String.Format("         {0}.", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Output object attributes.
            sbOutput.AppendLine("Success: Bind succeeded.");
            sbOutput.AppendLine(String.Format("Name:    {0}", objADAM.Name));
            sbOutput.AppendLine(String.Format("Path:    {0}", objADAM.Path));

            // Create first Contact.
            // Manager not specified.
            if (!CreateContact(
                objADAM,
                "CN=Dev3 Testing",
                "Testing",
                "Office Manager",
                "+64(6)5550101",
                "Dev3",
                "dev3test@horizons.govt.nz",
                ""))
            {
                return;
            }


            txtFeedback.Text = sbOutput.ToString();
            return;
        }


        static string GetPartitionsDN()
        {
            // Bind to the RootDSE to get the configurationNamingContext property.
            DirectoryEntry RootDSE = new DirectoryEntry("LDAP://devldapserver");
            DirectoryEntry ConfigContainer = new DirectoryEntry("LDAP://devldapserver");

            // Search for an object that is of type crossRefContainer.
            DirectorySearcher ConfigSearcher = new DirectorySearcher(ConfigContainer);
            ConfigSearcher.Filter = "(&(objectClass=crossRefContainer))";
            ConfigSearcher.PropertiesToLoad.Add("distinguishedName");
            ConfigSearcher.SearchScope = SearchScope.OneLevel;

            SearchResult result = ConfigSearcher.FindOne();

            return result.Properties["distinguishedName"][0].ToString();
        }


        /// <summary>
        /// Extend AD LDS Schema with Contact Class
        ///  and Additional-Information Attribute.
        /// </summary>
        private void ExtendSchemaContact()
        {

            StringBuilder sbOutput = new StringBuilder();
            // Writeable attribute.
            const int DS_INSTANCETYPE_NC_IS_WRITEABLE = 4;

            DirectoryEntry objRoot;         // Root of AD LDS instance.
            DirectoryEntry objSchema;       // Schema partiton.
            string strSchemaNamingContext;  // Schema DN.

            // Get schema path.
            try
            {
                objRoot = new DirectoryEntry(
                    "LDAP://devapserver:50000/RootDSE");
                strSchemaNamingContext = objRoot.Properties[
                    "schemaNamingContext"].Value.ToString();
                objSchema = new DirectoryEntry(String.Concat(
                    "LDAP://devapserver:50000/", strSchemaNamingContext));

                sbOutput.AppendLine(String.Format("Schema path: {0}", objSchema.Path));
                sbOutput.AppendLine(String.Format(""));
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format("Error:   Schema bind failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Declarations for new attribute.
            // Attribute name.
            const string strAttributeName = "Additional-Information";
            // New attribute.
            DirectoryEntry objNewAttribute;
            // Attribute CN.
            string strCNAttributeName;

            // Create new attribute.
            try
            {
                strCNAttributeName = String.Concat("CN=",
                    strAttributeName);
                objNewAttribute = objSchema.Children.Add(
                    strCNAttributeName, "attributeSchema");
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format("Error:   Attribute create failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Set selected values for attribute.
            try
            {
                objNewAttribute.Properties[
                    "instanceType"].Add(DS_INSTANCETYPE_NC_IS_WRITEABLE);
                objNewAttribute.Properties[
                    "attributeID"].Add("1.2.840.113556.1.4.265");
                objNewAttribute.Properties[
                    "attributeSyntax"].Add("2.5.5.12");
                objNewAttribute.Properties[
                    "isSingleValued"].Add(true);
                objNewAttribute.Properties["rangeUpper"].Add(32768);
                objNewAttribute.Properties[
                    "showInAdvancedViewOnly"].Add(true);
                objNewAttribute.Properties[
                    "adminDisplayName"].Add(strAttributeName);
                objNewAttribute.Properties[
                    "adminDescription"].Add(strAttributeName);
                objNewAttribute.Properties["oMSyntax"].Add(64);
                objNewAttribute.Properties["searchFlags"].Add(0);
                objNewAttribute.Properties[
                    "lDAPDisplayName"].Add("notes");
                objNewAttribute.Properties["name"].Add(strAttributeName);
                objNewAttribute.Properties["systemOnly"].Add(false);
                objNewAttribute.Properties["systemFlags"].Add(16);
                objNewAttribute.Properties["objectCategory"].Add(
                    String.Concat("CN=Attribute-Schema,",
                    strSchemaNamingContext));
                objNewAttribute.CommitChanges();

                sbOutput.AppendLine(String.Format(
                    "Success: Created attributeSchema class object:"));
                sbOutput.AppendLine(String.Format("         {0}", objNewAttribute.Name));
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format(
                    "Error:   Setting attribute properties failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Update schema cache.
            try
            {
                sbOutput.AppendLine(String.Format("         Updating schema cache."));
                objRoot.Properties["schemaUpdateNow"].Value = 1;
                objRoot.CommitChanges();
                sbOutput.AppendLine(String.Format(""));
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format(
                    "Error:   Updating schema cache failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Write declarations for new class.
            const string strClassName = "Contact";  // Class name.
            DirectoryEntry objNewClass;             // New class.
            string strCNClassName;                  // Class CN.

            // Create new class.
            try
            {
                strCNClassName = String.Concat("CN=", strClassName);
                objNewClass = objSchema.Children.Add(
                    strCNClassName, "classSchema");
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format("Error:   Class create failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Set selected values for class.
            try
            {
                objNewClass.Properties[
                    "instanceType"].Add(DS_INSTANCETYPE_NC_IS_WRITEABLE);
                objNewClass.Properties[
                    "subClassOf"].Add("organizationalPerson");
                objNewClass.Properties[
                    "governsID"].Add("1.2.840.113556.1.5.15");
                objNewClass.Properties["rDNAttID"].Add("cn");
                objNewClass.Properties[
                    "showInAdvancedViewOnly"].Add(true);
                objNewClass.Properties[
                    "adminDisplayName"].Add(strClassName);
                objNewClass.Properties[
                    "adminDescription"].Add(strClassName);
                objNewClass.Properties["objectClassCategory"].Add(1);
                objNewClass.Properties[
                    "lDAPDisplayName"].Add(strClassName);
                objNewClass.Properties["name"].Add(strClassName);
                objNewClass.Properties["systemOnly"].Add(false);
                objNewClass.Properties["systemPossSuperiors"].AddRange(
                    new object[] {"organizationalUnit", "domainDNS"});
                objNewClass.Properties["systemMayContain"].Add("notes");
                objNewClass.Properties["systemMustContain"].Add("cn");
                objNewClass.Properties[
                    "defaultSecurityDescriptor"].Add(String.Concat(
                    "D:(A;;RPWPCRCCDCLCLORCWOWDSDDTSW;;;DA)",
                    "(A;;RPWPCRCCDCLCLORCWOWDSDDTSW;;;SY)",
                    "(A;;RPLCLORC;;;AU)"));
                objNewClass.Properties["systemFlags"].Add(16);
                objNewClass.Properties["defaultHidingValue"].Add(false);
                objNewClass.Properties["objectCategory"].Add(
                    String.Concat("CN=Class-Schema,",
                    strSchemaNamingContext));
                objNewClass.Properties["defaultObjectCategory"].Add(
                    String.Concat("CN=Person,",
                    strSchemaNamingContext));
                objNewClass.CommitChanges();

                sbOutput.AppendLine(String.Format(
                    "Success: Created classSchema class object:"));
                sbOutput.AppendLine(String.Format("         {0}", objNewClass.Name));
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format(
                    "Error:   Setting class properties failed."));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }

            // Update schema cache.
            try
            {
                sbOutput.AppendLine(String.Format("         Updating schema cache."));
                objRoot.Properties["schemaUpdateNow"].Value = 1;
                objRoot.CommitChanges();
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(String.Format(
                    "Error:   Updating schema cache failed.",""));
                sbOutput.AppendLine(String.Format("         {0}", e.Message));
                txtFeedback.Text = sbOutput.ToString();
                return;
            }
            
            return;
        }
    



        /********************************************************************

    CreateApplicationPartitionCS()

    Description: Creates an application directory partition.

    Parameters:

    DCADsPath - Contains the ADsPath of the partition. This must also 
    contain the DNS name of the domain controller that the partition  
    will be created on. For example, the ADsPath 
    "LDAP://DC01.fabrikam.com/DC=test,DC=com" would cause the 
    partition to be created on DC01.fabrikam.com. The distinguished
    name of the partition will be 
    "<pwszPartitionPath>,DC=test,DC=com".

    Username - Contains the user name to be used for authentication.

    Password - Contains the password to be used for authentication.

    PartitionPath - Contains the relative distinguished name of the 
    partition. This must be in the form of "DC=dynamicdata".

    Description - Contains a string that will be used for the 
    description property for the domainDNS object.

*******************************************************************/

        static void CreateApplicationPartitionCS(string DCADsPath,
            string Username,
            string Password,
            string PartitionPath,
            string Description)
        {
            /* 
            Bind to the specified domain controller. The path must be in the
            form "LDAP://<server DNS name>/<partition path>", in most cases, 
            the <partition path> will be an invalid path, so 
            AuthenticationTypes.FastBind is used to enable the bind to 
            succeed even if the path is invalid. 
            AuthenticationTypes.Delegation is used to allow the LDAP 
            provider to use the credentials to contact the Domain Naming
            FSMO role holder to create or modify the crossRef object.
            */
            DirectoryEntry parent = new DirectoryEntry(DCADsPath,
                Username,
                Password,
                AuthenticationTypes.Secure |
                AuthenticationTypes.FastBind |
                AuthenticationTypes.Delegation);

            // Create the domainDNS object.
            DirectoryEntry domainDNS = parent.Children.Add(PartitionPath,
                                                           "domainDNS");

            // Set the instanceType property.
            domainDNS.Properties["instanceType"].Value = 5;

            // Set the description property.
            domainDNS.Properties["description"].Value = Description;

            // Commit the new object to the server.
            domainDNS.CommitChanges();
        }

        private void btnExtend_Click(object sender, EventArgs e)
        {
            ExtendSchemaContact();
        }


    }
}
