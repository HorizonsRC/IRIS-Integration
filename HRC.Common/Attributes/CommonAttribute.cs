
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace HRC.Common.Attributes
{
    public static class CommonAttribute
    {
        #region Methods

        public static T GetAttribute<T>(MemberInfo member)
            where T : Attribute
        {
            object[] attributes = member.GetCustomAttributes(typeof(T), true);
            return attributes.Length == 0 ? null : (T)attributes[0];
        }

        public static AttributeInfo<A, M> GetAttribute<A, M>(M member)
            where A : Attribute
            where M : MemberInfo
        {
            return new AttributeInfo<A, M>(GetAttribute<A>(member), member);
        }

        public static List<AttributeInfo<A, M>> GetPropertyAttributes<A, M>(Type type)
            where A : Attribute
            where M : MemberInfo
        {
            return GetAttributes<A, M>(type, true);
        }

        public static List<AttributeInfo<A, M>> GetFieldAttributes<A, M>(Type type)
            where A : Attribute
            where M : MemberInfo
        {
            return GetAttributes<A, M>(type, false);
        }


        private static List<AttributeInfo<A, M>> GetAttributes<A, M>(Type type, bool getProperties)
            where A : Attribute
            where M : MemberInfo
        {
            List<MemberInfo> members = new List<MemberInfo>();
            if (getProperties)
            {
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    members.Add(property);
                }
            }
            else
            {
                FieldInfo[] fields = type.GetFields();
                foreach (FieldInfo field in fields)
                {
                    members.Add(field);
                }
            }
            List<AttributeInfo<A, M>> attributes = new List<AttributeInfo<A, M>>();
            foreach (MemberInfo member in members)
            {
                AttributeInfo<A, M> info = GetAttribute<A, M>((M)member);
                if (info.Attribute != null)
                {
                    attributes.Add(info);
                }
            }
            return attributes;
        }

        #endregion Methods
    }
}
