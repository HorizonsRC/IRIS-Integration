using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.Common.Enums;
using HRC.Framework.DL;

namespace HRC.Framework.BL
{
        public class DocumentConfigs
    {
        private static string defaultFileStorageMethod = "FileSystem"; //DocumentFileStorage.FileSystem;
        private static string m_FileStorageMethod = null; //DocumentFileStorage.None;
        
        public static string FileStorageMethodFromId(int Id)
        {
            if (DocumentConfig.Load("FileStorageMethod") != null)
            {
                DocumentConfigValue value = DocumentConfig.Load("FileStorageMethod").Values.Find(delegate(DocumentConfigValue dcv)
                {
                    return dcv.Id == Id;
                });
                if (value != null)
                {
                    return value.Name;
                }
            }
            return defaultFileStorageMethod;
        }

        public static string FileStorageMethod
        {
            get
            {
                if (string.IsNullOrEmpty(m_FileStorageMethod))
                {
                    if (DocumentConfig.Load("FileStorageMethod") != null && DocumentConfig.Load("FileStorageMethod").Value != null)
                    {
                        //DescriptionEnum<DocumentFileStorage> descriptor = DescriptionEnum<DocumentFileStorage>.LoadFromDescription(DocumentConfig.Load("FileStorageMethod").Value.Name);
                        //m_FileStorageMethod = descriptor.Value;                                                    
                        m_FileStorageMethod = DocumentConfig.Load("FileStorageMethod").Value.Name;
                    }
                    else
                    {
                        m_FileStorageMethod = defaultFileStorageMethod;
                    }
                }
                return m_FileStorageMethod;
            }
        }
    }

    public class DocumentConfigValue : IdNameBase<int>
    {
        public string Value { get; set; }

        public int DocumentConfigId { get; set; }
    }

    public class DocumentConfig : IdNameBase<int>
    {
        private string m_Value;

        private static List<DocumentConfig> m_Cache;

        private static List<DocumentConfig> Cache
        {
            get
            {
                if (m_Cache == null)
                {
                    m_Cache = DocumentConfigManager.Load();
                }
                return m_Cache;
            }
        }

        public DocumentConfigValue Value
        {
            get
            {
                return this.Values.Find(delegate(DocumentConfigValue dcv)
                {
                    return dcv.Value.Equals(m_Value, StringComparison.InvariantCultureIgnoreCase);
                });
            }
        }

        public List<DocumentConfigValue> Values { get; set; }

        internal void SetValue(string value)
        {
            m_Value = value;
        }

        public DocumentConfig()
        {
            this.Values = new List<DocumentConfigValue>();
        }

        public static DocumentConfig Load(string name)
        {
            return Cache.Find(delegate(DocumentConfig d) { return d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase); });
        }
    }
}
