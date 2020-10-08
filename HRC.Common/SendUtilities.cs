using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Net.Sockets;
using System.Net.Security;
using HRC.Common.Configuration;
using HRC.Common.Validators;
using HRC.Common.Security;

namespace HRC.Common
{
    public class SendUtilities
    {
        private static void AddAllAddresses(MailAddressCollection collection, IEnumerable<MailAddress> addresses)
        {            
            if (addresses != null)
            {
                foreach (MailAddress address in addresses)
                {
                    collection.Add(address);
                }
            }
        }

        private static List<Attachment> AddAllAttachments(IEnumerable<string> fileAttachments)
        {
            List<Attachment> attachments = new List<Attachment>();
            if (fileAttachments != null)
            {
                foreach (string attachment in fileAttachments)
                {
                    if (attachment != "")
                    {
                        attachments.Add(new Attachment(attachment));
                    }
                }
            }
            return attachments;
        }

		private static MailMessage CreateMessage(string from, Encoding bodyEncoding)
		{
			MailMessage message = new MailMessage();
            message.BodyEncoding = bodyEncoding;
			message.From = new MailAddress(from);
			return message;
		}

        private static void SendEMail(MailMessage message, bool async, bool emailException)
		{
            ValidateAddressses(message.To, emailException);
            ValidateAddressses(message.CC, emailException);
            ValidateAddressses(message.Bcc, emailException);

            if (message.To.Count == 0 && message.CC.Count == 0 && message.Bcc.Count == 0)
            {
                if (!emailException)
                {
                    new Email()
                        .SetSubject(string.Format("[WARNING] skipping email ({0}) as it has no recipients", message.Subject))
                        .SendException();
                }
                return;
            }

            using (SmtpClient mailServer = new SmtpClient(CommonConfig.Instance.MailServer, CommonConfig.Instance.MailServerPort))
            {                                
                mailServer.Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                if (async)
                {
                    mailServer.SendAsync(message, null);
                }
                else
                {
                    mailServer.Send(message);
                }
            }
		}

        private static void ValidateAddressses(MailAddressCollection addresses, bool emailException)
        {
            for (int i = addresses.Count - 1; i >= 0; i--)
            {
                if (!EmailValidator.IsValid(addresses[i].Address))
                {
                    if (!emailException)
                    {
                        string subject = string.Format("[WARNING] Invalid email address removed ({0})",
                            Assembly.GetEntryAssembly().GetName().Name);
                        new Email()
                            .SetSubject(subject)
                            .SetBody("Removed address " + addresses[i].ToString())
                            .SendException();
                    }
                    
                    addresses.RemoveAt(i);
                }
            }
        }

        private static IEnumerable<MailAddress> AddressesFromString(string addresses)
        {
            if (addresses.SafeTrimOrEmpty() == "")
            {
                return new List<MailAddress>();
            }
            else
            {
                List<MailAddress> items = new List<MailAddress>();
                string[] toStrings = addresses.Replace(';', ',').Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string address in toStrings)
                {
                    items.Add(new MailAddress(address));
                }
                return items;
            }
        }        

        internal static void SendEMail(Email email, bool async, bool emailException)
        {
            using (MailMessage message = CreateMessage(email.From.ToString(), email.BodyEncoding))
            {
                message.Subject = email.Subject;
                message.Body = email.Body;
                message.IsBodyHtml = email.Html;
                if (email.ReplyTo != null)
                {
                    message.ReplyToList.Add(email.ReplyTo);
                }
                AddAllAddresses(message.To, email.To);
                AddAllAddresses(message.CC, email.CC);
                AddAllAddresses(message.Bcc, email.BCC);
                if (email.Attachments != null)
                {
                    foreach (Attachment attachment in email.Attachments)
                    {
                        message.Attachments.Add(attachment);
                    }
                }

                SendEMail(message, async, emailException);
            }
        }

        private static string RunMailServerCheck(Stream stream, string mailServer)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    writer.WriteLine(string.Format("EHLO {0}", mailServer));
                    writer.Flush();
                    return reader.ReadLine();
                    // GMail responds with: 220 mx.google.com ESMTP
                }
            }
        }

        public static string RunMailServerCheck(string mailServer, int port)
        {
            string result = string.Empty;
            using (TcpClient client = new TcpClient())
            {                
                bool usingSsl = false;

                client.Connect(mailServer, port);

                using (NetworkStream stream = client.GetStream())
                {
                    if (usingSsl)
                    {
                        using (SslStream sslStream = new SslStream(stream))
                        {
                            sslStream.AuthenticateAsClient(mailServer);
                            result = RunMailServerCheck(sslStream, mailServer);
                        }
                    }
                    else
                    {
                        result = RunMailServerCheck(stream, mailServer);
                    }
                }
            }
            return result;
        }       
    }
}
