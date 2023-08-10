using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeFinancials.BL
{
    public class JobActivity
    {
        [JsonProperty("JobActivityID")]
        public string JobActivityID { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("ActiveOrFuture")]
        public bool ActiveOrFuture { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("JobID")]
        public int JobID { get; set; }

        [JsonProperty("ProjectCode")]
        public string ProjectCode { get; set; }
    }
}
