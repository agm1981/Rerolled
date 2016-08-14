using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Common.DataLayer
{
    public class AllUserMySqlRepository : MySqlBaseRepository
    {
        private Func<IDataReader, MySqlUser> mapUser = dr => new MySqlUser
        {
            UserId = Convert.ToInt32(dr.Get<uint>("user_Id")),
            UserName = dr.Get<string>("userName"),
        };
        public IEnumerable<MySqlUser> GetAllUsersInMySql()
        {
            string sql = "Select userName, user_Id from xf_user;";
            return sqlH.ExecuteSet(
                CommandType.Text,
                sql,
                null,
                mapUser
            );
        }
        public void InsertUserName(MySqlUser user)
        {
            string sql = "InsertProfile";
            int userId = sqlH.ExecuteNonQuery<int>(
                CommandType.StoredProcedure,
                sql,
                c =>
                {
                    c.AddWithValue("@newName", user.UserName);
                    c.Add(new MySqlParameter("userId", MySqlDbType.Int32));
                    c["@userId"].Direction = ParameterDirection.Output;
                },
                c=> Convert.ToInt32(c.Parameters["@userId"].Value)
                
            );
            user.UserId = userId;
        }
    }
}
