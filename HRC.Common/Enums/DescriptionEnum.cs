
using System;
using System.Collections.Generic;
using System.Text;

namespace HRC.Common.Enums
{
    public class DescriptionEnum<T> : IComparable
    {
        #region Variables

        string m_Description;
        T m_Value;
        private static List<DescriptionEnum<T>> m_Cache;
        public static ToStringFormat ToStringFormat { get; set; }

        #endregion Variables

        #region Constructors

        public DescriptionEnum(T value)
            : this(value, CommonEnum.GetDescription((Enum)(object)value))
        {
        }

        public DescriptionEnum(T value, string description)
        {
            this.Value = value;
            this.Description = description;
        }

        #endregion Constructors

        #region Properties

        public string Description
        {
            get
            {
                return m_Description;
            }
            private set
            {
                m_Description = value;
            }
        }

        public T Value
        {
            get
            {
                return m_Value;
            }
            private set
            {
                m_Value = value;
            }
        }

        private static List<DescriptionEnum<T>> Cache
        {
            get
            {
                if (m_Cache == null)
                {
                    m_Cache = new List<DescriptionEnum<T>>();
                    foreach (T instance in Enum.GetValues(typeof(T)))
                    {
                        m_Cache.Add(new DescriptionEnum<T>(instance));
                    }
                }
                return m_Cache;
            }
        }

        #endregion Properties

        #region Methods

        public int CompareTo(object obj)
        {
            short value = 0;
            if (short.TryParse(Convert.ToInt16(this.Value).ToString(), out value))
            {
                return value.CompareTo(Convert.ToInt16(((DescriptionEnum<T>)obj).Value));
            }
            return 0;
        }

        public override string ToString()
        {
            if (ToStringFormat == ToStringFormat.UseIdAndName)
            {
                return string.Format("{0} - {1}", Convert.ToInt16(this.Value),
                    this.Description.SafeTrimOrEmpty());
            }
            else if (ToStringFormat == ToStringFormat.UseEnumString)
            {
                return string.Format("{0}", this.Value);
            }
            return this.Description.SafeTrimOrEmpty();
        }

        public override bool Equals(object obj)
        {
            DescriptionEnum<T> oVal = obj as DescriptionEnum<T>;
            return oVal == null ? false : object.Equals(oVal.Value, this.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        private static T LoadEnum(object Id)
        {
            return (T)Enum.ToObject(typeof(T), Id);
        }

        public static DescriptionEnum<T> Load(object Id)
        {
            return Cache.Find(delegate(DescriptionEnum<T> s)
            {
                return s.Value.Equals(LoadEnum(Id));
            });
        }

        public static DescriptionEnum<T> LoadFromName(string value)
        {
            return Cache.Find(delegate(DescriptionEnum<T> s)
            {
                return s.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase);
            });
        }

        public static DescriptionEnum<T> LoadFromDescription(string value)
        {
            return Cache.Find(delegate(DescriptionEnum<T> s)
            {
                return s.Description.Equals(value, StringComparison.CurrentCultureIgnoreCase);
            });
        }

        public static List<DescriptionEnum<T>> Load()
        {
            return new List<DescriptionEnum<T>>(Cache);
        }

        #endregion Methods
    }
}

