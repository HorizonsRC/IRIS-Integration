using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using HRC.Common.Attributes;

namespace HRC.Common.Enums
{
    internal class CommonEnum
    {
        #region Methods

        public static T GetValue<T>(string description)
            where T : struct
        {
            Type teaType = typeof(T);
            if (!teaType.IsEnum)
            {
                throw new ArgumentException("T must be of type enum");
            }
            string[] descriptions = GetDescriptions(teaType);
            Array values = Enum.GetValues(teaType);
            for (int i = 0; i < descriptions.Length; i++)
            {
                if (description == descriptions[i])
                {
                    return (T)values.GetValue(i);
                }
            }
            for (int i = 0; i < values.Length; i++)
            {
                if ((int)values.GetValue(i) == 0)
                {
                    return (T)values.GetValue(i);
                }
            }
            throw new InvalidOperationException("Invalid type/description combination and no default(0)");
        }

        public static DescriptionEnum<T> GetDescriptionValue<T>(T value)
        {
            return new DescriptionEnum<T>(value);
        }

        public static DescriptionEnum<T>[] GetDescriptionValues<T>()
            where T : struct
        {
            Array values = GetEnumValues<T>();
            DescriptionEnum<T>[] descriptions = new DescriptionEnum<T>[values.Length];
            int count = 0;
            foreach (Enum e in values)
            {
                descriptions[count++] = new DescriptionEnum<T>((T)(object)e);
            }
            return descriptions;
        }

        public static A GetAttribute<A>(Enum value)
            where A : Attribute
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                A attribute = CommonAttribute.GetAttribute<A>(field);
                return attribute;
            }
            return null;
        }

        public static string GetDescription(Enum value)
        {
            DescriptionAttribute descript = GetAttribute<DescriptionAttribute>(value);
            return descript == null ? value.ToString() : descript.Description;
        }

        public static string[] GetDescriptions(Type type)
        {
            Array values = GetEnumValues(type);
            string[] descriptions = new string[values.Length];
            int count = 0;
            foreach (Enum value in values)
            {
                descriptions[count++] = GetDescription(value);
            }
            return descriptions;
        }

        public static List<AttributeEnum<E, A>> GetAttributeEnums<E, A>()
            where E : struct
            where A : Attribute
        {
            Type type = typeof(E);
            Array values = GetEnumValues<E>();
            List<AttributeEnum<E, A>> list = new List<AttributeEnum<E, A>>();
            foreach (E value in values)
            {
                A attribute = GetAttribute<A>((Enum)(object)value);
                if (attribute != null)
                {
                    list.Add(new AttributeEnum<E, A>(value, attribute));
                }
            }
            return list;
        }

        static Array GetEnumValues<T>()
            where T : struct
        {
            return GetEnumValues(typeof(T));
        }

        static Array GetEnumValues(Type type)
        {
            return Enum.GetValues(type);
        }


        #endregion Methods
    }
}
