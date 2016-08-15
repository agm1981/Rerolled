namespace Common
{
    public class PostImported
    {
        public int OldPostId
        {
            get;
            set;
        }

        public int NewPostId
        {
            get;
            set;
        }

        public int NewThreadId
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"OldPostId: {OldPostId}, NewPostId: {NewPostId}";
        }
    }
}