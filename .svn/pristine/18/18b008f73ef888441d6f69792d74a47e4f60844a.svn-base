
using System;
using System.Collections.Generic;
using System.Text;

namespace HRC.Common.Enums
{
    public class AttributeEnum<E, A>
        where E : struct
        where A : Attribute
    {
        #region Fields
        
        E m_Value;
        A m_Attribute;

        #endregion

        #region Constructors

        public AttributeEnum(E value, A attribute)
        {
            this.Value = value;
            this.Attribute = attribute;
        }

        #endregion

        #region Properties

        public E Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public A Attribute
        {
            get { return m_Attribute; }
            set { m_Attribute = value; }
        }

        #endregion
    }
}
