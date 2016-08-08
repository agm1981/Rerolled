using System.Data.SqlClient;

namespace Common.DataLayer
{
    internal class ConnectionUtility
    {
        // change this name to use a different event source
        public const string EventLogSource = "ConnectionMonitoring";
        
        // Repeat seconds is how often the event log is written to:
        // the default is 1 hour.
        public const int RepeatSeconds = 3600;

        // this is how many seconds the connection must be open
        // before it is written to the EventLog.
        // the default is 3 minutes
        public const int OpenSeconds = 180;

        // change this to false to disable automatic logging
        public const bool UseLogging = false;
        

        private ConnectionUtility()
        {
        }

        // Returns (and creates if need be ) a ConnectionMonitor that is 
        // stored in the HttpContext.Application member.  
        public static ConnectionMonitor Monitor
        {
            get
            {
                if (true)
                    return null;

                //ConnectionMonitor monitor = null;
                //HttpContext context = HttpContext.Current;

                //if ( context != null )
                //{
                //    HttpApplicationState app = context.Application;
                //    app.Lock();

                //    try
                //    {
                //        monitor = app[ConnectionMonitor.CacheName] as ConnectionMonitor;

                //        if ( monitor == null )
                //        {
                //            // create the monitor and add it to the HttpApplicationState
                //            monitor = new ConnectionMonitor();
                //            app[ConnectionMonitor.CacheName] = monitor;

                //            if ( UseLogging )
                //            {
                //                // create an EventLog to write to 
                //                // keep in mind you'll need appropriate permissions to do this
                //                // see the HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog
                //                // registry key

                //                // if the event source doesn't exist, create it
                //                if( !EventLog.SourceExists( EventLogSource ) )
                //                {
                //                    EventLog.CreateEventSource( EventLogSource, "Application" );
                //                }
                        
                //                EventLog logger = new EventLog();
                //                logger.Source =  EventLogSource;

                //                // Enable automatic logging on the monitor -
                //                monitor.SetLogging( logger, RepeatSeconds, OpenSeconds );
                //                logger.WriteEntry( "Connection Monitoring Started", System.Diagnostics.EventLogEntryType.Information );                        
                //            }

                //        }                        
                //    }
                //    finally
                //    {
                //        app.UnLock();
                //    }

                //    if ( monitor !=  null )
                //    {
                //        // clear all closed connections from the monitor
                //        // this keeps the connection list from getting too large
                //        monitor.Flush();
                //    }
                //}

                //return monitor;
            }

        }

        // utility function that constructs a connection
        // with the passed connectionstring
        // and adds it to the monitor
        public static SqlConnection CreateConnection( string connectionString )
        {
            SqlConnection connection = CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        // utility function that constructs a connection
        // and adds it to the monitor
        public static SqlConnection CreateConnection()
        {
            SqlConnection connection = new SqlConnection();
            ConnectionMonitor monitor = Monitor;

            if ( monitor != null )
            {
                monitor.Add( new ConnectionInfo( connection ) );
            }

            return connection;
        }
    }

}
