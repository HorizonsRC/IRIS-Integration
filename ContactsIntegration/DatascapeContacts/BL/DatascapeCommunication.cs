﻿using HRC.Contacts.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeCommunication
    {
        public Communication Base { get; set; }
        public bool IsPhone { get; set; }
        public bool IsEmail { get; set; }
    }
}
