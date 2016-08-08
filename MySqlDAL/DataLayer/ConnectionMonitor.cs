using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Common.DataLayer
{
    // contains information about each connection that is added
    // to the ConnectionMonitor
    internal class ConnectionInfo
    {
        #region Private Members
        string mStackTrace = String.Empty;
        SqlConnection mConnection = null;
        DateTime mOpened = DateTime.MinValue;
        DateTime mClosed = DateTime.MinValue;
        #endregion

        #region Contructors
        public ConnectionInfo( SqlConnection connection )
        {
            mConnection = connection;

            // hook into the StateChange event so that know when the connection
            // is opened and closed
            connection.StateChange += new StateChangeEventHandler( Connection_StateChange );
                
            // Check to see if the connection is already open
            if ( ( connection.State & ConnectionState.Open ) != 0 )
            {
                DoOpen();
            }

            // Capture the current stack trace so we can figure
            // out where unclosed connections originated
            mStackTrace = "";
            string[] v = GetStackTrace().Split('\n');
            for (int i = 2; i < v.Length && i < 7; i++)
                mStackTrace += v[i].Trim() + "\r\n"; 
        }
        #endregion

        #region Public Properties
        // returns true if the connection has ever been opened, false otherwise
        // note that the connection could be currently closed and this property
        // would still return true
        public bool Opened
        {
            get
            {
                return mOpened != DateTime.MinValue;
            }
        }

        // returns true if the connection has been closed, false otherwise
        public bool Closed
        {
            get
            {
                return mClosed != DateTime.MinValue;
            }
        }

        // returns true if the connection is currently open
        public bool IsOpen
        {
            get
            {
                return ( Opened && !Closed );
            }
        }

        // returns the amount of time the connection has been open
        public TimeSpan TimeOpen
        {
            get
            {
                TimeSpan span = new TimeSpan( 0 );

                if ( Opened && !Closed )
                {
                    span = span.Add( DateTime.Now - mOpened );
                }

                return span;
            }
        }


        #endregion

        #region Public Methods

        // if the connection is open, closes it.
        // return true if the connection was closed, false if was already 
        // closed before this method was called
        public bool Close()
        {
            bool result = false;

            if ( ( mConnection.State & ConnectionState.Open ) != 0 )
            {
                mConnection.Close();
                mConnection = null;
                result = true;
            }

            return result;
        }

        // Appends a status string to the StringBuilder based on the method arguments:
        // 
        // secondsOpen - Only append a status string if the connection has been open for this many seconds
        // openOnly - Only append a status string if the connection is currently open
        // stackTrace - if true appends the stack trace to the status string
        //
        // returns true if a status string was appended to the StringBuilder, false otherwise
        public bool Status( StringBuilder sb, int secondsOpen, bool openOnly, bool stackTrace )
        {
            bool result;
            TimeSpan span = TimeOpen;

            if ( !IsOpen && openOnly )
            {
                result = false;
            }
            else if ( (int)span.TotalSeconds < secondsOpen )
            {
                result = false;
            }
            else
            {
                result = true;
                // Connection Open 
                sb.AppendFormat( "Connection Time Elapsed: {0}\r\n", span.ToString() );

                // if your connection string doesn't contain password info, you 
                // might want to enable the following line:
                //sb.AppendFormat( "Connection String: {0}\r\n", mConnection.ConnectionString );

                if ( stackTrace )
                {
                    sb.AppendFormat( "{0}\r\n", mStackTrace );
                    sb.Append( "\r\n" );
                }
                
            }

            return result;
        }

        #endregion

        #region Private Methods
        void DoOpen()
        {
            mOpened = DateTime.Now;
            mClosed = DateTime.MinValue;
        }

        void DoClose()
        {
            mClosed = DateTime.Now;
        }

        // Stores a stack trace in the mStackTrace member.
        string GetStackTrace()
        {
            StackTrace trace = new StackTrace( true );
            StringBuilder sb = new StringBuilder();

            for ( int i = 1; i < trace.FrameCount; i++ )
            {
                StackFrame frame = trace.GetFrame( i );
                sb.AppendFormat( "    at {0}.{1}\r\n", frame.GetMethod().DeclaringType, frame.GetMethod().Name );
            }

            return sb.ToString();

        }
        #endregion

        #region Events
        // called when the connection is opened or closed
        void Connection_StateChange( object sender, StateChangeEventArgs e )
        {
            if ( ( e.CurrentState & ConnectionState.Open ) != 0 )
            {
                DoOpen();
            }

            if ( e.CurrentState == ConnectionState.Closed  )
            {
                DoClose();
            }
        }

        #endregion
    }

    internal class ConnectionMonitor 
    {
        public const string CacheName = "_ConnectionMonitor";

        #region Private Members
        ArrayList mList = new ArrayList();

        // these members used for logging only
        EventLog mLogger = null;
        Timer mTimer = null;
        int mSecondsOpen = 0;
        #endregion

        #region Constructors
        public ConnectionMonitor()
        {
        }
        #endregion

        #region Public Properies
        // the total number of Items in the list
        public int Count
        {
            get
            {
                lock ( this )
                {
                    return mList.Count;
                }
            }
        }

        // Returns the EventLog that is being used to log entries 
        // Can be null
        // (See SetLogging)
        public EventLog Logger
        {
            get
            {
                return mLogger;
            }
        }

        // Returns the ConnectionInfo item at the given index
        public ConnectionInfo this[int idx]
        {
            get
            {
                lock ( this )
                {
                    return (ConnectionInfo)mList[idx];
                }
            }
        }

        // the number of seconds a connection must be opened before it
        // will be logged (see SetLogging)
        public int SecondsOpen
        {
            get
            {
                return mSecondsOpen;
            }
            set
            {
                mSecondsOpen = value;
            }
        }
        #endregion

        #region Public Methods

        // Adds a new connection to the list
        public void Add( ConnectionInfo item )
        {
            lock ( this )
            {
                mList.Add( item );
            }
        }

        // Removes a connection from the list
        public void Remove( ConnectionInfo item )
        {
            lock ( this )
            {
                mList.Remove( item );
            }
        }

        // Removes a connection from the list
        public void RemoveAt( int idx )
        {
            lock ( this )
            {
                mList.RemoveAt( idx );
            }
        }

        // Removes all connections from the list
        public void Clear()
        {
            lock ( this )
            {
                mList.Clear();
            }
        }


        // Enables automatic logging to the EventLog passed as the first argument
        // repeatSeconds - the number of seconds that should pass between log writes
        // secondsOpen - the number of seconds that the connection should be open before it is logged
        public void SetLogging( EventLog logger, int repeatSeconds, int secondsOpen )
        {
            lock ( this )
            {
                if ( mTimer != null )
                {
                    mTimer.Dispose();
                    mTimer = null;
                }

                mLogger = logger;
                mSecondsOpen = secondsOpen;

                if ( mLogger != null && repeatSeconds != 0 )
                {
                    int milliseconds = repeatSeconds * 1000;
                    mTimer = new Timer( new TimerCallback( TimerLog ), null, milliseconds, milliseconds );
                }
            }
        }

        // when automatic logging is being used (See the SetLogging method)
        // forces the event log to be written to 
        public void ForceLogging()
        {
            TimerLog( null );
        }

        // removes all closed connections from the list
        public void Flush()
        {
            lock ( this )
            {
                for ( int idx = mList.Count - 1; idx >= 0; idx-- )
                {
                    ConnectionInfo item = (ConnectionInfo)mList[idx];

                    if ( item.Closed )
                    {
                        mList.RemoveAt( idx );
                    }
                }
            }
        }

        // Writes to the event log any connections that meet the requirements passed as arguments
        // to this method:
        //
        // secondsOpen - the number of seconds the connection must be open before it is written
        // openOnly - writes the entry only if the connection is currently open
        // stackTrace - indicates if the stack trace should be added to the log entry
        //
        // returns the number of entries written
        public int Status( EventLog log, int secondsOpen, bool openOnly, bool stackTrace )
        {
            int result = 0;
            StringBuilder sb = new StringBuilder();

            lock ( this )
            {
                foreach ( ConnectionInfo info in mList )
                {
                    sb.Remove( 0, sb.Length );

                    if ( info.Status( sb, secondsOpen, openOnly, stackTrace ) )
                    {
                        result++;
                        log.WriteEntry( sb.ToString(), EventLogEntryType.FailureAudit );
                    }
                }
            }

            return result;

        }

        // Returns a status string of any connections that meet the requirements passed as arguments
        // to this method:
        //
        // secondsOpen - the number of seconds the connection must be open before it is written
        // openOnly - writes the entry only if the connection is currently open
        // stackTrace - indicates if the stack trace should be added to the log entry
        public string Status( int secondsOpen, bool openOnly, bool stackTrace )
        {
            StringBuilder sb = new StringBuilder();

            lock ( this )
            {
                foreach ( ConnectionInfo info in mList )
                {
                    info.Status( sb, secondsOpen, openOnly, stackTrace );
                }
            }

            return sb.ToString();
        }

        // Closes all connections that have been open for 'secondsOpen'
        // returns a count of closed connections
        public int Close( int secondsOpen )
        {
            int count = 0;

            lock ( this )
            {
                foreach ( ConnectionInfo info in mList )
                {
                    if ( info.IsOpen )
                    {
                        if ( info.TimeOpen.TotalSeconds > secondsOpen )
                        {
                            info.Close();
                            count++;
                        }
                    }
                }
            }

            return count;
        }
        #endregion

        #region Private Methods
        // Timer callback 
        void TimerLog( object state )
        {
            lock ( this )
            {
                if ( mLogger == null )
                {
                    // oops no logger anymore!
                    return;
                }
            }
            
            int count = Status( mLogger, mSecondsOpen, true, true );

            if ( count == 0 )
            {
                lock ( this )
                {
                    mLogger.WriteEntry( "Connection Monitor: No open connections found", EventLogEntryType.SuccessAudit );
                }
            }
        }
        #endregion

    }
}
