using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRC.Contacts.BL;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeContact : Contact
    {
        public List<DatascapeAddress> Addresses { get; set; }   
    }
}
