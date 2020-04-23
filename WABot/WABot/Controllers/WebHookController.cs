using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WABot.Api;
using WABot.Helpers.Json;

namespace WABot.Controllers
{
    /// <summary>
    /// Контроллер для обработки запросов, поступающих с chat-api.com
    /// </summary>
    [ApiController]
    [Route("/")]
    public class WebHookController : ControllerBase
    {
        /// <summary>
        /// Статический объект, представляющий API для данного контроллера.
        /// </summary>
        private static readonly WaApi api = new WaApi("https://eu115.chat-api.com/instance12345/", "123456789token");
        private static readonly string welcomeMessage = "Меню бота: \n" +
                                                        "1. chatid - Получить chatid\n" +
                                                        "2. file doc/gif,jpg,png,pdf,mp3,mp4 - Получить файл нужного формата\n" +
                                                        "3. ogg - Получить голосовое\n" +
                                                        "4. гео - Получить геолокацию\n" +
                                                        "5. group - Создать группу с ботом";

        /// <summary>
        /// Обработчик пост запросов поступающих от chat-api
        /// </summary>
        /// <param name="data">Сериализованный объект json</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> Post(Answer data)
        {
            foreach (var message in data.Messages)
            {
                if (message.FromMe)
                    continue;

                switch (message.Body.Split()[0].ToLower())
                {
                    case "chatid":
                        return await api.SendMessage(message.ChatId, $"Ваш ID: {message.ChatId}");
                    case "file":
                        var texts = message.Body.Split();
                        if (texts.Length > 1)
                            return await api.SendFile(message.ChatId, texts[1]);
                        break;
                    case "ogg":
                        return await api.SendOgg(message.ChatId);
                    case "гео":
                        return await api.SendGeo(message.ChatId);
                    case "group":
                        return await api.CreateGroup(message.Author);
                    default:
                        return await api.SendMessage(message.ChatId, welcomeMessage);
                }             
            }
            return "";          
        }  
    }
}
