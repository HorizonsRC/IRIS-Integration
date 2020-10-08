using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.IRIS.DL;

namespace HRC.IRIS.BL
{
    public class ReferenceDataValue : IdNameBase<long>
    {
    }

    public class ReferenceDataCollection : IdNameBase<long>
    {
        private static List<ReferenceDataCollection> m_Cache;
        private static List<ReferenceDataCollection> Cache
        {
            get
            {
                if (m_Cache == null)
                {
                    m_Cache = ReferenceDataManager.Load();
                }
                return m_Cache;
            }
        }

        public List<ReferenceDataValue> Values { get; set; }

        public ReferenceDataCollection()
        {
            this.Values = new List<ReferenceDataValue>();
        }

        public static List<ReferenceDataCollection> Load()
        {
            return Cache;
        }

        private static ReferenceDataCollection Load(string name)
        {
            return Load().Find(delegate(ReferenceDataCollection c)
            {
                return c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static ReferenceDataCollection LoadObjectTypeCollection()
        {
            return Load("object type");
        }

        public static ReferenceDataValue LoadObjectTypeFromId(long Id)
        {
            ReferenceDataCollection collection = LoadObjectTypeCollection();
            if (collection != null)
            {
                return collection.Values.Find(delegate(ReferenceDataValue v)
                {
                    return v.Id == Id;
                });
            }
            return null;
        }

        public static ReferenceDataValue LoadObjectTypeFromName(string name)
        {
            ReferenceDataCollection collection = LoadObjectTypeCollection();
            if (collection != null)
            {
                return collection.Values.Find(delegate(ReferenceDataValue v)
                {
                    return v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
                });
            }
            return null;
        }

        internal static ReferenceDataValue LoadContact()
        {
            return LoadObjectTypeFromName("contact");
        }

        public static bool IsContact(ReferenceDataValue value)
        {            
            return LoadContact() == null ? false : LoadContact().Id == value.Id;
        }
    }
}
