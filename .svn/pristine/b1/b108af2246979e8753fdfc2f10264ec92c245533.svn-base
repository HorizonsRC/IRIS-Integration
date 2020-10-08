using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.IO;
using System.Xml.Serialization;

using HRC.Common;
using HRC.Common.Configuration;
using HRC.Common.Exceptions;

namespace HRC.Common
{
    public class ObjectMapManager
    {
        private static ObjectMapManager m_Instance { get; set; }
        public static ObjectMapManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new ObjectMapManager();
                }
                return m_Instance;
            }
        }

        public ObjectMapManager()
        {
            this.Repository = new List<ObjectMap>();
        }

        public List<ObjectMap> Repository { get; set; }

        public void Add(ObjectMap missingObject)
        {
            this.Repository.Add(missingObject);
        }

        public ObjectMap Load(object instance, string objectType, string value, string targetSystem)
        {
            return this.Repository.Find(delegate(ObjectMap m)
            {
                return ((instance == null && m.Instance == null) || m.Instance.Equals(instance)) &&
                    m.ObjectType.Equals(objectType, StringComparison.InvariantCultureIgnoreCase) &&
                    m.Value.SafeTrimOrEmpty().Equals(value.SafeTrimOrEmpty(), StringComparison.InvariantCultureIgnoreCase) &&
                    m.TargetSystem.Equals(targetSystem, StringComparison.InvariantCultureIgnoreCase);
            });
        }        

        public void Export(bool printStack, Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            if (printStack)
            {
                serializer.Serialize(stream, this);
            }
            else
            {
                ObjectMapManager manager = new ObjectMapManager();
                foreach (ObjectMap missingObject in this.Repository)
                {
                    manager.Repository.Add(ObjectMap.Create(missingObject.Instance, missingObject.ObjectType, missingObject.Value,
                        missingObject.TargetSystem, missingObject.SourceSystem, missingObject.Message));
                }
                serializer.Serialize(stream, manager);
            }
        }

        public void Add(string objectType, string value, string targetSystem, string sourceSystem)
        {
            this.Add(objectType, value, targetSystem, sourceSystem, null, string.Empty);
        }

        public void Add(string objectType, string value, string targetSystem, string sourceSystem,
            object instance, string message)
        {
            ObjectMap objectMap = this.Load(instance, objectType, value, targetSystem);
            if (objectMap != null)
            {
                objectMap.Count++;
            }
            else
            {
                objectMap = ObjectMap.Create(instance, objectType, value, targetSystem, sourceSystem, message);
                objectMap.StackTrace = StackItem.FromStackTrace(Environment.StackTrace);
                this.Add(objectMap);
            }
        }

        public static void AddGlobal(string objectType, string value, string targetSystem, string sourceSystem)
        {
            ObjectMapManager.Instance.Add(objectType, value, targetSystem, sourceSystem);
        }

        public void SaveToFile(bool printStack, string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                this.Export(printStack, stream);
            }
        }

        public static void SaveToFileGlobal(bool printStack, string fileName)
        {
            ObjectMapManager.Instance.SaveToFile(printStack, fileName);
        }

        private void SaveToStream(bool printStack, Stream stream)
        {
            this.Export(printStack, stream);
            stream.Position = 0;
        }        

        public void SendEmail(bool printStack, string fileName, List<string> addresses, 
            string subject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.SaveToStream(false, stream);
                Email email = new Email();
                email.AddAttachment(stream, fileName);
                foreach (string address in addresses)
                {
                    email.AddTo(address);
                }

                string domain = CommonConfig.Instance.MailFromEmail;
                domain = domain.SafeSubstring(domain.IndexOf("@") + 1, domain.Length);

                string applicationName = ConfigurationManager.AppSettings["ApplicationName"];

                email.SetFrom(string.Format("{0}@{1}", applicationName, domain),
                    applicationName);
                email.SetSubject(subject);
                email.Send();
            }
        }

        public static void SendEmailGlobal(bool printStack, string fileName, 
            List<string> addresses, string subject)
        {            
            ObjectMapManager.Instance.SendEmail(printStack, fileName, addresses, subject);
        }

    }

    public class ObjectMap 
    {
        [XmlIgnore]
        public object Instance { get; set; }
        public string Message { get; set; }

        public string ObjectType { get; set; }
        public string Value { get; set; }
        public string TargetSystem { get; set; }
        public string SourceSystem { get; set; } 

        public int Count { get; set; }
        
        public List<StackItem> StackTrace { get; set; }

        internal static ObjectMap Create(object instance, string objectType, string value, string targetSystem,
            string sourceSystem, string message)
        {
            ObjectMap objectMap = new ObjectMap();
            objectMap.ObjectType = objectType;
            objectMap.Value = value.SafeTrimOrEmpty();
            objectMap.TargetSystem = targetSystem;
            objectMap.SourceSystem = sourceSystem;
            objectMap.Instance = instance;
            objectMap.Message = string.IsNullOrEmpty(message) ? objectMap.BasicToString() : message;
            objectMap.Count = 1;
            return objectMap;
        }

        private string BasicToString()
        {
            return string.Format("Object Type: {0} with the Value: {1} is not known by the Application: [{2}]",
                this.ObjectType,
                this.Value,
                this.TargetSystem,
                Environment.NewLine,
                this.Count);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}This occurred: [{2}] times.",
                BasicToString(),
                Environment.NewLine,
                this.Count);
        }


    }
}
