using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using Newtonsoft.Json;

namespace HRC.Contacts.BL
{
    public class Contact 
    {
        public long IrisContactId { get; set; }

        public ContactType ContactType { get; set; }

        public Contact()
        {            
        }

        public Contact(ContactType contactType)
            : this()
        {         
          this.ContactType = contactType;
        }
    }
}
