namespace Schoolvoetbalapi.Model
{
    public class Team
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Points { get; set; }
        public int CreatorId { get; set; }
        public User Creator { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        public ICollection<Match> AwayMatches { get; set; } = new List<Match>();

    }
}
