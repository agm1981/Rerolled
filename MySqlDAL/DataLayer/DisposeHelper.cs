using System;

namespace Common.DataLayer
{
    /// <summary>
    /// Class to call an Action when disposing.
    /// Useful for returning objects for use in using() clauses
    /// </summary>
    public class DisposeHelper : IDisposable
    {
        private Action OnDispose { get; set; }

        public DisposeHelper(Action onDispose)
        {
            this.OnDispose = onDispose;
        }

        public void Dispose()
        {
            this.OnDispose();
        }
    }
}
