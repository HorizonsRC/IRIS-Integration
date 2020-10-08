using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net;
using System.Configuration;
using System.Threading;
using HRC.Common.Enums;
using HRC.Common;
using HRC.Common.Security;

namespace HRC.Common.Data
{
    public static class CommandHelper    
    {
        private static int DefaultProcTimeout = 120;
        private static int DefaultCommandTimeout = 60;

        public static SqlParameter CreateParameter(this SqlCommand command, string parameterName, object value)
        {
            return CreateParameter<SqlCommand, SqlParameter>(command, parameterName, value);
        }
        
        public static param CreateParameter<cmd, param>(this cmd command, string parameterName, object value)            
            where cmd : DbCommand
            where param : DbParameter
        {
            return command.CreateParameter<param>(parameterName, value, command is SqlCommand ? "@" : ":");
        }

        public static param CreateParameter<param>(this DbCommand command, string parameterName, object value, string parameterPlaceHolder)
            where param : DbParameter
        {
            param parameter = (param)command.CreateParameter();
            parameter.ParameterName = parameterName.Length == 0 
                ? parameterPlaceHolder.SafeTrimOrEmpty()
                : parameterPlaceHolder.SafeTrimOrEmpty().Length == 0
                    ? parameterName
                    : (parameterName[0] == parameterPlaceHolder.SafeTrimOrEmpty()[0]) 
                        ? parameterName 
                        : string.Format("{0}{1}", parameterPlaceHolder.SafeTrimOrEmpty()[0], parameterName);
            parameter.SetParameterValue<param>(value);
            command.Parameters.Add(parameter);
            return parameter;
        }

        public static void SetParameterValue<param>(this param parameter, object value)
            where param : IDataParameter
        {
            ((param)parameter).Value = value ?? DBNull.Value;
        }

        private static T GetValueOrDefault<T>(object value, T defaultValue, out bool isNull)
        {           
            if (value == null || Convert.IsDBNull(value))
            {
                isNull = true;
                return defaultValue;
            }
            else
            {
                isNull = false;
                Type tType = typeof(T);
                if (tType.IsEnum)
                {
                    return (T)Enum.ToObject(tType, value);
                }
                else
                {
                    return (T)value;
                }
            }
        }

        public static T GetValueOrDefault<T>(object value, T defaultValue)
        {
            bool isNull;
            return GetValueOrDefault<T>(value, defaultValue, out isNull);
        }

        public static T GetValueOrDefault<T>(object value)
        {
            return GetValueOrDefault<T>(value, default(T));
        }   

        public static Nullable<T> GetNullOrValue<T>(object value) where T : struct
        {
            bool isNull;
            T t = GetValueOrDefault<T>(value, default(T), out isNull);
            return isNull ? null : (Nullable<T>)t;
        }

        public static Nullable<T> GetNullOrChangeType<T>(object value) where T : struct
        {
            if (value != null)
            {
                T t = (T)Convert.ChangeType(value, typeof(T));
                return (Nullable<T>)t;
            }
            else
            {
                return null;
            }
        }       

        public static T GetSqlValue<T>(string sql, SqlConnection con, params object[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.GetValue<T, SqlConnection, SqlCommand, SqlParameter>(sql, con, args);                       
        }        

        public static SqlResult ExecuteSqlQueryWithId(string sql, SqlConnection con, params object[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteQuery<SqlConnection, SqlCommand, SqlParameter>(sql, true, null,
                con, DefaultCommandTimeout, args);
        }

        public static SqlResult CheckConnection(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);            
            SqlResult result = new SqlResult();
            builder.ConnectTimeout = 5;
            try
            {
                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                }
            }
            catch (SqlException ex)
            {
                result.Successful = false;
                result.Exception = ex; 
            }
            return result;
        }

        public static SqlResult ExecuteSqlQuery(string sql, SqlConnection con, params object[] args)
        {            
            DbAnalysis.Current = new SqlDbAnalysis();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteQuery<SqlConnection, SqlCommand, SqlParameter>(sql, false, null,
                con, DefaultCommandTimeout, args);
        }

        public static SqlResult ExecuteSqlQueryWithResult(string sql, SqlConnection con, params object[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteQueryWithResult<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter>(sql, false, null,
                con, new SqlDataAdapter(), DefaultCommandTimeout, args);
        }

        public static SqlResult ExecuteSqlProcWithResult(string method, SqlConnection con, params Tuple<string, object>[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();            
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteProc<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter>(method, null,
                con, new SqlDataAdapter(), true, DefaultProcTimeout, args);
        }

        public static SqlResult ExecuteSqlProcWithResult(string method, SqlConnection con, int timeout, params Tuple<string, object>[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteProc<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter>(method, null,
                con, new SqlDataAdapter(), true, timeout, args);
        }
        
        public static SqlResult ExecuteSqlProc(string method, SqlConnection con, params Tuple<string, object>[] args)
        {
            DbAnalysis.Current = new SqlDbAnalysis();            
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            return DbAnalysis.Current.ExecuteProc<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter>(method, null,
                con, null, false, DefaultProcTimeout, args);
        }

        public static SqlConnection CreateConnection(ConnInstance connType)
        {
         
            SqlConnection con = new SqlConnection(GetConnectionString(connType));
            con.Open();
            return con;
        }

        public static SqlConnection CreateAppRoleConnection(AppRoleConnection appRole)
        {
            SqlConnection con = new SqlConnection(appRole.ConnectionString);
            con.Open();
            if (appRole.Credential != null)
            {
                ExecuteSqlQuery(string.Format("exec sp_setapprole '{0}', '{1}'", appRole.Credential.UserName, appRole.Credential.Password), con);
            }
            return con;
        }

        public static string GetConnectionString(ConnInstance connType)
        {
            DescriptionEnum<ConnInstance> descriptor = new DescriptionEnum<ConnInstance>(connType);
            return GetConnectionString(descriptor.Description);
        }

        private static string GetConnectionString(string connectionStringName)
        {
            string connStr = ConfigurationManager.ConnectionStrings[connectionStringName].ToString();
            string appName = ConfigurationManager.AppSettings["ApplicationName"];
            
            string windowsName = WindowsSecurity.GetWindowsName();

            string connStrAddition = string.Format("Workstation id=ifmuser:{0};Application Name={1}", windowsName, appName);            
            connStr += connStr.EndsWith(";") ? connStrAddition : string.Format(";{0}", connStrAddition);
            
            return connStr;
        }

        public static NetworkCredential GetAppRole(AppRoleInstance appRole)
        {
            DescriptionEnum<AppRoleInstance> descriptor = new DescriptionEnum<AppRoleInstance>(appRole);
            string[] appRoleArray = descriptor.Description.Split(';');
            NetworkCredential credential = new NetworkCredential();
            credential.UserName = ConfigurationManager.AppSettings[appRoleArray[0]].ToString();
            credential.Password = ConfigurationManager.AppSettings[appRoleArray[1]].ToString();
            return credential;
        }

        public static bool SqlObjectExists(string name, SqlObjectType objectType, AppRoleConnection appRole)
        {
            bool value = false;
            using (SqlConnection con = CommandHelper.CreateAppRoleConnection(appRole))
            {
                value = SqlObjectExists(name, objectType, con);
            }
            return value;
        }

        public static bool SqlObjectExists(string name, SqlObjectType objectType, SqlConnection con)
        {
            string sql = @"SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT) AS [OBJECTEXISTS] 
                             FROM SYS.OBJECTS O
                            WHERE O.NAME = {0} 
                              AND O.[TYPE] = {1}";
            DescriptionEnum<SqlObjectType> objectTypeEnum = new DescriptionEnum<SqlObjectType>(objectType);
            return GetSqlValue<bool>(sql, con, name, objectTypeEnum.Description);
        }

        public static int GetIdentity(SqlConnection con, SqlTransaction trans)
        {
            string sql = @"SELECT @@IDENTITY";
            int Id = default(int);
            using (SqlCommand command = con.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = trans;
                Id = (int)Convert.ChangeType(command.ExecuteScalar(), typeof(int));
            }
            return Id;
        }

        public static int GetNextKey(SqlConnection con, SqlTransaction trans, string tablename, string fieldname)
        {
            string hostname = ConfigurationManager.AppSettings["HostName"];
            string sql = @"SELECT next_id from glob_subscriber_next_key WHERE host_name={0} and table_name={1} and field_name={2} ";
            int Id = default(int);
            using (SqlCommand command = con.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandText = string.Format(sql,
                    command.CreateParameter("HostName", hostname),
                    command.CreateParameter("TableName", tablename),
                    command.CreateParameter("FieldName", fieldname)
                    );
                command.Transaction = trans;
                Id = (int)Convert.ChangeType(command.ExecuteScalar(), typeof(int));
            }
            return Id;
        }
        public static void UpdateNextKey(SqlConnection con, SqlTransaction trans, string tablename, string fieldname)
        {
            string hostname = ConfigurationManager.AppSettings["HostName"];
            string sql = @"UPDATE glob_subscriber_next_key SET next_id=next_id+1 WHERE host_name={0} and table_name={1} and field_name={2} ";
          
            using (SqlCommand command = con.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandText = string.Format(sql,
                    command.CreateParameter("HostName", hostname),
                    command.CreateParameter("TableName", tablename),
                    command.CreateParameter("FieldName", fieldname)
                    );
                command.Transaction = trans;
                command.ExecuteNonQuery();
            }
            return;
            
        }     
	}
}