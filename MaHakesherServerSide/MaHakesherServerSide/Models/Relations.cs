using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaHakesherServerSide.Models
{
    public class Relations
    {
        public Relations(string userName, string history)
        {
            this.UserName = userName;
            this.History = history;
        }

        [Key]
        public string UserName { get; set; }

        public string History { get; set; }
    }
}
