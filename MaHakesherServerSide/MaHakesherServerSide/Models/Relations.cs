using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaHakesherServerSide.Models
{
    public class Relations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public string Destination { get; set; }
    }
}
