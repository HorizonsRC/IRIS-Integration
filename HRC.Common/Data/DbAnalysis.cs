using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;
using System.Data.Common;
using System.Data;
using HRC.Common;
using HRC.Common.Data;
using HRC.Common.Configuration;

namespace HRC.Common.Data
{
    public class DbAnalysis
    {
        #region Fields
        
        private static List<DbAnalysis> m_Cache;                                        
        protected List<string> m_IoData = new List<string>();
        
        #endregion

        #region Properties

        public static DbAnalysis Current { get; set; }
        
        private static List<DbAnalysis> Cache
        {
            get 
            { 
                if (m_Cache == null)
                {
                     m_Cache = new List<DbAnalysis>();
                }
                return m_Cache; 
            }
        }

        protected static bool Active { get; set; }

        public static bool NoExec { get; set; }

        public static bool IncludeStats { get; set; }

        public static bool IncludeAnalysisOnException { get; set; }
        
        public string Sql  { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan ElapsedTime
        {
            get { return EndTime.Subtract(StartTime);  }
        }

        public SqlResult Result { get; set; }
    
        public string IoData
        {
            get 
            { 
                string s = string.Empty;
                if (this.m_IoData != null)
                {
                    foreach (string s1 in this.m_IoData)
                    {
                        s += string.Format("{0}{1}", s1, Environment.NewLine);
                    }
                }
                return s;
            }            
        }
        #endregion

        #region Methods

        protected virtual void StartAnalyzer()
        {
            this.StartTime = DateTime.Now;
        }

        protected virtual string StartAnalyzer(DbCommand command) 
        {
            if (DbAnalysis.Active)
            {
                this.StartTime = DateTime.Now;
                this.Sql = command.CommandText;
                return command.CommandText;
            }
            else
            {
                return command.CommandText;
            }
        }

        public static List<DbAnalysis> Load()
        {
            return Cache;
        }
        
        public static void Open()
        {
            DbAnalysis.Active = true;
        }
        
        public static void Close()
        {
            DbAnalysis.Active = false;
            Cache.Clear();
        }                        

        public static void EndAnalyzer(SqlResult result)
        {
            if (!DbAnalysis.Active)
            {
                return;
            }
            
            if (Current != null)
            {
                Current.EndTime = DateTime.Now;
                Current.Result = result;
                Cache.Add(Current);
                Current = null;
            }
        }

        public static string GetFileName(string extra)
        {
            return string.Format("SqlAnalysis_{0} {1}.txt", 
                DateTime.Now.ToString("ddMMyyyy-hhmmss"),
                extra);
        }

        public static void ExportAnalysis(string path)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TimeSpan elapsedTimeSpan = new TimeSpan();
                TimeSpan totalTimeSpan = DbAnalysis.ExportAnalysis(stream, ref elapsedTimeSpan);
                using (FileStream fileStream = new FileStream(path, FileMode.CreateNew))
                {
                    stream.WriteTo(fileStream);
                    fileStream.Close();
                }
            }
        }

        public static TimeSpan ExportAnalysis(MemoryStream stream, ref TimeSpan elapsedTimeSpan)
        {
            TimeSpan totalTimeSpan = new TimeSpan();
            if (stream != null)
            {
                DateTime totalTime = DateTime.MinValue;                
                foreach (DbAnalysis query in DbAnalysis.Load())                
                {                    
                    totalTime = totalTime.AddTicks(query.ElapsedTime.Ticks);                    
                }
                totalTimeSpan = totalTime.Subtract(DateTime.MinValue);
                
                if (DbAnalysis.Load().Count > 0)
                {
                    DateTime elapsedTime = DbAnalysis.Load()[0].StartTime;
                    int count = DbAnalysis.Load().Count - 1;
                    elapsedTimeSpan = DbAnalysis.Load()[count].EndTime.Subtract(elapsedTime);
                }

                string msg = string.Format("Total Elapsed Time: {2}{0}Total Querying Time: {1}{0}{0}",
                    Environment.NewLine,
                    ToTimeString(totalTimeSpan),
                    ToTimeString(elapsedTimeSpan));

                foreach (DbAnalysis query in DbAnalysis.Load())
                {
                    msg += string.Format("Query took {1} seconds{0}{6}{0}Recs Affected:{5}{0}ID:{7}{0}IO: {2}{0}{3}{0}{4}{0}{0}",
                        Environment.NewLine,
                        ToTimeString(query.ElapsedTime),
                        query.IoData,
                        query.Sql,
                        "-------------------",
                        query.Result.RecsAffected.ToString(),
                        query.Result.Successful ? "Successful" : "Error: " +
                            query.Result.Exception != null ? query.Result.Exception.Message.ToString() : string.Empty,
                            query.Result.Identity != null ? query.Result.Identity.ToString() : string.Empty);
                }                
                stream.Write(System.Text.Encoding.Default.GetBytes(msg), 0, msg.Length);
                stream.Position = 0;
            }
            return totalTimeSpan;
        }

        public static void Email()
        {            
            string subject = string.Format("[SqlLog] for {0})",
                Application.ProductName);

            using (MemoryStream stream = new MemoryStream())
            {
                TimeSpan elapsedTimeSpan = new TimeSpan();
                TimeSpan totalTimeSpan = ExportAnalysis(stream, ref elapsedTimeSpan);

                string body = string.Format("Total Elapsed Time: {2}{0}Total Querying Time: {1}{0}{0}",
                    Environment.NewLine,
                    ToTimeString(totalTimeSpan),
                    ToTimeString(elapsedTimeSpan));

                body += string.Format("Total Queries: {0}{1}", DbAnalysis.Load().Count, Environment.NewLine);
                body += string.Format("Error Queries: {0}", DbAnalysis.Load().FindAll(delegate(DbAnalysis q)
                {
                    return q.Result.Successful == false;
                }).Count);

                string displayName = string.Format("{0} Logs", Application.ProductName);

                new Email()
                    .SetFrom(new MailAddress(CommonConfig.Instance.MailFromEmail, displayName))
                    .AddTo(CommonConfig.Instance.ExceptionEmailTo)
                    .AddAttachment(stream, GetFileName(null))
                    .SetSubject(subject)
                    .SetBody(body)                    
                    .Send();
            }
        }

        private static string ToTimeString(TimeSpan span)
        {
            return string.Format("{0}:{1:00}:{2:00}.{3:00}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
        }

        public virtual string SetMaxDop(DbCommand command)
        {
            return command.CommandText;
        }

        public virtual void SetNoExec(DbConnection con)
        {            
        }

        public virtual object GetIdentity(DbConnection con)
        {
            return 0;
        }

        private object[] CreateParameters<cmd, param>(cmd command, object[] args)
            where cmd : DbCommand
            where param : DbParameter
        {
            if (args != null)
            {
                object[] newArgs = new object[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    newArgs[i] = ((cmd)command).CreateParameter<cmd, param>(string.Format("VALUE{0}", i), args[i]).ParameterName;
                }
                return newArgs;
            }
            return new object[0];
        }

        internal T GetValue<T, cn, cmd, param>(string sql, cn con, params object[] args)
            where cn : DbConnection
            where cmd : DbCommand
            where param : DbParameter
        {
            using (DbCommand command = con.CreateCommand())
            {
                command.CommandText = string.Format(sql, this.CreateParameters<cmd, param>((cmd)command, args));
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        return CommandHelper.GetValueOrDefault<T>(reader[0]);
                    }
                    else
                    {
                        return default(T);                        
                    }
                }
            }
        }

        internal SqlResult ExecuteProc<cn, cmd, param, data>(string method, DbTransaction trans,
            cn con, data dataAdapter, bool hasResultSet, int timeout, params Tuple<string, object>[] args)
            where cn : DbConnection
            where cmd : DbCommand
            where param : DbParameter
            where data : DbDataAdapter
        {
            SqlResult result = new SqlResult();
            using (DbCommand command = con.CreateCommand())
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null)
                    {
                        ((cmd)command).CreateParameter<cmd, param>(args[i].Item1, args[i].Item2);
                    }
                }
                command.CommandTimeout = timeout;
                command.CommandText = method;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = trans;

                this.StartAnalyzer(command);
                command.CommandText = this.SetMaxDop(command);
                try
                {
                    this.SetNoExec(con);
                    if (hasResultSet)
                    {
                        dataAdapter.SelectCommand = command;
                        result.DataSet = new DataSet();
                        dataAdapter.Fill(result.DataSet, "ResultSet");
                    }
                    else
                    {
                        result.RecsAffected = command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    OnException(ex, result);
                }
                DbAnalysis.EndAnalyzer(result);
            }
            return result;
        }

        internal SqlResult ExecuteQuery<cn, cmd, param>(string sql, bool getId, DbTransaction trans,
            cn con, int timeout, params object[] args)
            where cn : DbConnection
            where cmd : DbCommand
            where param : DbParameter
        {
            SqlResult result = new SqlResult();
            using (DbCommand command = con.CreateCommand())
            {
                object[] newArgs = this.CreateParameters<cmd, param>((cmd)command, args);
                command.CommandTimeout = timeout;
                command.CommandText = string.Format(sql, newArgs);
                command.Transaction = trans;                

                this.StartAnalyzer(command);
                command.CommandText = this.SetMaxDop(command);
                try
                {   
                    this.SetNoExec(con);
                    result.RecsAffected = command.ExecuteNonQuery();                    
                }
                catch (Exception ex)
                {
                    OnException(ex, result);
                }
                result.Identity = getId && result.Successful ? this.GetIdentity(con) : 0;
                DbAnalysis.EndAnalyzer(result);
            }
            return result;
        }

        internal SqlResult ExecuteQueryWithResult<cn, cmd, param, data>(string sql, bool getId, DbTransaction trans,
            cn con, data dataAdapter, int timeout, params object[] args)
            where cn : DbConnection
            where cmd : DbCommand
            where param : DbParameter
            where data : DbDataAdapter
        {
            SqlResult result = new SqlResult();
            using (DbCommand command = con.CreateCommand())
            {
                object[] newArgs = this.CreateParameters<cmd, param>((cmd)command, args);
                command.CommandTimeout = timeout;
                command.CommandText = string.Format(sql, newArgs);
                command.Transaction = trans;

                this.StartAnalyzer(command);
                command.CommandText = this.SetMaxDop(command);
                try
                {
                    this.SetNoExec(con);
                    dataAdapter.SelectCommand = command;
                    result.DataSet = new DataSet();
                    dataAdapter.Fill(result.DataSet, "ResultSet");
                }
                catch (Exception ex)
                {
                    OnException(ex, result);
                }
                DbAnalysis.EndAnalyzer(result);
            }
            return result;
        }

        private SqlResult OnException(Exception ex, SqlResult result)
        {
            result.Successful = false;
            result.Exception = ex;
            return result;
        }

        #endregion Methods



        
    }
}
