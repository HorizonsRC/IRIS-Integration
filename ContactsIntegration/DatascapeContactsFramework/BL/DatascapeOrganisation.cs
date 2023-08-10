using HRC.Contacts.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeOrganisation
    {
        public Organization Base { get; set; }
        public string TradingAs { get; set; }
        public string OrganisationRegistrationNumber { get; set; }
        public string GSTNumber { get; set; } // NOT IN IRIS
        public string CorrespondenceMethod { get; set; }
        public string InteractionMethod { get; set; }
    }
}
