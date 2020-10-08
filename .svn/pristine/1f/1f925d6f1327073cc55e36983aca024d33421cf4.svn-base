using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.Contacts.BL;

namespace HRC.OzoneContacts.BL
{
    public class OzoneAddress
    {
        public Address Base { get; set; }

        public string FullAddress
        {
            get
            {
                return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                    Environment.NewLine,
                    this.Base.AddressLine1,
                    this.Base.AddressLine2,
                    this.Base.AddressLine3,
                    this.Base.AddressLine4,
                    this.Base.AddressLine5).SafeTrimOrEmpty();
            }
        }
    }
}
