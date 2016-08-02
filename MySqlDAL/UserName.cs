using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;


namespace MySqlDAL
{

    public class UserName
    {
     
        [Key]
        [Column("UserName")]
        [StringLength(50)]
        public string UserName1 { get; set; }


        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();

      
        public virtual ICollection<Thread> Threads { get; set; } = new HashSet<Thread>();

        public override string ToString()
        {
            return $"UserName1: {UserName1}, Posts: {Posts}, Threads: {Threads}";
        }
    }
}
