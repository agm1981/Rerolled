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
    public class MySqlDataConnector
    {
        private MySqlConnection connection
        {
            get;
            set;
        }
        public MySqlDataConnector()
        {
            connection = new MySqlConnection {ConnectionString = "server=localhost;user=root;database=rererolled;port=3306;password=cmgonzalez;"};
        }

        public object ExecuteReader(CommandType commandType, string commandText, MySqlParameterCollection parameters )
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = connection,
                CommandText = commandText,
                CommandType = commandType
            };
            foreach (MySqlParameter mySqlParameter in parameters)
            {
                cmd.Parameters.Add(mySqlParameter);
            }

            cmd.ExecuteReader();

            connection.Close();

            return null;
        }
    }
}
