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

        [ForeignKey("Thread")]
        public int ThreadId
        {
            get; set;
        }

        [Required]
        [StringLength(50)]
        [ForeignKey("CreatedBy")]
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

        public virtual Thread Thread { get; set; }

        public virtual UserName CreatedBy { get; set; }

        public override string ToString()
        {
            return $"PostId: {PostId}, ThreadId: {ThreadId}, UserName: {UserName}, PostContent: {PostContent}, PostDate: {PostDate}, Thread: {Thread}, CreatedBy: {CreatedBy}";
        }
    }
}
