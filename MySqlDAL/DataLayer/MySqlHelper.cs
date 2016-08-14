using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace Common.DataLayer
{
    public class MySqlHelper : DbHelper<MySqlConnection, MySqlCommand, MySqlParameterCollection>
    {
        public MySqlHelper(string connectionString)
            : base(connectionString)
        {
            //this.DefaultCommandTimeout = 60;
        }

        protected override MySqlCommand CreateCommand(string cmdText, MySqlConnection connection)
        {
            return new MySqlCommand(cmdText, connection) {CommandTimeout = 120};
        }

        protected override MySqlConnection CreateConnection(string connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }

        protected override IDbDataAdapter CreateDataAdapter(MySqlCommand cmd)
        {
            return new MySqlDataAdapter(cmd);
        }
    }
}

//    private readonly string connectionstring;


        //    public MySqlHelper( string connstring)
        //    {
        //        connectionstring = connstring;
        //    }
        //    public virtual T ExecuteNonQuery<T>(CommandType cmdType, string cmdText, Action<MySqlCommand> cmdAction, Action<MySqlParameterCollection> paramAction, Func<MySqlCommand, T> returnValue)
        //    {
        //        if (returnValue == null) throw new ArgumentNullException("returnValue");

        //        MySqlConnection connection = new MySqlConnection { ConnectionString = connectionstring };
        //        try
        //        {
        //            using (var cmd = new MySqlCommand
        //            {
        //                Connection = connection,
        //                CommandText = cmdText,
        //                CommandType = cmdType
        //            })
        //            {
        //                cmd.CommandType = cmdType;
        //                if (cmdAction != null) cmdAction(cmd);
        //                if (paramAction != null) paramAction((TParameterCollection)cmd.Parameters);
        //                foreach (IDataParameter p in cmd.Parameters)
        //                {
        //                    if (p.Value == null) p.Value = DBNull.Value;
        //                }

        //                if (this.SharedConnection != conn) conn.Open();
        //                try
        //                {
        //                    cmd.ExecuteNonQuery();
        //                    return returnValue(cmd);
        //                }
        //                finally
        //                {
        //                    if (this.SharedConnection != conn) conn.Close();
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            if (this.SharedConnection != conn) conn.Dispose();
        //        }
        //    }



        //    public virtual IEnumerable<T> ExecuteSet<T>(CommandType cmdType, string cmdText, Action<MySqlParameterCollection> paramAction, Func<IDataReader, T> map)
        //    {
        //        MySqlConnection connection = new MySqlConnection { ConnectionString = connectionstring };
        //        try
        //        {
        //            using (var cmd = new MySqlCommand
        //            {
        //                Connection = connection,
        //                CommandText = cmdText,
        //                CommandType = cmdType
        //            })
        //            {
        //                paramAction?.Invoke(cmd.Parameters);

        //                connection.Open();
        //                try
        //                {
        //                    using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
        //                    {
        //                        while (dr.Read())
        //                        {
        //                            yield return map(dr);
        //                        }
        //                    }
        //                }
        //                finally
        //                {
        //                    connection.Close();
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            connection.Dispose();
        //        }
        //    }
        //}

    
