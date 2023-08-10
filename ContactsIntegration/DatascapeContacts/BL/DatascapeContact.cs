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
        public DatascapePerson Person { get; set; }
        public DatascapeOrganisation Organisation { get; set; }
        public List<DatascapeAddress> Addresses { get; set; }   
        public List<DatascapeCommunication> Communications { get; set; }
    }
}
