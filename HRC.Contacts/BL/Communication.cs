using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;

namespace HRC.Contacts.BL
{
    public class Communication : IdNameBase<int>
    {
        public string BasicCommunicationType { get; set; }
        public string CommunicationType { get; set; }
                
        public string Email { get; set; }
        public string Website { get; set; }
        
        public string Comments { get; set; }
        public bool? PrimaryFlag { get; set; }
        public int ContactId { get; set; }

        public long SourceId { get; set; } //IrisId

        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string Number { get; set; }      
        public string Extension {get; set;}

        public bool IsCurrent { get; set; }
    }
}
