using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.IO;
using HRC.Common;
using HRC.Common.Data;
using HRC.Framework.BL;

namespace HRC.Framework.DL
{
    internal class DocumentContext
    {
        internal string PathName { get; set; }
        internal byte[] TransactionContext { get; set; }
    }

    internal class DocumentManager
    {
        private static string DocumentFields = @"{0}{1}IrisId, {0}{1}Id, {0}{1}LatestVersionId, {0}{1}DocumentName, {0}{1}DocumentFullPath, {0}{1}Owner, {0}{1} Status";
        private static string DocumentVersionFields = @"{0}{1}VersionId, {0}{1}CreatedDate, {0}{1}CreatedBy, {0}{1}ModifiedDate, {0}{1}ModifiedBy, {0}{1}DocumentFileStorage";

        internal static void Save(Document document, DocumentVersion version)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlTransaction trans = con.BeginTransaction())
                {
                    string sql = @"INSERT INTO DOCUMENT(IRISID, LATESTVERSIONID, DOCUMENTNAME, 
                                               DOCUMENTFULLPATH, OWNER, STATUS) 
                                               VALUES ({0}, {1}, {2}, {3}, {4}, {5})";

                    using (SqlCommand docInsert = con.CreateCommand())
                    {
                        docInsert.CommandText = string.Format(sql,
                            docInsert.CreateParameter("IRISID", document.IrisId),
                            docInsert.CreateParameter("VERSIONID", document.CurrentVersionId),
                            docInsert.CreateParameter("DOCUMENTNAME", document.Name),
                            docInsert.CreateParameter("DOCUMENTFULLPATH", document.DocumentFullPath),
                            docInsert.CreateParameter("OWNER", document.Owner),
                            docInsert.CreateParameter("STATUS", (short)document.Status));

                        docInsert.Transaction = trans;
                        docInsert.ExecuteNonQuery();
                    }

                    document.Id = CommandHelper.GetIdentity(con, trans);                    
                    
                    version.DocumentId = document.Id;
                    
                    SaveVersion(con, trans, document, version);

                    trans.Commit();
                }
            }
        }

        internal static void SaveVersion(Document document, DocumentVersion version)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlTransaction trans = con.BeginTransaction())
                {
                    SaveVersion(con, trans, document, version);

                    trans.Commit();
                }
            }
        }

        private static void SaveVersion(SqlConnection con, SqlTransaction trans, Document document, DocumentVersion version)
        {
            string sql = 
                @"INSERT INTO DOCUMENTVERSION (DOCUMENTID, VERSIONID, CREATEDDATE, CREATEDBY, 
                                               MODIFIEDDATE, MODIFIEDBY, DocumentFS, DocumentFileStorage)
                                       VALUES ({0}, {1}, {2}, {3}, {4}, {5}, 0x, {6})";

            using (SqlCommand versionInsert = con.CreateCommand())
            {
                versionInsert.CommandText = string.Format(sql,
                    versionInsert.CreateParameter("DOCUMENTID", document.Id),
                    versionInsert.CreateParameter("VERSIONID", version.VersionId),
                    versionInsert.CreateParameter("CREATEDDATE", version.CreatedDate),
                    versionInsert.CreateParameter("CREATEDBY", version.CreatedBy),
                    versionInsert.CreateParameter("MODIFIEDDATE", version.ModifiedDate),
                    versionInsert.CreateParameter("MODIFIEDBY", version.ModifiedBy),
                    versionInsert.CreateParameter("DocumentFileStorage", (byte)Document.FileStorageMethod));

                versionInsert.Transaction = trans;
                versionInsert.ExecuteNonQuery();

                if (Document.FileStorageMethod == DocumentFileStorage.DatabaseBinary)
                {
                    SaveFileBinary(con, trans, version);
                }
                else if (Document.FileStorageMethod == DocumentFileStorage.DatabaseFileStream)
                {
                    SaveFileStream(con, trans, version);
                }
            }
        }

        internal static void SaveFile(DocumentVersion version)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlTransaction trans = con.BeginTransaction())
                {
                    if (Document.FileStorageMethod == DocumentFileStorage.DatabaseBinary)
                    {
                        SaveFileBinary(con, trans, version);
                    }
                    else if (Document.FileStorageMethod == DocumentFileStorage.DatabaseFileStream)
                    {
                        SaveFileStream(con, trans, version);
                    }
                    trans.Commit();
                }
            }
        }

        private static void SaveFileStream(SqlConnection con, SqlTransaction trans, DocumentVersion version)
        {
            DocumentContext context = GetDocumentContext(con, trans, version.DocumentId, version.VersionId);            
            using (SqlFileStream stream = new SqlFileStream(context.PathName, context.TransactionContext, FileAccess.ReadWrite))
            {
                stream.Write(version.Document, 0, version.Document.Length);
            }                               
        }

        internal static void SaveFileBinary(SqlConnection con, SqlTransaction trans, DocumentVersion version)
        {
            string sql = @"UPDATE DV
                              SET DV.DOCUMENT = {2}
                             FROM DOCUMENTVERSION DV (ROWLOCK)
                            WHERE DV.DOCUMENTID = {0} 
                              AND DV.VERSIONID = {1}";

            using (SqlCommand command = con.CreateCommand())
            {
                command.CommandText = string.Format(sql,
                    command.CreateParameter("ID", version.DocumentId),
                    command.CreateParameter("VERSIONID", version.VersionId),
                    "@DOCUMENT");

                SqlParameter param = command.Parameters.Add("@DOCUMENT", SqlDbType.VarBinary);
                param.Value = version.Document;
                command.Transaction = trans;
                command.ExecuteNonQuery();
            }
        }

        private static void LoadFileStream(SqlConnection con, SqlTransaction trans, ref DocumentVersion version)
        {
            DocumentContext context = GetDocumentContext(con, trans, version.DocumentId, version.VersionId);
            using (SqlFileStream stream = new SqlFileStream(context.PathName, context.TransactionContext, FileAccess.Read))
            {
                int length = (int)stream.Length;
                version.Document = new byte[length];
                stream.Read(version.Document, 0, length);
            }
        }

        internal static void LoadFileStream(SqlConnection con, ref DocumentVersion version)
        {
            using (SqlTransaction trans = con.BeginTransaction())
            {
                LoadFileStream(con, trans, ref version);
                trans.Commit();
            }
        }

        internal static void LoadFileStream(ref DocumentVersion version)
        {
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                LoadFileStream(con, ref version);
            }
        }

        private static DocumentContext GetDocumentContext(SqlConnection con, SqlTransaction trans, int Id, Guid guid)
        {
            DocumentContext context = new DocumentContext();
            
            string sql = @"SELECT DocumentFS.PathName() as PathName, GET_FILESTREAM_TRANSACTION_CONTEXT() 
                             FROM DOCUMENTVERSION dv (nolock)
                            WHERE DV.DOCUMENTID = {0}
                              AND DV.VERSIONID = {1}";

            using (SqlCommand command = con.CreateCommand())
            {
                command.Transaction = trans;
                command.CommandText = string.Format(sql,
                    command.CreateParameter("ID", Id),
                    command.CreateParameter("VERSIONID", guid));

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        context.PathName = CommandHelper.GetValueOrDefault<string>(reader["PathName"]);
                        context.TransactionContext = reader.GetSqlBytes(1).Buffer;
                    }
                }
            }
            return context;
        }                

        private static Document LoadDocument(IDataRecord record)
        {
            Document document = new Document();
            document.Id = CommandHelper.GetValueOrDefault<int>(record["ID"]);
            document.IrisId = CommandHelper.GetValueOrDefault<long>(record["IRISID"]);                        
            document.Name = CommandHelper.GetValueOrDefault<string>(record["DocumentName"]);
            document.DocumentFullPath = CommandHelper.GetValueOrDefault<string>(record["DocumentFullPath"]);
            document.Owner = CommandHelper.GetValueOrDefault<string>(record["Owner"]);
            document.Status = (DocumentStatus)CommandHelper.GetValueOrDefault<short>(record["Status"]);
            return document;
        }

        private static DocumentVersion LoadDocumentDetails(SqlConnection con, IDataRecord record, int Id, bool loadDocument)
        {
            DocumentVersion version = new DocumentVersion();
            version.DocumentId = Id;
            version.VersionId = CommandHelper.GetValueOrDefault<Guid>(record["VersionId"]);
            version.CreatedDate = CommandHelper.GetValueOrDefault<DateTime>(record["CreatedDate"]);
            version.CreatedBy = CommandHelper.GetValueOrDefault<string>(record["CreatedBy"]);
            version.ModifiedDate = CommandHelper.GetValueOrDefault<DateTime>(record["ModifiedDate"]);
            version.ModifiedBy = CommandHelper.GetValueOrDefault<string>(record["ModifiedBy"]);

            version.FileStorageMethod = (DocumentFileStorage)CommandHelper.GetValueOrDefault<byte>(record["DocumentFileStorage"]);                        

            if (loadDocument)
            {
                if (version.FileStorageMethod == DocumentFileStorage.DatabaseBinary)
                {
                    version.Document = Convert.IsDBNull(record["Document"]) ? null : (byte[])record["Document"];
                }
                else if (version.FileStorageMethod == DocumentFileStorage.DatabaseBinary)
                {                    
                    //need to load document from the stream from outside this scope
                }
            }
            return version;
        }

        internal static List<Document> Load(long IrisId, bool loadDocumentContent)
        {
            List<Document> documents = new List<Document>();
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"
                           SELECT {0}, {1}{2} {3} 
                             FROM DOCUMENT D (NOLOCK) 
                             JOIN DOCUMENTVERSION DV (NOLOCK) ON D.ID = DV.DOCUMENTID 
                            WHERE D.IRISID = {4}
                         ORDER BY D.ID ASC",
                        DocumentFields.TranslateFields("D"),
                        DocumentVersionFields.TranslateFields("DV"),
                        loadDocumentContent ? "," : string.Empty,
                        loadDocumentContent ? "{0}{1}[DOCUMENT]".TranslateFields("DV") : string.Empty,
                        command.CreateParameter("IRISID", IrisId));

                    LoadDocuments(con, command, ref documents, loadDocumentContent);
                }

                //here we will load document content from outside the scope of an existing SqlDataReader
                if (loadDocumentContent)
                {
                    LoadDocumentContent(con, ref documents);
                }
            }
            return documents;
        }

        private static void LoadDocumentContent(SqlConnection con, ref List<Document> documents)
        {                        
            foreach (Document document in documents)
            {
                for (int i = 0; i < document.Versions.Count; i++)
                {
                    DocumentVersion version = document.Versions[i];
                    if (version.FileStorageMethod == DocumentFileStorage.DatabaseFileStream)
                    {
                        LoadFileStream(con, ref version);
                    }
                }
            }
        }

        internal static List<Document> Load(List<int> documentIds, bool loadDocumentContent)
        {
            List<Document> documents = new List<Document>();
            using (SqlConnection con = CommandHelper.CreateConnection(ConnInstance.HRCFramework))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = string.Format(@"
                           SELECT {0}, {1}{2} {3} 
                             FROM DOCUMENT D (NOLOCK) 
                             JOIN DOCUMENTVERSION DV (NOLOCK) ON D.ID = DV.DOCUMENTID 
                            WHERE D.ID IN ({4})
                         ORDER BY D.ID ASC",
                        DocumentFields.TranslateFields("D"),
                        DocumentVersionFields.TranslateFields("DV"),
                        loadDocumentContent ? "," : string.Empty,
                        loadDocumentContent ? "{0}{1}[DOCUMENT]".TranslateFields("DV") : string.Empty,
                        documentIds.ToSvString(false, ","));

                    LoadDocuments(con, command, ref documents, loadDocumentContent);
                }

                if (loadDocumentContent)
                {
                    LoadDocumentContent(con, ref documents);
                }
            }
            return documents;
        }

        private static void LoadDocuments(SqlConnection con, SqlCommand command, ref List<Document> documents, bool loadDocumentContent)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                int Id = int.MinValue;
                int Idx = -1;
                while (reader.Read())
                {
                    if (Id != (int)reader["ID"])
                    {
                        Id = (int)reader["ID"];
                        documents.Add(LoadDocument(reader));
                        Idx++;
                    }
                    documents[Idx].Versions.Add(LoadDocumentDetails(con, reader, Id, loadDocumentContent));
                }
            }
        }
    }
}
