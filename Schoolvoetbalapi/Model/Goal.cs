namespace Schoolvoetbalapi.Model
{
    public class Goal
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public User Player { get; set; }
        public int MatchId { get; set; }
        public Match Match { get; set; }
        public int Minute { get; set; }
    }
}
