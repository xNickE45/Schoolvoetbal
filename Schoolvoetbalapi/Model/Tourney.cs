namespace Schoolvoetbalapi.Model
{
    public class Tourney
    {
        public int Id { get; set; }
        public List<Match> Matches { get; set; }
        public string Name { get; set; }
    }
}
