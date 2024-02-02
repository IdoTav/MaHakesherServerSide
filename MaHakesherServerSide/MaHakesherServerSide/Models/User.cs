using System.ComponentModel.DataAnnotations;
namespace MaHakesherServerSide.Models
{
    public class User
    {
        public User(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
            this.SourceAndDest = new List<Relations>();
        }

        [Key]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public List<Relations>? SourceAndDest { get; set; } 

    }
}
