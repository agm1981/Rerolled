namespace Common.DataLayer
{
    public class MySqlUser
    {
        public string UserName
        {
            get;
            set;
        }

        public int UserId
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"UserName: {UserName}, UserId: {UserId}";
        }
    }
}