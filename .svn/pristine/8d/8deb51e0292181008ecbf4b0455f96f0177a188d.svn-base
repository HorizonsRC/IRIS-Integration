
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Reflection;

namespace HRC.Common.Exceptions
{
    public class ExceptionInformation
    {
        #region Variables

        Exception exception;
        DateTime m_DateTime = DateTime.Now;
        List<StackItem> m_StackTrace;
        string m_Version = Application.ProductVersion;

        #endregion Variables

        #region Constructors

        public ExceptionInformation(Exception exception)
        {
            this.exception = exception;
        }

        ExceptionInformation()
        {
        }

        #endregion Constructors

        #region Properties

        public string Version
        {
            get
            {
                return m_Version;
            }
            set { }
        }

        public DateTime DateTime
        {
            get
            {
                return m_DateTime;
            }
            set { }
        }

        public string Message
        {
            get
            {
                return exception.Message;
            }
            set { }
        }

        public string ErrorType
        {
            get
            {
                return exception.GetType().ToString();
            }
        }

        public List<StackItem> StackTrace
        {
            get
            {
                if (m_StackTrace == null)
                {
                    m_StackTrace = StackItem.FromStackTrace(exception.StackTrace);
                }
                return m_StackTrace;
            }
            set { }
        }

        public string TargetSite
        {
            get
            {
                return exception.TargetSite == null ? null : exception.TargetSite.ToString();
            }
            set { }
        }

        public string ProductName
        {
            get
            {
                return Application.ProductName;
            }
            set { }
        }

        public string MachineName
        {
            get
            {
                return Environment.MachineName;
            }
            set { }
        }

        public string LogonName
        {
            get
            {
                return Environment.UserDomainName + "\\" + Environment.UserName;
            }
            set { }
        }

        public ExceptionInformation InnerException
        {
            get
            {
                return exception.InnerException == null ? null : new ExceptionInformation(exception.InnerException);
            }
            set { }
        }

        #endregion Properties

        #region Methods

        public void Export(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stream, this);
        }

        public static string GetExceptionStack(Exception ex)
        {
            ExceptionInformation info = new ExceptionInformation(ex);
            return info.Export();
        }

        private string Export()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            StringWriter textWriter = new StringWriter();
            serializer.Serialize(textWriter, this);
            return textWriter.ToString();
        }

        #endregion Methods
    }
}
