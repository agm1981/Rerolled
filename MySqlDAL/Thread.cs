
namespace Common
{
    public class Thread
    {
        public string CombinedName => $"{OldForumName}____{ThreadName}";

        public int ThreadId
        {
            get; set;
        }

        public string ThreadName
        {
            get;
            set;
        }

        public string OldForumName
        {
            get;
            set;
        }
        public int? NewForumId
        {
            get;
            set;
        }

        public string UserNameStarter
        {
            get;
            set;
        }

        public int? NewThreadId
        {
            get;
            set;
        }


        public override string ToString()
        {
            return $"ThreadId: {ThreadId}, ThreadName: {ThreadName}, UserName: {UserNameStarter}";
        }
    }
}
