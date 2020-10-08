using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HRC.Common
{
    public abstract class IdNameBase<T>
    {
        private T m_Id;

        public T Id
        {
            get
            {
                return m_Id;
            }
            set
            {
                if (typeof(T) == typeof(string))
                {
                    m_Id = (T)(object)((string)(object)value).SafeTrimOrEmpty();
                }
                else
                {
                    m_Id = value;
                }
            }
        }
        private string m_Name;

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value.SafeTrimOrEmpty();
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Id.ToString();
            }
            return string.Format("{0} - {1}", this.Id, this.Name);
        }

        public override bool Equals(object obj)
        {
            IdNameBase<T> id = obj as IdNameBase<T>;
            return id != null && object.Equals(id.Id, this.Id);
        }

        public override int GetHashCode()
        {
            return (Id == null ? new object() : Id).GetHashCode();
        }
    }
}
