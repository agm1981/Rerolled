namespace Common.DataLayer
{
    public abstract class MySqlBaseRepository
    {
        protected MySqlHelper sqlH { get; set; }

        protected MySqlBaseRepository()
            : this(new MySqlHelper("server=localhost;user=root;database=xenforo;port=3306;password=cmgonzalez;"))
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