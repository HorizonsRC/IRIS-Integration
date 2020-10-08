using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;

namespace HRC.Common.Data
{
    public class SqlDbAnalysis : DbAnalysis
    {
        private static List<string> DmlOperations = new List<string>() { "SELECT", "DELETE", "UPDATE", "INSERT" };

        public static byte? MaxDop { get; set; }

        protected override string StartAnalyzer(DbCommand command)
        {
            if (DbAnalysis.Active)
            {
                if (IncludeStats)
                {
                    ((SqlCommand)command).Connection.InfoMessage += ((SqlDbAnalysis)Current).OnInfoMessage;
                    command.CommandText = string.Format(
                        @"SET STATISTICS TIME ON
                        SET STATISTICS IO ON;{1}
                        {0};{1}
                        SET STATISTICS TIME OFF
                        SET STATISTICS IO OFF",
                        command.CommandText,
                        Environment.NewLine);
                }
                this.StartTime = DateTime.Now;
                this.Sql = command.CommandText;
                return command.CommandText;
            }
            else
            {
                return command.CommandText;
            }
        }
        
        public override string SetMaxDop(DbCommand command)
        {
            if (MaxDop != null)
            {
                string sql = command.CommandText.Trim();
                sql = sql.SafeSubstring(0, sql.IndexOf(" ")).ToUpper();

                if (DmlOperations.Contains(sql))
                {
                    return string.Format("{0} OPTION (MAXDOP {1})",
                        command.CommandText, MaxDop);
                }
                else
                {
                    return command.CommandText;
                }
            }
            else
            {
                return command.CommandText;
            }
        }

        public override void SetNoExec(DbConnection con)
        {
            if (!NoExec) return;
            string sql = @"SET NOEXEC ON";
            using (SqlCommand command = ((SqlConnection)con).CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteScalar();
            }
        }

        public override object GetIdentity(DbConnection con)
        {
            string sql = @"SELECT @@IDENTITY";
            using (DbCommand command = con.CreateCommand())
            {
                command.CommandText = sql;
                return command.ExecuteScalar();                
            }
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            this.m_IoData.Add(args.Message);
        }


    }
}
