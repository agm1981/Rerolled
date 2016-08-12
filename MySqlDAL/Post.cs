using System;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class Post
    {
        public int PostId
        {
            get; set;
        }
       
        [Required]
        [StringLength(50)]
        public string UserName
        {
            get; set;
        }

        [Required]
        public string PostContent
        {
            get; set;
        }

        public string NewPostContent
        {
            get; set;
        }


        [Required]
        public DateTime PostDate
        {
            get; set;
        }

        [Required]
        public string ThreadName
        {
            get;
            set;
        }

        public int? NewPostId
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"PostId: {PostId}, UserName: {UserName}, PostContentLength: {PostContent?.Length}, PostDate: {PostDate}";
        }
    }
}
