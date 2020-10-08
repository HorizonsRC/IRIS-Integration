using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using HRC.Common.Data;
using HRC.IRIS.BL;

namespace HRC.IRIS.DL
{
    internal class ReferenceDataManager
    {
        internal static List<ReferenceDataCollection> Load()
        {
            List<ReferenceDataCollection> items = new List<ReferenceDataCollection>();
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.IRIS))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = @"SELECT rdc.ID as CId, rdc.Name as CName, rdv.ID as VID, rdv.DisplayValue as VName
	  FROM [ReferenceDataCollection] rdc (nolock)
	  join ReferenceDataValue rdv (nolock) on rdc.ID = rdv.CollectionID
  order by rdc.ID, rdv.ID";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        long collectionId = long.MinValue;
                        ReferenceDataCollection collection = null;
                        while (reader.Read())
                        {                            
                            if (collectionId != (long)reader["CId"])
                            {
                                if (collectionId != long.MinValue)
                                {
                                    items.Add(collection);
                                }
                                collection = new ReferenceDataCollection();
                                collection.Id = (long)reader["CId"];
                                collection.Name = CommandHelper.GetValueOrDefault<string>(reader["CName"]);
                                collectionId = (long)reader["CId"];
                            }
                            ReferenceDataValue value = new ReferenceDataValue();
                            value.Id = (long)reader["VId"];
                            value.Name = CommandHelper.GetValueOrDefault<string>(reader["VName"]);
                            collection.Values.Add(value);
                        }
                        if (collection != null)
                        {
                            items.Add(collection);
                        }
                    }
                }
            }
            return items;
        }
    }
}
