using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Common.DataLayer
{
    /// <summary>
    /// Execute multiple sql queries in a batch
    /// </summary>
    public class MultiSet : IDisposable
    {
        private IDataReader DataReader { get; set; }
        private bool HasNextResultSet { get; set; }
        private DisposeHelper DisposeHelper { get; set; }

        public void ExecuteSet(Action<IDataReader> map)
        {
            this.ExecuteSet("", map);
        }
        public void ExecuteSet(string name, Action<IDataReader> map)
        {
            this.ExecuteSet(name, dr =>
            {
                map(dr);
                return 0;
            }).ToArray(); // Enumerate so query runs
        }

        public T ExecuteSingle<T>(Func<IDataReader, T> map)
        {
            if (!this.HasNextResultSet) throw new InvalidOperationException("No next result!");
            if (!this.DataReader.Read()) return default(T);
            var t = map(this.DataReader);
            this.HasNextResultSet = this.DataReader.NextResult();
            return t;
        }

        public IEnumerable<T> ExecuteSet<T>(Func<IDataReader, T> map)
        {
            return this.ExecuteSet("", map);
        }
        public IEnumerable<T> ExecuteSet<T>(string name, Func<IDataReader, T> map)
        {
            return this.ExecuteSet(name, map, false);
        }
        public IEnumerable<T> ExecuteSet<T>(Func<IDataReader, T> map, bool suppressDefaultValue) {
            return this.ExecuteSet("", map, suppressDefaultValue);
        }
        public IEnumerable<T> ExecuteSet<T>(string name, Func<IDataReader, T> map, bool suppressDefaultValue)
        {
            string inMapExceptionText = string.IsNullOrEmpty(name) ? "" : string.Format("map \"{0}\"", name);
            if (!this.HasNextResultSet) throw new InvalidOperationException("No next result!" + inMapExceptionText);
            var comparer = EqualityComparer<T>.Default;
            while (this.DataReader.Read())
            {
                T value;
                try {
                    value = map(this.DataReader);
                } 
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(inMapExceptionText)) throw;
                    throw new InvalidOperationException("Mapping exception in " + inMapExceptionText + ": " + ex.Message, ex);
                }
                if (suppressDefaultValue && comparer.Equals(value, default(T))) continue;
                yield return value;
            }
            try
            {
                this.HasNextResultSet = this.DataReader.NextResult();
            }
            catch (DbException ex)
            {
                if (string.IsNullOrEmpty(inMapExceptionText)) throw;
                throw new DataException("SQL exception *after* " + inMapExceptionText + ": " + ex.Message, ex);
            }
        }

        internal MultiSet(IDataReader dr, DisposeHelper d)
        {
            this.DataReader = dr;
            this.DisposeHelper = d;
            this.HasNextResultSet = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.DisposeHelper != null)
                {
                    this.DisposeHelper.Dispose();
                    this.DisposeHelper = null;
                }
            }
        }
    }
}
