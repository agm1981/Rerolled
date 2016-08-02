using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;


namespace MySqlDAL
{

    public class UserProfile
    {
     
        [Key]
        [StringLength(50)]
        public string UserName { get; set; }


       

        public override string ToString()
        {
            return $"UserName: {UserName}";
        }
    }
}
