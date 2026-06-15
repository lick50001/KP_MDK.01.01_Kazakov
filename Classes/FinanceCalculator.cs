using Kazakov_KP_01._01.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kazakov_KP_01._01.Classes
{
    public class FinanceSummary
    {
        public decimal TotalProfit { get; set; }
        public decimal Profit24h { get; set; }
        public decimal ProfitSession { get; set; }
    }

    public static class FinanceCalculator
    {
        public static FinanceSummary Calculate(List<Finance> records)
        {
            var now = DateTime.Now;
            var sessionStart = SessionManager.SessionStartTime;

            System.Diagnostics.Debug.WriteLine($"=== DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Now: {now}");
            System.Diagnostics.Debug.WriteLine($"SessionStart: {sessionStart}");
            foreach (var r in records)
                System.Diagnostics.Debug.WriteLine($"Record: EventTime={r.EventTime}, Amount={r.Amount}, >= session? {r.EventTime >= sessionStart}");

            decimal total = records.Sum(f => f.Amount);

            decimal last24h = records
                .Where(f => f.EventTime >= now.AddHours(-24))
                .Sum(f => f.Amount);

            decimal session = records
                .Where(f => f.EventTime >= sessionStart)
                .Sum(f => f.Amount);

            return new FinanceSummary
            {
                TotalProfit = total,
                Profit24h = last24h,
                ProfitSession = session
            };
        }

        public static string FormatMoney(decimal amount)
        {
            string sign = amount >= 0 ? "+" : "-";
            decimal abs = Math.Abs(amount);
            return $"{sign}${abs:N0}";
        }

        public static string FormatMoneyPlain(decimal amount)
        {
            return $"${amount:N0}";
        }
    }
}