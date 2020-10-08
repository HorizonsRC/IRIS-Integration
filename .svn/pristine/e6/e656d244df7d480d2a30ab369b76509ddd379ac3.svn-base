using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;

namespace HRC.OzoneContacts.BL
{
    public enum OzoneQueueStatus : byte
    {
        New = 0,                //not processed at all
        PartialCompletion = 1,  //processed; some attempted updates failed
        FullCompletion = 2,     //processed; no attempted updates failed
        Failed = 3,             //processed; all attempted updates failed 
    }
    
    public class OzoneQueueItem : IdNameBase<int>
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public OzoneQueueStatus Status { get; set; }
        public long IrisId { get; set; }
    }
}
