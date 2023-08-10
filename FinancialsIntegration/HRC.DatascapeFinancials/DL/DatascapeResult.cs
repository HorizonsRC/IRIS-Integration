using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HRC.DatascapeFinancials.DL
{
    public class DatascapePostResult : Result
    {
        [JsonProperty("JobNumber")]
        public string JobNumber { get; set; }
    }
}
