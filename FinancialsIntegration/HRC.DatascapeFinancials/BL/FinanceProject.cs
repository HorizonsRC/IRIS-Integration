using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeFinancials
{
    public class FinanceProject
    {
        [JsonProperty("DatascapeJobGroup")]
        public string DatascapeJobGroup { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("ProjectCode")]
        public string ProjectCode { get; set; }

        [JsonProperty("LongDescription")]
        public string LongDescription { get; set; }
    }

    public class FinanceProjectPayload
    {
        [JsonProperty("IRISFinanceProject")]
        public FinanceProject FinanceProject { get; set; }
    }



    
}
