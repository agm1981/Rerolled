using System;
using System.Collections.Generic;
using System.Data;

namespace Common.DataLayer
{
    public class AllUsersSavedRepository : BaseRepository
    {
        private readonly Func<IDataReader, UsersSaved > _mapUsersSaved = dr => new UsersSaved
        { 
           Username = dr.Get<string>("Username"),
           NewUsername = dr.Get<string>("NewUsername"),
           NewUserId = dr.Get<int>("NewUserId"),
            
        };

        //private Func<IDataReader, int> mapIds = dr => dr.Get<int>("PostId");

      

        public IEnumerable<string> GetPendingUsersForImport()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"select distinct p.username from Posts  p left join userssaved  s on p.UserName = s.userName
where s.userName is null",
                null,
                c=> c.Get<string>("username")
            );
        }

        public void Save(UsersSaved item)
        {
            string sql = @"INSERT INTO [dbo].[UsersSaved]
                    ([Username]
                       ,[NewUsername]
                       ,[NewUserId])
                    VALUES
                      (@Username
                       ,@NewUsername
                       ,@NewUserId)";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@Username", item.Username);
                           c.AddWithValue("@NewUsername", item.NewUsername);
                           c.AddWithValue("@NewUserId", item.NewUserId);
                       }
                   );
        }

        public void Save(IEnumerable<UsersSaved> users)
        {
            foreach (UsersSaved user in users)
            {
                Save(user);
            }
        }

        
    }
}

