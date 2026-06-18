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
            DateTime now = DateTime.UtcNow;
            DateTime sessionStart = SessionManager.SessionStartTime;

            decimal total = records.Sum(f => f.Amount);

            decimal last24h = records
                .Where(f => f.EventTime >= now.AddHours(-24))
                .Sum(f => f.Amount);

            decimal session = records
                .Where(f => f.EventTime >= sessionStart)
                .Sum(f => f.Amount);

            FinanceSummary summary = new FinanceSummary();
            summary.TotalProfit = total;
            summary.Profit24h = last24h;
            summary.ProfitSession = session;
            return summary;
        }

        public static string FormatMoney(decimal amount)
        {
            string sign = amount >= 0 ? "+" : "-";
            decimal abs = Math.Abs(amount);
            return sign + "$" + abs.ToString("N0");
        }

        public static string FormatMoneyPlain(decimal amount)
        {
            return "$" + amount.ToString("N0");
        }
    }
}