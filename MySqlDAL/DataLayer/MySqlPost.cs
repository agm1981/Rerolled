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

        //CREATE DEFINER=`root`@`localhost` PROCEDURE `InsertPost`(in threadId int, in userId int, in userna varchar(50), in postDate int, in content mediumtext, in position int, out postId int )
    }
}