using System.ComponentModel.DataAnnotations;

namespace Common
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
