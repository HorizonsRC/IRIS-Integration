using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.Contacts.BL;

namespace HRC.OzoneContacts.BL
{
    public class OzoneCommunication
    {
        public Communication Base { get; set; }

        public char PhoneType { get; set; }
        public bool IsPhone { get; set; }
        public bool IsEmail { get; set; }
        public bool IsWebsite { get; set; }

        public string Dialer
        {
            get
            {
                return string.Format("{0}*{1}*{2}*{3}*{4}",
                    this.PhoneType, this.Base.CountryCode, this.Base.AreaCode, this.Base.Number, this.Base.Extension);
            }
        }


        
    }
}
