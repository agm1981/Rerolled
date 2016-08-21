namespace Common.DataLayer
{
    public abstract class MySqlBaseRepository
    {
        protected MySqlHelper sqlH { get; set; }

        protected MySqlBaseRepository()
            : this(new MySqlHelper("server=localhost;user=root;database=xenforo;password=cmgonzalez;Shared Memory Name=MYSQL;"))
        {
        }

        protected MySqlBaseRepository(string connectionString)
            : this(new MySqlHelper(connectionString))
        {
        }

        protected MySqlBaseRepository(MySqlHelper sqlHelper)
        {
            this.sqlH = sqlHelper;
        }
        

    }
}