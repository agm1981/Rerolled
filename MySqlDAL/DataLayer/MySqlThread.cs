namespace Common.DataLayer
{
    public class MySqlThread
    {
        public string Title
        {
            get;
            set;
        }

        public int NodeId
        {
            get;
            set;
        }

        public int ThreadId
        {
            get;
            set;
        }

        public int StarterUserId
        {
            get;
            set;
        }

        public string StarterUserName
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"Title: {Title}, ForumId: {NodeId}, ThreadId: {ThreadId}, StarterUserId: {StarterUserId}";
        }
    }
}