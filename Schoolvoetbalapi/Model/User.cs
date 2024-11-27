using System.Security.Cryptography;
using System.Text;

namespace Schoolvoetbalapi.Model
{
    public class User
    {
        public User()
        {
            // Genereer een token door het e-mailadres en wachtwoord te hashen
            string toHash = Email + Password;
            Token = ComputeSha256Hash(toHash);
            
            
        }
        public bool CheckPassword(string unhashedPassword)
        {
            string hashedPassword = ComputeSha256Hash(unhashedPassword);
            return hashedPassword == Password;
          
        }
        public static string ComputeSha256Hash(string rawData)
        {
            // Maak een SHA256-hashobject aan
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Bereken de hash - retourneert een byte-array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Converteer de byte-array naar een hexadecimale string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // Voeg elke byte toe als hexadecimale string
                }
                return builder.ToString(); // Retourneer de resulterende hash
            }
        }

        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public bool Admin { get; set; }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
        public ICollection<Team> CreatedTeams { get; set; } = new List<Team>();
        public ICollection<Match> RefereedMatches { get; set; } = new List<Match>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    }
}
