using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Models;
using Newtonsoft.Json;

namespace Kazakov_KP_01._01.Services
{
    public class ApiService
    {
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7193/api/")
        };

        // ЛОГИН || РЕГИСТРАЦИЯ
        public async Task<string> LoginAsync(string username, string password)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(username), "Usname");
            content.Add(new StringContent(password), "Password");

            var response = await _client.PostAsync("Users/Login", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeAnonymousType(json, new { token = "" });
                return data.token;
            }
            return null;
        }
        public async Task<string> RegisterAsync(string username, string password)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(username), "Usname");
            content.Add(new StringContent(password), "Password");

            var response = await _client.PostAsync("Users/Register", content);
            if (response.IsSuccessStatusCode)
                return "Success";
            else
            {
                var errorMes = await response.Content.ReadAsStringAsync();
                return errorMes;
            }
        }

        // ЛОГИ
        public async Task<List<Logs>> GetLogAsync()
        {
            var response = await _client.GetAsync($"Log/Get?token={UserSession.Token}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Logs>>(json);
            }

            return new List<Logs>();
        }
        public async Task<string> AddLogAsync(string logtype, string message)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(logtype), "logType");
            content.Add(new StringContent(message), "message");
            content.Add(new StringContent(DateTime.Now.ToString("o")), "eventTime");

            var response = await _client.PostAsync($"Log/Add?token={UserSession.Token}", content);

            if (response.IsSuccessStatusCode)
                return "Success"; 
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(errorBody) ? $"Ошибка: {response.StatusCode}" : errorBody;
            }
        }

        // ПРЕДММЕТЫ
        public async Task<List<Items>> GetItemAsync()
        {
            var response = await _client.GetAsync($"Items/Get?token={UserSession.Token}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Items>>(json);
            }

            return new List<Items>();
        }
        public async Task<string> AddItemAsync(string itemname, int maxBuyprice, bool isactive)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(itemname), "itemname");
            content.Add(new StringContent(maxBuyprice.ToString()), "maxBuyprice");
            content.Add(new StringContent(isactive.ToString().ToLower()), "isactive");

            var response = await _client.PostAsync($"Items/Add?token={UserSession.Token}", content);

            if (response.IsSuccessStatusCode)
                return "Success";
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(errorBody) ? $"Ошибка: {response.StatusCode}" : errorBody;
            }
        }
        public async Task<string> EditItemAsync(int itemId, string itemname, int maxBuyprice, bool isactive)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(itemId.ToString()), "itemId");
            content.Add(new StringContent(itemname), "itemname");
            content.Add(new StringContent(maxBuyprice.ToString()), "maxBuyprice");
            content.Add(new StringContent(isactive.ToString().ToLower()), "isactive");

            var response = await _client.PostAsync($"Items/Edit?token={UserSession.Token}", content);

            if (response.IsSuccessStatusCode)
                return "Success";
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(errorBody) ? $"Ошибка: {response.StatusCode}" : errorBody;
            }
        }

        public async Task<string> DeleteItemAsync(int itemId)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(itemId.ToString()), "itemd");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"Items/Delete?token={UserSession.Token}")
            {
                Content = content
            };

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return "Success";
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(errorBody) ? $"Ошибка: {response.StatusCode}" : errorBody;
            }
        }
    }
}
