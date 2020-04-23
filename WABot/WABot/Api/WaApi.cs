using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WABot.Helpers;

namespace WABot.Api
{
    /// <summary>
    /// Класс представляющий реализацию API сайта chat-api.com
    /// </summary>
    public class WaApi
    {
        /// <summary>
        /// Ссылка. Можно получить в личном кабинете.
        /// </summary>
        private string APIUrl = "";
        /// <summary>
        /// Токен. Можно получить в личном кабинете.
        /// </summary>
        private string token = "";

        /// <summary>
        /// Конструктор принимает в качестве параметров токен и ссылку
        /// </summary>
        /// <param name="aPIUrl">Ссылка</param>
        /// <param name="token">Токен</param>
        public WaApi(string aPIUrl, string token)
        {
            APIUrl = aPIUrl;
            this.token = token;
        }

        /// <summary>
        /// Метод делает запрос на сервер chat-api.com.
        /// </summary>
        /// <param name="method">Метод API согласно документации.</param>
        /// <param name="data">Json данные</param>
        /// <returns></returns>
        public async Task<string> SendRequest(string method, string data)
        {
            string url = $"{APIUrl}{method}?token={token}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var result = await client.PostAsync("", content);
                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Отправляет сообщение по данному ID
        /// </summary>
        /// <param name="chatID">ID чата</param>
        /// <param name="text">Текст сообщения</param>
        /// <returns></returns>
        public async Task<string> SendMessage(string chatID, string text)
        {
            var data = new Dictionary<string, string>()
            {
                {"chatId",chatID },
                { "body", text }
            };
            return await SendRequest("sendMessage", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Отправляет голосовое сообщение
        /// </summary>
        /// <param name="chatID">ID чата</param>
        /// <returns></returns>
        public async Task<string> SendOgg(string chatID)
        {
            string ogg = "https://firebasestorage.googleapis.com/v0/b/chat-api-com.appspot.com/o/audio_2019-02-02_00-50-42.ogg?alt=media&token=a563a0f7-116b-4606-9d7d-172426ede6d1";
            var data = new Dictionary<string, string>
            {
                {"audio", ogg },
                {"chatId", chatID }
            };

            return await SendRequest("sendAudio", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Отправляет геолокацию
        /// </summary>
        /// <param name="chatID">ID чата</param>
        /// <returns></returns>
        public async Task<string> SendGeo(string chatID)
        {
            var data = new Dictionary<string, string>()
            {
                { "lat", "55.756693" },
                { "lng", "37.621578" },
                { "address", "Ваш адрес" },
                { "chatId", chatID}
            };
            return await SendRequest("sendLocation", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Создает группу между пользователем и ботом.
        /// </summary>
        /// <param name="author">Параметр author из полученного JSON тела.</param>
        /// <returns></returns>
        public async Task<string> CreateGroup(string author)
        {
            var phone = author.Replace("@c.us", "");
            var data = new Dictionary<string, string>()
            {
                { "groupName", "Группа C#"},
                { "phones", phone },
                { "messageText", "Это ваша группа." }
            };
            return await SendRequest("group", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Отпавляет пользователю файлы.
        /// </summary>
        /// <param name="chatID">ID чата.</param>
        /// <param name="format">Формат нужного файла.</param>
        /// <returns></returns>
        public async Task<string> SendFile(string chatID, string format)
        {
            var availableFormat = new Dictionary<string, string>()
            {
                {"doc", Base64String.Doc },
                {"gif",Base64String.Gif },

                { "jpg",Base64String.Jpg },
                { "png", Base64String.Png },
                { "pdf", Base64String.Pdf },
                { "mp4",Base64String.Mp4 },
                { "mp3", Base64String.Mp3}
            };

            if (availableFormat.ContainsKey(format))
            {
                var data = new Dictionary<string, string>(){
                    { "chatId", chatID },
                    { "body", availableFormat[format] },
                    { "filename", "yourfile" },
                    { "caption", $"Ваш файл" }                  
                };

                return await SendRequest("sendFile", JsonConvert.SerializeObject(data));
            }

            return await SendMessage(chatID, "Нет файла с таким форматом");
        }
    }
}
