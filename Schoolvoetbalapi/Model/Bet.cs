namespace Schoolvoetbalapi.Model
{
    public class Bet
    {
        public int Id { get; set; }
        public User User { get; set; }
        public float MoneyBet { get; set; }
        public Team BetTeam { get; set; }
    }
}
