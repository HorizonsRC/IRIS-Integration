using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using HRC.Common;
using HRC.Common.Data;
using HRC.Common.Enums;
using HRC.IRIS.BL;

namespace HRC.IRIS.DL
{
    internal class IRISObjectManager
    {
        private static string FieldNames = @"{0}{1}[ID], {0}{1}[ObjectTypeID], {0}{1}[LinkID], {0}{1}[LinkDetails], {0}{1}[BusinessID], {0}{1}[EDRMSReference]";

        internal static List<IRISObject> Load()
        {
            List<IRISObject> objects = new List<IRISObject>();

            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"select {0} from IRISObject o (nolock)",
                        FieldNames.TranslateFields("o"));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            objects.Add(Load(reader));
                        }
                    }
                }
            }

            return objects;
        }

        private static IRISObject Load(IDataRecord record)
        {
            IRISObject data = new IRISObject();
            data.Id = CommandHelper.GetValueOrDefault<long>(record["ID"]);
            data.Name = CommandHelper.GetValueOrDefault<string>(record["LinkDetails"]);                       

            long objectTypeId = CommandHelper.GetValueOrDefault<long>(record["ObjectTypeID"]);
            data.IRISObjectType = ReferenceDataCollection.LoadObjectTypeFromId(objectTypeId);
            
            data.LinkId = (int)Convert.ChangeType(record["LinkId"], typeof(int));
            data.BusinessId = CommandHelper.GetValueOrDefault<string>(record["BusinessID"]);
            data.EDRMSReference = CommandHelper.GetValueOrDefault<string>(record["EDRMSReference"]);
            return data;
        }

        internal static IRISObject LoadFromContactId(int Id)
        {
            IRISObject data = null;
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    ReferenceDataValue contactReference = ReferenceDataCollection.LoadContact();                    
                    command.CommandText = string.Format(@"select {0} from IRISObject o (nolock) where o.LinkID = {1} and o.ObjectTypeID = {2}",
                        FieldNames.TranslateFields("o"),
                        command.CreateParameter("LinkID", Id),
                        command.CreateParameter("ObjectTypeID", contactReference.Id));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data = Load(reader);
                        }
                    }
                }
            }
            return data;
        }

        internal static IRISObject LoadFromEDRMSReference(string reference)
        {
            IRISObject data = null;
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"select {0} from IRISObject o (nolock) where o.EDRMSReference = {1}",
                        FieldNames.TranslateFields("o"),
                        command.CreateParameter("EDRMSReference", reference));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data = Load(reader);
                        }
                    }
                }
            }
            return data;
        }

        internal static IRISObject LoadFromBusinessId(string businessId)
        {
            IRISObject data = null;
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"select {0} from IRISObject o (nolock) where o.BusinessID = {1}",
                        FieldNames.TranslateFields("o"),
                        command.CreateParameter("BusinessID", businessId));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            data = Load(reader);
                        }
                    }
                }
            }
            return data;
        }

        internal static bool SetIsFinProjectCodeConfirmed(string businessId, bool newValue)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"UPDATE IRISObject SET IsFINProjectCodeConfirmed={1}  WHERE BusinessID = {0}",
                        command.CreateParameter("BusinessID", businessId), command.CreateParameter("IsFINProjectCodeConfirmed", newValue ? 1 : 0));
                    command.ExecuteNonQuery();
                    
                }
            }
            return false;
        }
    }
}
