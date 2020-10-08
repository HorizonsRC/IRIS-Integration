using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.Contacts.BL;

namespace HRC.OzoneContacts.BL
{
    public class OzonePerson
    {
        public int Id { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string ConfidentialReason { get; set; }

        public Person Base { get; set; }
        //public new List<OZOrganization> MemberOf { get; set; }

    }

}
