using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace Common.DataLayer
{
    /// <summary>
    /// Strongly typed DBHelper for MS SQL Server
    /// </summary>
    public partial class SqlHelper : DbHelper<SqlConnection, SqlCommand, SqlParameterCollection>
    {
        public static string InClause<T>(string cmdTextFormat, IEnumerable<T> values, params object[] additionalArgs)
        {
            if (values == null || !values.Any()) values = new T[] { default(T) };
            return string.Format(cmdTextFormat, additionalArgs.Prepend(values.Select((i, n) => "@p" + n.ToString()).Join(", ")).ToArray());
        }

        public static IEnumerable<SqlParameter> InParameters<T>(IEnumerable<T> values) 
        {
            if (values == null || !values.Any()) values = new T[] { default(T) };
            int index = 0;
            foreach (var v in values)
            {
                yield return new SqlParameter("@p" + index++.ToString(), v.DBNullIfDefault());
            }
        }

        public static void InParameters<T>(SqlParameterCollection c, IEnumerable<T> values)
        {
            if (values == null || !values.Any()) values = new T[] { default(T) };
            values.Each((v, n) => c.AddWithValue("@p" + n.ToString(), v.DBNullIfDefault()));
        }

        public static string BuildTrustedConnectionString(string serverName, string databaseName)
        {
            var s = new SqlConnectionStringBuilder();
            s.DataSource = serverName;
            s.InitialCatalog = databaseName;
            s.IntegratedSecurity = true;
            return s.ToString();
        }
        public static string BuildConnectionString(string serverName, string databaseName, string userName, string password)
        {
            var s = new SqlConnectionStringBuilder();
            s.DataSource = serverName;
            s.InitialCatalog = databaseName;
            s.UserID = userName;
            s.Password = password;
            return s.ToString();
        }
        public static string GetConnectionString(string connectionStringKey)
        {
            return ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
        }

        public int DefaultCommandTimeout { get; set; }

        public SqlHelper(string connectionString)
            : base(connectionString)
        {
            this.DefaultCommandTimeout = 60;
        }

        public TransactionScope BeginTransaction()
        {
            var options = new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            };
            return new TransactionScope(TransactionScopeOption.Required, options);
        }

        protected override SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            ConnectionMonitor monitor = ConnectionUtility.Monitor;
            if (monitor != null)
            {
                monitor.Add(new ConnectionInfo(connection));
            }
            return connection;
        }

        protected override SqlCommand CreateCommand(string cmdText, SqlConnection connection)
        {
            return new SqlCommand(cmdText, connection) { CommandTimeout = DefaultCommandTimeout };
        }

        protected override IDbDataAdapter CreateDataAdapter(SqlCommand cmd)
        {
            return new SqlDataAdapter(cmd);
        }

        public SafeDataReader ExecuteSafeReader(CommandType cmdType, string cmdText, Action<SqlParameterCollection> paramAction)
        {
            var conn = this.SharedConnection ?? CreateConnection(this.ConnectionString);
            var cmd = CreateCommand(cmdText, conn);
            cmd.CommandType = cmdType;
            if (paramAction != null) paramAction(cmd.Parameters);

            if (this.SharedConnection != conn) conn.Open();
            
            var dr = cmd.ExecuteReader();

            var d = new DisposeHelper(() =>
            {
                dr.Dispose();
                cmd.Dispose();
                if (this.SharedConnection != conn)
                {
                    conn.Dispose();
                }
            });
            return new SafeDataReader(dr, d);
        }

        // Use a SqlDataReader to stream results
        protected SqlDataReader ExecuteReader(CommandType cmdType, string cmdText, Action<SqlParameterCollection> paramAction)
        {
            var conn = this.SharedConnection ?? CreateConnection(this.ConnectionString);
            var cmd = CreateCommand(cmdText, conn);
            cmd.CommandType = cmdType;
            if (paramAction != null) paramAction(cmd.Parameters);

            if (this.SharedConnection != conn) conn.Open();
            return cmd.ExecuteReader(); // WARNING: Leaks SqlCommand and SqlConnection IDisposables
        }

        public SqlMap<T> ExecuteMapSingle<T>(CommandType cmdType, string cmdText, Action<SqlParameterCollection> paramAction, Func<IDataReader, T> map)
        {
            var s = new SqlMapSet<T>(this);
            s.AddRoot(cmdText, paramAction, map, (dr, m) => !EqualityComparer<T>.Default.Equals(m, default(T))); // true if it's not default value (since there should only be 1 root)
            return new SqlMap<T>(s);
        }

        public SqlMapSet<T> ExecuteMapSet<T>(CommandType cmdType, string cmdText, Action<SqlParameterCollection> paramAction, Func<IDataReader, T> map)
        {
            return this.ExecuteMapSet(cmdType, cmdText, paramAction, map, null);
        }
        public SqlMapSet<T> ExecuteMapSet<T>(CommandType cmdType, string cmdText, Action<SqlParameterCollection> paramAction, Func<IDataReader, T> map, Func<IDataReader, T, bool> defaultPredicate)
        {
            var s = new SqlMapSet<T>(this);
            s.AddRoot(cmdText, paramAction, map, defaultPredicate);
            return s;
        }

        // Bulk copy implementations
        public void BulkCopy(SqlDataReader dr, string destinationTable, int batchSize, ColumnMatchingMode mode)
        {
            var conn = this.SharedConnection ?? CreateConnection(this.ConnectionString);
            try
            {
                using (var bc = new SqlBulkCopy(conn))
                {
                    bc.DestinationTableName = destinationTable;
                    bc.BatchSize = batchSize;
                    bc.NotifyAfter = batchSize;

                    switch (mode)
                    {
                        case ColumnMatchingMode.Ordinal:
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                bc.ColumnMappings.Add(i, i);
                            }
                            break;
                        case ColumnMatchingMode.CaseSensitiveName:
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                string columnName = dr.GetName(i);
                                bc.ColumnMappings.Add(columnName, columnName);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("mode");
                    }

                    int totalRows = 0;
                    bc.SqlRowsCopied += (s, e) =>
                    {
                        Trace.TraceInformation("{0} rows copied...", totalRows += batchSize);
                    };

                    if (this.SharedConnection != conn) conn.Open();
                    try
                    {
                        bc.WriteToServer(dr);
                    }
                    finally
                    {
                        if (this.SharedConnection != conn) conn.Close();
                    }
                }
            }
            finally
            {
                if (this.SharedConnection != conn) conn.Dispose();
            }
        }
        public void BulkCopy(DataTable dt, string destinationTable)
        {
            BulkCopy(dt, destinationTable, ColumnMatchingMode.Ordinal);
        }
        public void BulkCopy(DataTable dt, string destinationTable, ColumnMatchingMode mode)
        {
            var c = new List<SqlBulkCopyColumnMapping>(dt.Columns.Count);
            switch (mode)
            {
                case ColumnMatchingMode.Ordinal:
                    foreach (DataColumn dc in dt.Columns)
                    {
                        c.Add(new SqlBulkCopyColumnMapping(dc.Ordinal, dc.Ordinal));
                    }
                    break;
                case ColumnMatchingMode.CaseSensitiveName:
                    foreach (DataColumn dc in dt.Columns)
                    {
                        c.Add(new SqlBulkCopyColumnMapping(dc.ColumnName, dc.ColumnName));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
            BulkCopy(dt, destinationTable, c.ToArray());
        }
        public void BulkCopy(DataTable dt, string destinationTable, params string[] destinationColumnNames)
        {
            var columnMaps = destinationColumnNames.Select((s, i) => new SqlBulkCopyColumnMapping(i, s));
            BulkCopy(dt, destinationTable, columnMaps.ToArray());
        }
        public void BulkCopy(DataTable dt, string destinationTable, params SqlBulkCopyColumnMapping[] columnMappings)
        {
            var conn = this.SharedConnection ?? CreateConnection(this.ConnectionString);
            try
            {
                using (var bc = new SqlBulkCopy(conn))
                {
                    bc.DestinationTableName = destinationTable;
                    foreach (var columnMapping in columnMappings)
                    {
                        if (columnMapping != null)
                        {
                            bc.ColumnMappings.Add(columnMapping);
                        }
                    }

                    int totalRows = dt.Rows.Count;
                    bc.NotifyAfter = totalRows / 10;
                    bc.SqlRowsCopied += (s, e) =>
                    {
                        Trace.TraceInformation("{0}/{1} rows copied...", e.RowsCopied, totalRows);
                    };

                    if (this.SharedConnection != conn) conn.Open();

                    try
                    {
                        bc.WriteToServer(dt);
                    }
                    catch (Exception ex)
                    {
                        var e = new SqlBulkCopyExceptionHelper(this.ConnectionString, bc, ex, dt);
                        Exception newEx;
                        if (e.TryHandle(out newEx))
                        {
                            throw newEx;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if (this.SharedConnection != conn) conn.Close();
                    }
                }
            }
            finally
            {
                if (this.SharedConnection != conn) conn.Dispose();
            }
        }

        // Use SqlAgent to start a fire and forget task
        // h/t http://www.motobit.com/tips/detpg_async-execute-sql/
        public Guid FireAndForget(string cmdText, string dbName = "master")
        {
            var jobGuid = this.ExecuteNonQuery(CommandType.StoredProcedure, "msdb.dbo.sp_add_job", c =>
            {
                c.AddWithValue("@job_name", "FireAndForget: " + Guid.NewGuid().ToString());
                c.AddWithValue("@owner_login_name", "sa");
                c.Add("@job_id", SqlDbType.UniqueIdentifier, 16).Direction = ParameterDirection.Output;
            }, c => (Guid)c.Parameters["@job_id"].Value);
            this.ExecuteNonQuery(CommandType.StoredProcedure, "msdb.dbo.sp_add_jobserver", c => c.AddWithValue("@job_id", jobGuid));
            this.ExecuteNonQuery(CommandType.StoredProcedure, "msdb.dbo.sp_add_jobstep", c =>
            {
                c.AddWithValue("@job_id", jobGuid);
                c.AddWithValue("@step_name", "Run");
                c.AddWithValue("@command", cmdText);
                c.AddWithValue("@database_name", dbName);
                c.AddWithValue("@on_success_action", 3);
            });
            this.ExecuteNonQuery(CommandType.StoredProcedure, "msdb.dbo.sp_add_jobstep", c =>
            {
                c.AddWithValue("@job_id", jobGuid);
                c.AddWithValue("@step_name", "Delete");
                c.AddWithValue("@command", string.Format("EXEC msdbp.dbo.sp_delete_job @job_name='FireAndForget: {0}'", jobGuid));
                c.AddWithValue("@database_name", dbName);
                c.AddWithValue("@on_success_action", 3);
            });
            this.ExecuteNonQuery(CommandType.StoredProcedure, "msdb.dbo.sp_start_job", c => c.AddWithValue("@job_id", jobGuid));
            return jobGuid;
        }
    }
}