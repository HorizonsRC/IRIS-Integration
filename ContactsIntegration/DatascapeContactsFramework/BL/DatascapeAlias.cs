using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRC.Common;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeAlias : IdNameBase<int>
    {
        public string Note { get; set; }
        public long SourceID { get; set; }
    }
}
