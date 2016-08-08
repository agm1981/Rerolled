using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Common.DataLayer
{
    public partial class SqlHelper
    {
        /// <summary>
        /// Use a MultiSet to map collection of root entities with child collections
        /// </summary>
        /// <typeparam name="TRoot"></typeparam>
        public class SqlMapSet<TRoot>
        {
            private SqlHelper SqlHelper { get; set; }
            private IList<Tuple<string, Func<IDataReader, TRoot, bool>, Action<IDataReader, TRoot>>> Maps { get; set; }
            private IList<Action<TRoot>> PostActions { get; set; }
            private Func<IDataReader, TRoot> RootMap { get; set; }
            private Action<SqlParameterCollection> ParamAction { get; set; }
            private StringBuilder CommandText { get; set; }
            private Func<IDataReader, TRoot, bool> DefaultPredicate { get; set; }
            public bool SuppressDefaultValues { get; set; }

            internal SqlMapSet(SqlHelper sqlHelper)
            {
                this.SqlHelper = sqlHelper;
                this.Maps = new List<Tuple<string, Func<IDataReader, TRoot, bool>, Action<IDataReader, TRoot>>>();
                this.PostActions = new List<Action<TRoot>>();
                this.CommandText = new StringBuilder();
                this.SuppressDefaultValues = !typeof(TRoot).IsValueType;
            }

            // Add query for the root entities
            internal void AddRoot(string cmdText, Action<SqlParameterCollection> paramAction, Func<IDataReader, TRoot> rootMap, Func<IDataReader, TRoot, bool> defaultPredicate)
            {
                this.RootMap = rootMap;
                this.ParamAction = paramAction;
                this.DefaultPredicate = defaultPredicate;
                this.AddRootCommand(cmdText);
            }

            // Add post query mappings and actions
            public void After(Action<TRoot> action)
            {
                this.PostActions.Add(action);
            }

            // Add queries for dependent values and collections
            public void Add(Action<IDataReader, TRoot> map, string cmdText, Action<SqlParameterCollection> paramAction = null)
            {
                this.Add("", map, cmdText, paramAction);
            }
            public void Add(string name, Action<IDataReader, TRoot> map, string cmdText, Action<SqlParameterCollection> paramAction = null)
            {
                this.Add(name, this.DefaultPredicate, map, cmdText, paramAction);
            }
            public void Add(Func<IDataReader, TRoot, bool> predicate, Action<IDataReader, TRoot> map, string cmdText, Action<SqlParameterCollection> paramAction = null)
            {
                this.Add("", predicate, map, cmdText, paramAction);
            }
            public void Add(string name, Func<IDataReader, TRoot, bool> predicate, Action<IDataReader, TRoot> map, string cmdText, Action<SqlParameterCollection> paramAction = null)
            {
                this.Maps.Add(new Tuple<string, Func<IDataReader, TRoot, bool>, Action<IDataReader, TRoot>>(
                    name,
                    predicate,
                    map
                ));

                // TODO: Can rename parameters with index after adding (just loop through the ones added, change name and update cmdText) and replace cmdText so that maps can reuse same param names without conflict
                this.AppendCommand(cmdText);
                if (paramAction != null) this.AppendParameterAction(paramAction);
            }

            public IEnumerable<TRoot> Execute()
            {
                using (var multiSet = this.SqlHelper.ExecuteMultiSet(
                    CommandType.Text,
                    this.CommandText.ToString(),
                    this.ParamAction
                ))
                {
                    var roots = multiSet.ExecuteSet("root", this.RootMap, this.SuppressDefaultValues).ToArray();
                    var comparer = EqualityComparer<TRoot>.Default;

                    foreach (var map in this.Maps)
                    {
                        multiSet.ExecuteSet(
                            map.Item1,
                            dr =>
                            {
                                foreach (TRoot matchingRoot in roots.Where(r => map.Item2(dr, r)))
                                {
                                    map.Item3(dr, matchingRoot);
                                }
                            }
                        );
                    }
                    foreach (var action in this.PostActions)
                    {
                        foreach (var m in roots)
                        {
                            action(m);
                        }
                    }
                    return roots.Memoize();
                }
            }

            private void AppendCommand(string cmdText)
            {
                this.CommandText.Append(cmdText);
                if (!cmdText.EndsWith(";")) this.CommandText.Append(";");
                this.CommandText.AppendLine().AppendLine();
            }

            private void AppendParameterAction(Action<SqlParameterCollection> paramAction)
            {
                this.ParamAction += paramAction;
            }

            private void AddRootCommand(string cmdText)
            {
                if (!cmdText.EndsWith(";")) cmdText += ";";
                cmdText += Environment.NewLine + Environment.NewLine;
                this.CommandText.Insert(0, cmdText);
            }
        }
    }
}
