using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HRC.DatascapeFinancials.BL
{
    public class WorkOrder
    {
        [JsonProperty("ID")]
        public int ID { get; set; }

        [JsonProperty("JobKey")]
        public string JobKey { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("RCM")]
        public RCM RCM { get; set; }

        [JsonProperty("RCP")]
        public RCP RCP { get; set; }
    }

    public class RCM
    {
        [JsonProperty("LongDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("ProjectCode")]
        public string ProjectCode { get; set; }
    }

    public class RCP
    {
        [JsonProperty("LongDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("ProjectCode")]
        public string ProjectCode { get; set; }
    }
}
