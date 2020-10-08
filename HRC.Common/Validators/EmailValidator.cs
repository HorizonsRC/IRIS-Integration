
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HRC.Common.Validators
{
    public static class EmailValidator
    {
        public static bool IsValid(string email)
        {
            return Regex.IsMatch(email, @"^([a-zA-Z0-9_\-\.+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.Compiled);
        }
    }
}
