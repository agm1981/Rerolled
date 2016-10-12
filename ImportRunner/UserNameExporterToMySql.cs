using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.DataLayer;

namespace ImportRunner
{
    public class UserNameExporterToMySql
    {
        public const string name_suffix = "_foh";
        private HashSet<UsersSaved> UserNamesExported
        {
            get;
            set;
        }

        public void Start()
        {
            AllPostsRepository rep = new AllPostsRepository();
            AllUserMySqlRepository mySqlRep = new AllUserMySqlRepository();
            
            HashSet<string> usersToImportWithOutSuffix = new HashSet<string>(rep.GetAllUsersFromPostTable()); 
            
            foreach (string usernameToSaved in usersToImportWithOutSuffix)
            {
                MySqlUser toInsert = new MySqlUser
                {
                    UserName = usernameToSaved + name_suffix
                };
                mySqlRep.InsertUserName(toInsert);
            }
        }
    }
}
