using System.ComponentModel.DataAnnotations;

namespace SpaceMarket.Api.Models
{
    public class Finance
    {
        [Key]
        public int Finance_Id { get; set; }
        public string FinanceType { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public DateTime EventTime { get; set; }

        public int UserId { get; set; }
        public Users User { get; set; }
    }
}