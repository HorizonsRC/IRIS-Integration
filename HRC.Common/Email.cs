
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;
using HRC.Common.Exceptions;
using HRC.Common.Configuration;
using HRC.Common.Validators;

namespace HRC.Common
{
    public class Email
    {
        #region Fields
        
        string m_Subject;
        string m_Body;
        bool m_Html;
        List<MailAddress> m_To = new List<MailAddress>();
        MailAddress m_From = new MailAddress(CommonConfig.Instance.MailFromEmail);
        List<MailAddress> m_CC = new List<MailAddress>();
        List<MailAddress> m_BCC = new List<MailAddress>();
        List<Attachment> m_Attachments = new List<Attachment>();
        MailAddress m_ReplyTo;
        Encoding m_BodyEncoding = Encoding.UTF8;

        #endregion

        #region Properties

        public Encoding BodyEncoding
        {
            get { return m_BodyEncoding; }
        }

        public MailAddress ReplyTo
        {
            get { return m_ReplyTo; }
        }

        public string Subject
        {
            get
            {
                return m_Subject;
            }
        }

        public string Body
        {
            get
            {
                return m_Body;
            }
        }

        public ReadOnlyCollection<MailAddress> To
        {
            get
            {
                return AsReadOnly(m_To);
            }
        }

        public MailAddress From
        {
            get
            {
                return m_From;
            }
        }

        public ReadOnlyCollection<MailAddress> CC
        {
            get
            {
                return AsReadOnly(m_CC);
            }
        }

        public ReadOnlyCollection<MailAddress> BCC
        {
            get
            {
                return AsReadOnly(m_BCC);
            }
        }

        public ReadOnlyCollection<Attachment> Attachments
        {
            get
            {
                return AsReadOnly(m_Attachments);
            }
        }

        public bool Html
        {
            get
            {
                return m_Html;
            }
        }

        #endregion

        #region Methods

        public Email SetReplyTo(string replyTo)
        {
            return SetReplyTo(new MailAddress(replyTo));
        }

        public Email SetReplyTo(MailAddress replyTo)
        {
            m_ReplyTo = replyTo;
            return this;
        }

        public Email SetSubject(string subject)
        {
            m_Subject = subject;
            return this;
        }

        public Email SetBody(string body, params object[] args)
        {
            return SetBody(string.Format(body, args));
        }

        public Email SetBody(string body)
        {
            m_Body = body;
            return this;
        }

        public Email SetFrom(string address, string displayName)
        {
            return SetFrom(new MailAddress(address, displayName));
        }

        public Email SetFrom(MailAddress from)
        {
            m_From = from;
            return this;
        }

        public Email SetBodyEncoding(Encoding encoding)
        {
            m_BodyEncoding = encoding;
            return this;
        }

        public Email SetHtml(bool html)
        {
            m_Html = html;
            return this;
        }

        public Email AddAttachment(string file)
        {
            return AddAttachment(new Attachment(file));
        }

        public Email AddAttachment(Stream stream, string name)
        {
            return AddAttachment(new Attachment(stream, name));
        }

        public Email AddAttachment(Attachment attachment)
        {
            m_Attachments.Add(attachment);
            return this;
        }

        Email AddAddresses(IEnumerable<MailAddress> toAdd, List<MailAddress> target)
        {
            foreach (MailAddress address in toAdd)
            {
                target.Add(address);
            }
            return this;
        }

        public Email AddBCC(string bcc)
        {
            return AddBCC(new MailAddress(bcc));
        }

        public Email AddBCC(MailAddress bcc)
        {
            return AddBCC(new MailAddress[] { bcc });
        }

        public Email AddBCC(IEnumerable<MailAddress> bcc)
        {
            return AddAddresses(bcc, m_BCC);
        }

        public Email AddCC(string cc)
        {
            return AddCC(new MailAddress(cc));
        }

        public Email AddCC(MailAddress cc)
        {
            return AddCC(new MailAddress[] { cc });
        }

        public Email AddCC(IEnumerable<MailAddress> cc)
        {
            return AddAddresses(cc, m_CC);
        }

        static ReadOnlyCollection<T> AsReadOnly<T>(List<T> list)
        {
            return (list ?? new List<T>()).AsReadOnly();
        }

        public Email AddTo(string to)
        {
            if (to.Contains(";"))
            {
                // Handles simple splitting for multiple email addresses
                List<MailAddress> ma = new List<MailAddress>();

                foreach (string splitTo in to.Split(';'))
                {
                    ma.Add(new MailAddress(splitTo));
                }
                return AddTo(ma);
            }
            else
            {
                return AddTo(new MailAddress(to));
            }
        }

        public Email AddTo(MailAddress to)
        {
            return AddTo(new MailAddress[] { to });
        }

        public Email AddTo(IEnumerable<MailAddress> to)
        {
            return AddAddresses(to, m_To);
        }

        public void Send()
        {
            SendUtilities.SendEMail(this, false, false);
        }

        public void SendException()
        {            
            if (this.To.Count == 0)
            {
                this.AddTo(CommonConfig.Instance.ExceptionEmailTo);
            }
            SendUtilities.SendEMail(this, false, true);
        }

        public void SendAsync()
        {
            SendUtilities.SendEMail(this, true, false);
        }

        public static Email CreateExceptionEmail(Exception e)
        {
            if (e != null)
            {
                return CreateExceptionEmail(new ExceptionInformation(e));
            }
            return null;
        }

        public static Email CreateExceptionEmail(ExceptionInformation info)
        {
            Email email = new Email();

            email.SetFrom(new MailAddress(CommonConfig.Instance.MailFromEmail, string.Format("{0} Exceptions", info.ProductName)));
            email.SetBody(string.Format("Program: {0}{1}Error: {2}{1}DateTime: {3}{1}Machine: {4}{1}Version: {5}{1}Exception: {6}",
                info.ProductName, Environment.NewLine, info.Message, info.DateTime, Environment.MachineName, info.Version, info.ErrorType));
            email.AddTo(CommonConfig.Instance.ExceptionEmailTo);
            email.SetSubject("[Exception] Error in " + info.ProductName);

            MemoryStream stream = new MemoryStream();
            info.Export(stream);
            stream.Position = 0;
            email.AddAttachment(stream, string.Format("Error_{0}.xml", DateTime.Now.ToString("yyyyMMdd-HHmmss")));

            return email;
        }

        #endregion
    }
}
