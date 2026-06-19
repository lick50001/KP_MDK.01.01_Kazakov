using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Services
{
    public static class VkNotifier
    {
        private static readonly HttpClient _client = new HttpClient();

        private const string GroupToken = "vk1.a.qXmTEzw6VHOv4HLTzZYjeP1LsR5SL_gJYwU83p7HTFJTiGTwjJg4YIMqD8FcWC57zcbx7Epy1qLRHD2SXrn13J8RQ6QWruFzCix-tZhsigcWVlxPAD-vpkWHtFkZV7thQavbq9vjL1wqmrTc0F_dYXTIVJD2AY4DOAuz37Y73pxv-Oeeni01eUNxOqW16oDF2MW0xPLSxHJNPk7GwM6_Nw";
        private const string UserId = "495108218";
        private const string ApiVersion = "5.131";

        public static async Task SendAsync(string message)
        {
            try
            {
                string encodedMessage = Uri.EscapeDataString(message);
                string randomId = new Random().Next(1, int.MaxValue).ToString();

                string url = "https://api.vk.com/method/messages.send" +
                             "?user_id=" + UserId +
                             "&message=" + encodedMessage +
                             "&random_id=" + randomId +
                             "&access_token=" + GroupToken +
                             "&v=" + ApiVersion;

                var response = await _client.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();

                if (result.Contains("error"))
                {
                    System.Diagnostics.Debug.WriteLine("VK ошибка: " + result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VK исключение: " + ex.Message);
            }
        }

        public static async Task NotifyBuyAsync(string itemName, int quantity, decimal totalSpent)
        {
            string message =
                "Покупка\n" +
                "Предмет: " + itemName + "\n" +
                "Количество: " + quantity + " шт.\n" +
                "Потрачено: " + totalSpent.ToString("N0") + " silver";

            await SendAsync(message);
        }

        public static async Task NotifySellAsync(decimal totalEarned)
        {
            string message =
                "Продажа\n" +
                "Выручка: " + totalEarned.ToString("N0") + " silver";

            await SendAsync(message);
        }

        public static async Task NotifyCycleSummaryAsync(decimal profit24h, decimal profitSession)
        {
            string message =
                "Итоги цикла\n" +
                "Прибыль (24ч): " + profit24h.ToString("N0") + "\n" +
                "Прибыль (сеанс): " + profitSession.ToString("N0");

            await SendAsync(message);
        }

        public static async Task<string> TestSendAsync(string message)
        {
            try
            {
                string encodedMessage = Uri.EscapeDataString(message);
                string randomId = new Random().Next(1, int.MaxValue).ToString();

                string url = "https://api.vk.com/method/messages.send" +
                             "?user_id=" + UserId +
                             "&message=" + encodedMessage +
                             "&random_id=" + randomId +
                             "&access_token=" + GroupToken +
                             "&v=" + ApiVersion;

                var response = await _client.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();
                return result; 
            }
            catch (Exception ex)
            {
                return "Исключение: " + ex.Message;
            }
        }
    }
}