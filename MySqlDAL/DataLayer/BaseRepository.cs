using System;
using System.Data;
using System.Data.SqlClient;

namespace Common.DataLayer
{
    public abstract class BaseRepository
    {
        protected SqlHelper sqlH { get; set; }

        protected BaseRepository()
            : this(SqlHelper.GetConnectionString("ConnString"))
        {
        }

        protected BaseRepository(string connectionString)
            : this(new SqlHelper(connectionString))
        {
        }

        protected BaseRepository(SqlHelper sqlHelper)
        {
            this.sqlH = sqlHelper;
        }
        protected bool IsDataBaseReadOnly()
        {
            var cs = SqlHelper.GetConnectionString("ConnString");
                SqlConnectionStringBuilder builder = 
                new SqlConnectionStringBuilder(cs);
            var dbName = builder["Database"];
            return sqlH.ExecuteScalar(CommandType.Text,
                @"SELECT is_read_only  FROM sys.databases WHERE name = @dbName",
                c => c.AddWithValue("@dbName", dbName),
                o => (o == null) ? false : (bool)o
            );
        }
        
    }
}
