using HRC.Contacts.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapePerson
    {
        public Person Base { get; set; }
        public string MiddleName { get; set; }
        public string SurnameSuffix { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MaritalStatus { get; set; }
        public string Confidential { get; set; }
        public string CorrespondenceMethod { get; set; }
        public string InteractionMethod { get; set; }
    }
}
