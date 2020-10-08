using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HRC.Common.Validators
{
    public static class PhoneValidator
    {
        public static string Validate(string phone)
        {
            return Regex.Replace(phone, "[^0-9]", string.Empty);
        }
    }
}
