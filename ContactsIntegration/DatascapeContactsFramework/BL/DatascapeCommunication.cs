using HRC.Contacts.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeCommunication
    {
        public Communication Base { get; set; }
        public bool IsPhone { get; set; }
        public bool IsEmail { get; set; }
        public bool EmailIsBilling { get; set; }

        public bool CountryCodeIsValid()
        {
            if (String.IsNullOrEmpty(Base.CountryCode)){ return false; }
            string pattern = @"^(00(?!64)\d{2}|0076|0061)$";
            return Regex.IsMatch(Base.CountryCode, pattern);
        }
    }
}
