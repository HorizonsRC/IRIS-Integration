
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace HRC.Common.Attributes
{
    public class AttributeInfo<A, M>
        where A : Attribute
        where M : MemberInfo
    {

        #region Variables

        private A m_Attribute;
        private M m_Member;

        #endregion Variables

        #region Constructors

        public AttributeInfo(A attribute, M member)
        {
            this.Attribute = attribute;
            this.Member = member;
        }

        #endregion Constructors

        #region Properties

        public M Member
        {
            get
            {
                return m_Member;
            }
            set
            {
                m_Member = value;
            }
        }

        public A Attribute
        {
            get
            {
                return m_Attribute;
            }
            set
            {
                m_Attribute = value;
            }
        }

        #endregion Properties
    }
}
