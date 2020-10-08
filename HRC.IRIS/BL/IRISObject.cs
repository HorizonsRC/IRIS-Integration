﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using HRC.Common;
using HRC.IRIS.DL;

namespace HRC.IRIS.BL
{
    public class IRISObject : IdNameBase<long>
    {
        private static bool UseCache = false;

        private static List<IRISObject> m_Cache;

        private static List<IRISObject> Cache
        {
            get
            {
                if (m_Cache == null && UseCache)
                {                                        
                    m_Cache = IRISObjectManager.Load();
                }
                return m_Cache;
            }
        }

        public string BusinessId { get; set; }

        public string ObjectType { get; set; }

        public string ObjectSubType { get; set; }

        public string EDRMSReference { get; set; }

        public int LinkId { get; set; }

        public ReferenceDataValue IRISObjectType { get; set; }

        public static List<IRISObject> Load()
        {
            return Cache;
        }

        public static IRISObject LoadFromContactId(int Id)
        {
            if (UseCache)
            {
                return Cache.Find(delegate(IRISObject o)
                {                    
                    return o.LinkId == Id && ReferenceDataCollection.IsContact(o.IRISObjectType);
                });
            }
            else
            {
                return IRISObjectManager.LoadFromContactId(Id);
            }
        }

        public static IRISObject LoadFromEDRMSReference(string reference)
        {
            if (UseCache)
            {
                return Cache.Find(delegate(IRISObject o)
                {
                    return string.Equals(o.EDRMSReference, reference, StringComparison.InvariantCultureIgnoreCase);
                });
            }
            else
            {
                return IRISObjectManager.LoadFromEDRMSReference(reference);
            }
        }

        public static IRISObject LoadFromBusinessID(string businessId)
        {
            if (UseCache)
            {
                return Cache.Find(delegate(IRISObject o)
                {
                    return string.Equals(o.BusinessId, businessId, StringComparison.InvariantCultureIgnoreCase);
                });
            }
            else
            {
                return IRISObjectManager.LoadFromBusinessId(businessId);
            }
        }

        public static bool SetIsFinProjectCodeConfirmed(string businessId, bool newValue)
        {
            return IRISObjectManager.SetIsFinProjectCodeConfirmed(businessId, newValue);
            //return false;
        }

    }
}
