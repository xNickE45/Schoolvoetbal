namespace Schoolvoetbalapi.Model
{
    public class Match
    {
        public int Id { get; set; }
        public  Team Team1 { get; set; }
        public int TourneyId { get; set; } 
        public Team Team2 { get; set; }
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }
        public DateTime StartTime { get; set; }
        public bool Finished { get; set; }
        public List<Bet> Bets { get; set; }
    }
}
