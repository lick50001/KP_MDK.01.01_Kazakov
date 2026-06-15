using System;

namespace Kazakov_KP_01._01.Models
{
    public class Finance
    {
        public int Finance_Id { get; set; }
        public string FinanceType { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public DateTime EventTime { get; set; }
        public int UserId { get; set; }
    }
}