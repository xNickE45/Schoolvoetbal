namespace Schoolvoetbalapi.Model
{
    public class Match
    {
        public int Id { get; set; }
        public int Team1Id { get; set; }
        public Team Team1 { get; set; }
        public int Team2Id { get; set; }
        public Team Team2 { get; set; }
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }
        public required string Field { get; set; }
        public int RefereeId { get; set; }
        public User Referee { get; set; }
        public required string Time { get; set; }
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    }
}

