using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySqlDAL
{
    public class Thread
    {

        public int ThreadId
        {
            get; set;
        }

        [Required]
        [StringLength(400)]
        public string ThreadName
        {
            get; set;
        }

        //[Required]
        [StringLength(50)]
        public string UserName
        {
            get; set;
        }

        public bool? IsThreadPoll
        {
            get; set;
        }
       


        public override string ToString()
        {
            return $"ThreadId: {ThreadId}, ThreadName: {ThreadName}, UserName: {UserName}, IsThreadPoll: {IsThreadPoll}";
        }
    }
}
