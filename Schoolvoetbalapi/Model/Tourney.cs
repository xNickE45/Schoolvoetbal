using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Schoolvoetbalapi.Model
{
    public class Tourney
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public List<Match> Matches { get; set; }
        public string Name { get; set; }
    }
}
