using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using HRC.Framework.BL;
using HRC.Common.Data;

namespace HRC.Framework.DL
{
    class DocumentConfigManager
    {
        internal static List<DocumentConfig> Load()
        {
            List<DocumentConfig> configs = new List<DocumentConfig>();
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "select dc.Name, dc.Id, dc.Value from DocumentConfig dc (nolock)";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DocumentConfig config = new DocumentConfig();
                            config.Id = (int)reader["Id"];
                            config.Name = (string)reader["Name"];
                            config.SetValue((string)reader["Value"]);
                            configs.Add(config);
                        }
                    }
                }

                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "SELECT dcv.[Id], dcv.[DocumentConfigId], dcv.[Name], dcv.[Value] FROM [DocumentConfigValue] dcv (nolock)";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DocumentConfigValue value = new DocumentConfigValue();
                            value.Id = (int)reader["Id"];
                            value.Name = (string)reader["Name"];
                            value.Value = (string)reader["Value"];

                            int documentConfigId = (int)reader["DocumentConfigId"];
                            DocumentConfig config = configs.Find(delegate(DocumentConfig dc) { return dc.Id == documentConfigId; });
                            if (config != null)
                            {
                                config.Values.Add(value);
                            }
                        }
                    }
                }

            }
            return configs;
        }
    }
}
