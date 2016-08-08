using System;
using System.Data;
using System.Linq;

namespace Common.DataLayer
{
    public partial class SqlHelper
    {
        /// <summary>
        /// Use a SqlSetMap to only return a single entity
        /// </summary>
        /// <typeparam name="TRoot"></typeparam>
        public class SqlMap<TRoot>
        {
            private SqlMapSet<TRoot> SqlMapSet { get; set; }

            internal SqlMap(SqlMapSet<TRoot> sqlMapSet)
            {
                this.SqlMapSet = sqlMapSet;
            }

            public void Add(Action<IDataReader, TRoot> map, string cmdText)
            {
                this.SqlMapSet.Add(map, cmdText);
            }

            public TRoot Execute()
            {
                return this.SqlMapSet.Execute().SingleOrDefault();
            }
        }
    }
}
