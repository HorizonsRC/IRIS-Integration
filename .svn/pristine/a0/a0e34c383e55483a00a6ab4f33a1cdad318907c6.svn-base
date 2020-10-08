
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace HRC.Common.Exceptions
{
    public class StackItem
    {
        #region Variables

        const string PATTERN = @"^\ *at\ (?:(?:(?<target>.+?):line\ (?<line>\d+))|(?<target>.+))";

        string m_Method;
        int m_LineNumber = -1;

        #endregion Variables

        #region Constructors

        #endregion Constructors

        #region Properties

        [XmlAttribute]
        public string Method
        {
            get
            {
                return m_Method;
            }
            set
            {
                m_Method = value;
            }
        }

        [XmlAttribute]
        public int LineNumber
        {
            get
            {
                return m_LineNumber;
            }
            set
            {
                m_LineNumber = value;
            }
        }

        #endregion Properties

        #region Methods

        public static List<StackItem> FromStackTrace(string stackTrace)
        {
            if (stackTrace == null)
            {
                return new List<StackItem>();
            }
            Regex regex = new Regex(PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            MatchCollection matches = regex.Matches(stackTrace);

            List<StackItem> items = new List<StackItem>();
            foreach (Match match in matches)
            {
                StackItem item = new StackItem();
                item.Method = match.Groups["target"].Value.TrimEnd('\r');
                int line;
                if (int.TryParse(match.Groups["line"].Value, out line))
                {
                    item.LineNumber = line;
                }
                items.Add(item);
            }
            return items;
        }

        #endregion Methods
    }
}
