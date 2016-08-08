using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Common.DataLayer
{
    partial class SqlHelper
    {
        // Tries to make useful error messages out of useless bulk copy errors
        private class SqlBulkCopyExceptionHelper
        {
            private SqlBulkCopy SqlBulkCopy { get; set; }
            private Exception InnerException { get; set; }
            private string ConnectionString { get; set; }
            private DataTable SourceTable { get; set; }

            public SqlBulkCopyExceptionHelper(string connectionString, SqlBulkCopy bc, Exception ex, DataTable sourceTable)
            {
                this.ConnectionString = connectionString;
                this.SqlBulkCopy = bc;
                this.InnerException = ex;
                this.SourceTable = sourceTable;
            }

            public bool TryHandle(out Exception ex)
            {
                var handlers = new Func<Exception>[] { HandleInvalidColumnLength, HandleConversionError };
                foreach (var h in handlers)
                {
                    ex = h();
                    if (ex != null) return true;
                }
                ex = null;
                return false;
            }

            private Exception HandleConversionError()
            {
                DataTable schemaDt = GetSchema();

                foreach (DataRow dr in this.SourceTable.Rows)
                {
                    foreach (DataColumn dc in schemaDt.Columns)
                    {
                        object o = dr[GetSourceColumnIndex(dc.Ordinal)];
                        if (o == null || o == DBNull.Value) continue;

                        try
                        {
                            Convert.ChangeType(o, dc.DataType);
                        }
                        catch (FormatException)
                        {
                            return new InvalidOperationException(
                                string.Format("Cannot convert {0} to {1}. Row {2}, Column {3}:{4}", o, dc.DataType, dr.Table.Rows.IndexOf(dr), dc.Ordinal, dc.ColumnName),
                                this.InnerException
                            );
                        }
                    }
                }
                return null;
            }

            private Exception HandleInvalidColumnLength()
            {
                const string INVALID_COL_LENGTH = "Received an invalid column length from the bcp client for colid";
                const int ERROR_CODE = unchecked((int)0x80131904);
                var sqlEx = this.InnerException as SqlException;
                if (sqlEx == null || sqlEx.ErrorCode != ERROR_CODE || sqlEx.Errors.Count < 1)
                {
                    // Not correct SQL Exception
                    return null;
                }
                string msg = sqlEx.Errors[0].Message;
                int columnId;
                if (msg.Length < INVALID_COL_LENGTH.Length || !int.TryParse(msg.Substring(INVALID_COL_LENGTH.Length).TrimEnd('.'), out columnId))
                {
                    // Incorrect message
                    return null;
                }

                // get length of Sql table column
                DataTable schemaDt = GetSchema();
                var destCol = schemaDt.Columns[columnId - 1]; // columnId is 1-based
                var sourceCol = this.SourceTable.Columns[GetSourceColumnIndex(destCol.Ordinal)];
                var destColLength = destCol.MaxLength;
                if (destColLength <= 0) return null;

                foreach (DataRow dr in this.SourceTable.Rows)
                {
                    var s = dr[sourceCol.Ordinal] as string;
                    if (s == null) continue;

                    if (s.Length > destColLength)
                    {
                        return new InvalidOperationException(
                            string.Format("Row {0}, Column index {1} is {2} chars, but {3}.{4} max length is {5} chars",
                                this.SourceTable.Rows.IndexOf(dr), sourceCol.Ordinal, s.Length,
                                this.SqlBulkCopy.DestinationTableName, schemaDt.Columns[destCol.Ordinal].ColumnName, destColLength
                            ),
                            this.InnerException
                        );
                    }
                }

                return null;
            }

            private DataTable GetSchema()
            {
                DataTable schemaDt = new DataTable();
                using (var conn = new SqlConnection(this.ConnectionString))
                using (var cmd = new SqlCommand("SELECT * FROM " + this.SqlBulkCopy.DestinationTableName, conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    ConnectionMonitor monitor = ConnectionUtility.Monitor;
                    if (monitor != null)
                    {
                        monitor.Add(new ConnectionInfo(conn));
                    }
                    da.FillSchema(schemaDt, SchemaType.Source);
                }
                return schemaDt;
            }

            private int GetSourceColumnIndex(int destinationIndex)
            {
                SqlBulkCopyColumnMapping cm = this.SqlBulkCopy.ColumnMappings.Cast<SqlBulkCopyColumnMapping>()
                    .FirstOrDefault(c => c.DestinationOrdinal == destinationIndex);
                if (cm == null)
                {
                    cm = this.SqlBulkCopy.ColumnMappings[destinationIndex];
                }
                return cm.SourceOrdinal < 0 ? this.SourceTable.Columns.IndexOf(cm.SourceColumn) : cm.SourceOrdinal;
            }
        }
    }
}
