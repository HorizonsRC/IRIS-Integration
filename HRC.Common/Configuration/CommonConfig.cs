using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ComponentModel;

namespace HRC.Common.Configuration
{
    public class CommonConfig : ConfigurationSectionEx 
    {
        #region Fields

        private static CommonConfig m_Instance;

        public const string ConfigFilename = "HRC.Common.Config";        

        const string MailServerName = "mailServer";        
        const string MailServerPortName = "mailServerPort";
        const string ExceptionEmailToName = "exceptionEmailTo";
        const string MailFromEmailName = "mailFromEmail";
        const string SmsGatewayName = "smsGateway";

        const string SvnRepositoryUriName = "svnRepositoryUri";                

        #endregion

        #region Constructors

        private CommonConfig()
        {
        }

        #endregion

        #region Properties
        
        [ConfigurationProperty(SvnRepositoryUriName, DefaultValue = "https://devsvnapp01.local/svn/Dev", IsRequired = false)]        
        public string SvnRepositoryUri
        {
            get
            {
                return (string)this[SvnRepositoryUriName];
            }
            set
            {
                this[SvnRepositoryUriName] = value;
            }
        }
        
        [ConfigurationProperty(MailServerName, DefaultValue = "mail.horizons.govt.nz", IsRequired = false)]
        public string MailServer
        {
            get
            {
                return (string)this[MailServerName];
            }
            set
            {
                this[MailServerName] = value;
            }
        }

        [ConfigurationProperty(MailServerPortName, DefaultValue = "25", IsRequired = false)]
        public int MailServerPort
        {
            get
            {
                return (int)this[MailServerPortName];
            }
            set
            {
                this[MailServerPortName] = value;
            }
        }

        [ConfigurationProperty(MailFromEmailName, DefaultValue = "Reports@horizons.govt.nz", IsRequired = false)]
        public string MailFromEmail
        {
            get
            {
                return (string)this[MailFromEmailName];
            }
            set
            {
                this[MailFromEmailName] = value;
            }
        }

        [ConfigurationProperty(SmsGatewayName, DefaultValue = "pcsms.co.nz", IsRequired = false)]
        public string SmsGateway
        {
            get
            {
                return (string)this[SmsGatewayName];
            }
            set
            {
                this[SmsGatewayName] = value;
            }
        }

        [ConfigurationProperty(ExceptionEmailToName, DefaultValue = "Dave.Mitchell@horizons.govt.nz", IsRequired = false)]
        public string ExceptionEmailTo
        {
            get
            {
                return (string)this[ExceptionEmailToName];
            }
            set
            {
                this[ExceptionEmailToName] = value;
            }
        }

        public static CommonConfig Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    //m_Instance = new CommonConfig();

                    bool success;
                    m_Instance = OpenConfig(new CommonConfig(), "", true, false, out success);
                    if (!success)
                    {
                        m_Instance = new CommonConfig();
                    }
                }
                return m_Instance;
            }
        }
        
        protected override string OverrideConfigFilename
        {
            get
            {
                return FindConfigFile(ConfigFilename);
            }
        }        

        #endregion Properties

        #region Methods

        #endregion Methods


    }
}
