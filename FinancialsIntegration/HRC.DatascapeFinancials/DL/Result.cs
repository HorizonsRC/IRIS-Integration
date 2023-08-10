using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeFinancials.DL
{
    public class Result
    {
        [JsonProperty("StatusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("InnerException")]
        public InnerException InnerException { get; set; }

    }

    public class Data
    {
    }

    public class InnerException
    {
        [JsonProperty("Contexts")]
        public List<string> Contexts { get; set; }

        [JsonProperty("StackTrace")]
        public string StackTrace { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Data")]
        public Data Data { get; set; }

        [JsonProperty("InnerException")]
        public InnerException innerException { get; set; }

        [JsonProperty("Source")]
        public string Source { get; set; }

        [JsonProperty("HResult")]
        public int HResult { get; set; }

        [JsonProperty("ValidationResult")]
        public ValidationResult ValidationResult { get; set; }
    }

    public class ValidationResult
    {
        [JsonProperty("MemberNames")]
        public List<object> MemberNames { get; set; }

        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }
    }
}
