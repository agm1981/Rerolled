namespace Common.DataLayer
{
    public class MySqlPost
    {
        public int PostId
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }

        public int ThreadId
        {
            get;set;
        }

        public int UserId
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public int PostDate
        {
            get;
            set;
        }

        public int Position
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"Position: {Position}, PostDate: {PostDate}, ThreadId: {ThreadId}, UserId: {UserId}, UserName: {UserName}";
        }

        protected bool Equals(MySqlPost other)
        {
            return PostId == other.PostId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MySqlPost) obj);
        }

        public override int GetHashCode()
        {
            return PostId*1252;
        }

        //CREATE DEFINER=`root`@`localhost` PROCEDURE `InsertPost`(in threadId int, in userId int, in userna varchar(50), in postDate int, in content mediumtext, in position int, out postId int )
    }
}