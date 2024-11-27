namespace Schoolvoetbalapi.Model
{
    public class Match
    {
        public int Id { get; set; }
        public int Team1Id { get; set; }
        public int Team2Id { get; set; }
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }
        public DateTime StartTime { get; set; }
        public bool Finished { get; set; }
    }
}
