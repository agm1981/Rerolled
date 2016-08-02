using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySqlDAL
{


    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
        
        public override string ToString()
        {
            return $"PostId: {PostId}, UserName: {UserName}, PostContent: {PostContent}, PostDate: {PostDate}";
        }
    }
}
