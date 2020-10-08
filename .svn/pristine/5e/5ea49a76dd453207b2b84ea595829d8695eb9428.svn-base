using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;
using HRC.Contacts.BL;
using HRC.PowerBuilderContacts.DL;

namespace HRC.PowerBuilderContacts.BL
{
    public class PBContact : Contact
    {

        //public ContactType ContactType { get; set; }
        public PBPerson Person { get; set; }
        public PBOrganization Organization { get; set; }
        public List<PBAddress> Addresses { get; set; }
        public List<PBCommunication> Communications { get; set; }

        public PBContact() : base()
        {
        }

        public PBContact(ContactType contactType)
            : base(contactType)
        {
            //this.Addresses = new List<PBAddress>();
            //this.Communications = new List<PBCommunication>();
        }

        public int Save(bool newContact)
        {
            return PBContactManager.Save(this, newContact);
        }

        public static PBContact Load(int Id) 
        {
            //Creator<PBContact> creator = new Creator<PBContact>(() => new PBContact(PBContactManager.LoadContactType(Id)));            
            return PBContactManager.Load(Id);
        }

        public static PBContact Load(int Id, ContactType contactType) 
        {
            //Creator<PBContact> creator = new Creator<PBContact>(() => new PBContact(contactType));
            //return PBContactManager.Load<PBContact>(Id, creator);
            
            return PBContactManager.Load(Id, contactType);
        }
    }

    
}
