using System.ComponentModel.DataAnnotations;

namespace SpaceMarket.Api.Models
{
    public class Logs
    {
        [Key]
        public int Log_Id { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public DateTime EventTime { get; set; }

        public int UserId { get; set; }
        public Users User { get; set; }
    }
}
