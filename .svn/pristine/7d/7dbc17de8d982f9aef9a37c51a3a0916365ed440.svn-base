using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using HRC.Common.Data;

namespace HRC.Framework.BL
{
    public class Logging
    {
        public static void Write(string eventName, string description, string comment, long irisId = 0)
        {
            Write(eventName, description, comment, null, irisId);
        }

        public static void Write(string eventName, string description, string comment, string errorMessage, long irisId=0)
        {
            string sql = @"insert into Logging([DateTime], [Event], [Description], [WindowsUser], Comment, [Exception], [IrisId])
                values ({0}, {1}, {2}, {3}, {4}, {5}, {6}) ";            
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.Logging))
            {
                CommandHelper.ExecuteSqlQuery(sql, con,
                    DateTime.Now,
                    eventName,
                    description,
                    Environment.UserName,
                    comment,
                    errorMessage, irisId);
            }                        
        } 
    }
}
