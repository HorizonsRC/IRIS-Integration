
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using HRC.Common;

namespace HRC.Common.Data
{
    public class SqlResult
    {
        public int RecsAffected { get; set; }
        public bool Successful { get; set; }
        public Exception Exception { get; set; }        
        public object Identity { get; set; }
        public DataSet DataSet { get; set; }

        public string ExceptionString
        {
            get
            {
                if (this.Exception != null)
                {
                    return this.Exception.Message.SafeTrimOrEmpty();
                }
                return string.Empty;                
            }
        }

        public SqlResult()
        {
            Successful = true;
        }        
    }
}
