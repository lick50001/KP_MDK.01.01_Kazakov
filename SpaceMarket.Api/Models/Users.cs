using System.ComponentModel.DataAnnotations;

namespace SpaceMarket.Api.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PwdHash { get; set; }
        public string LevelRoot { get; set; }
        public List<Items> Items { get; set; } = new();
        public List<Logs> Logs { get; set; } = new();

        public Users() { }

        public Users(int userId, string userName, string pwdHash, string level)
        {
            UserId = userId;
            UserName = userName;
            PwdHash = pwdHash;
            LevelRoot = level;
        }
    }
}
