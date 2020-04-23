### Введение

В этом руководстве расскажем как создать WhatsApp бота на C#, используя наш шлюз API WhatsApp

Данный бот будет реагировать на команды, поступающие ему в виде обычных сообщений в WhatsApp и отвечать на них. Функционал тестового бота будет ограничен следующими функциями:

*   Реакция приветственным текстом на сообщение, команды которого нет у бота. Вывод меню бота
*   Вывод ID текущего чата (в ЛС или в беседе)
*   Отправка файлов различных форматов (pdf, jpg, doc, mp3 и др.)
*   Отправка голосовых сообщений (файлов *.ogg)
*   Отправка геолокации
*   Создание отдельной группы с собеседником и ботом

Писать наш бот будем с использованием технологии ASP.Net для поднятия сервера, который будет обрабатывать и отвечать на запросы пользователей

[Получить бесплатный доступ к WhatsApp API](https://app.chat-api.com)  

## Глава 1\. Создание проекта ASP.Net

Откроем Visual Studio и создадим проект "Веб приложение ASP.NET Core".

Далее выберем шаблон с пустым проектом (также можно выбрать шаблон API, который будет включать в себя уже необходимые контроллеры, которые останется только отредактировать. Мы же для наглядности создадим все с нуля)

Откроем файл **Startup.cs** и впишем в метод Configure данный код:

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
             if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

Это позволит нам настроить навигацию с использованием контроллера. Сейчас приступим к написанию самого контроллера.

Для этого создадим в проекте папку с названием Controllers, в которой создадим наш класс контроллер WebHookController

Наш контроллер должен наследовать класс **ControllerBase** и быть помечен атрибутами

    using Microsoft.AspNetCore.Mvc;
    namespace WaBot.Controllers
        {
            [ApiController]
            [Route("/")]
            public class WebHookController : ControllerBase
                {

                }
        }

Атрибут **Route** отвечат за адрес, по которому будет срабатывать данный котроллер. Указываем базовый путь домена.

На данном этапе наш контоллер практически готов. Теперь нам необходимо добавить методы по работе с API WA и другие вспомогательные классы, которые пригодятся нам в работе.

### Авторизация телефона

Свяжем whatsapp с нашим скриптом, чтобы по мере написания кода - проверять его работу. Для этого переходим [в личный кабинет](https://app.chat-api.com) и получаем там QR-код. Далее открываем WhatsApp на мобильном телефоне, заходим в Настройки -> WhatsApp Web -> Сканируем QR-код.

## Глава 2\. Класс API

В этой главе мы рассмотрим написание класса, который будет отвечать за взаимодействие с нашем API шлюзом. Документацию можно почитать [здесь](https://chat-api.com/ru/docs.html). Создадим класс **WaApi**:

    public class WaApi
        {
            private string APIUrl = "";
            private string token = "";

            public WaApi(string aPIUrl, string token)
                {
                    APIUrl = aPIUrl;
                    this.token = token;
                }
        }

Данный класс будет хранить в себе поля **APIUrl** и **token**, которые необходимы для работы с API. Получить их можно в вашем личном кабинете аккаунта.

Так же и конструктор, который присваивает значения в поля. Благодаря этому мы можем иметь несколько объектов, которые могут представлять различных ботов в случае, если необходимо настроить работу нескольких ботов одновременно.

### Метод для отправки запросов

Добавим в этот класс асинхронный метод, который будет осуществлять отправку POST запросов:

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

Данный метод принимает два аргумента.

*   _method_ - название нужного метода согласно [документации](https://chat-api.com/ru/docs.html)
*   _data_ - json строка для отправки

В методе сформируем строку url, на которую будет отправляться запрос. И далее делаем POST запрос на данный адрес с помощью класса System.Net.Http.HttpClient. Возвращаем ответ сервера.

На основе данного метода мы можем сделать необходимый нам функционал для работы бота.

### Отправка сообщений

    public async Task<string> SendMessage(string chatID, string text)
        {
            var data = new Dictionary<string, string>()
            {
                {"chatId",chatID },
                { "body", text }
            };
            return await SendRequest("sendMessage", JsonConvert.SerializeObject(data));
        }

Данный метод в качестве параметров принимает:

*   chatId - ID чата, куда необходимо отправить сообщение
*   text - текст отправляемого сообщения.

Для того, чтобы сформировать строку Json, воспользуемся удобной библиотекой [Newtonsoft.Json](https://www.newtonsoft.com/json). Создадим словарь, ключом которого будет строка с необходимым Json полем, согласно [документации](https://chat-api.com/ru/docs.html), а значением - наши параметры. Достаточно просто вызвать метод **JsonConvert.SerializeObject** и передать в него наш словарь для формирования Json строки. Вызываем метод **SendRequest**, передав в него название метода для отправки сообщений и нашу json строку.

Таким образом наш бот будет отвечать пользователям.

### Отправка голосового сообщения

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

Далее логика построения методов аналогична. Смотрим [документацию](https://chat-api.com/ru/docs.html) и отправляем на сервер нужные данные, вызывая необходимые методы.  

Для отправки голосового сообщения используется ссылка на файл формата .ogg и метод **sendAudio**

### Метод для отправки геолокации

    public async Task<string> SendGeo(string chatID)
        {
            var data = new Dictionary<string, string>()
            {
                { "lat", "55.756693" },
                { "lng", "37.621578" },
                { "address", "Your address" },
                { "chatId", chatID}
            };
            return await SendRequest("sendLocation", JsonConvert.SerializeObject(data));
        }

### Создание группы

Создает конференцию, в которой будете вы и бот

    public async Task<string> CreateGroup(string author)
        {
            var phone = author.Replace("@c.us", "");
            var data = new Dictionary<string, string>()
            {
                { "groupName", "Group C#"},
                { "phones", phone },
                { "messageText", "This is your group." }
            };
            return await SendRequest("group", JsonConvert.SerializeObject(data));
        }

### Метод для отправки файлов

Обговорим некоторые моменты, связанные с данным методом. Он принимает в параметры:

*   chatID - ID чата
*   format - формат файла, который необходимо отправить.

Для того, чтобы отправить файл, документация предусматривает несколько способов:

*   Ссылка на файл, который нужно отправить
*   Строка, которая является файлом, закодированным с помощью метода Base64.

**Рекомендуется отправлять файлы с помощью второго способа**, закодировав файлы в Base64 формат. В **Главе 4** мы более подробно об этом расскажем. А сейчас стоит знать, что мы описали статический класс **Base64String**, в котором описали свойства, записав в них все тестовые файлы нужных форматов. В методе просто вызываем свойство нужного формата и передаем данную Base64 строку на сервер.

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
                    { "caption", $"My file!" }
                };

                return await SendRequest("sendFile", JsonConvert.SerializeObject(data));
            }

            return await SendMessage(chatID, "No file with this format");
        }

На этом базовый функционал нашего класса API описан. Теперь соединим наш контроллер из **Главы 1** и API.

### Глава 3\. Обработка запросов.

Вернемся к контроллеру из первой главы. Опишем метод внутри контроллера, который будет обрабатывать post-запросы, приходящие на наш сервер от chat-api.com. Назовем метод **Post** (название может быть любым) и пометим его атрибутом **[HttpPost]**, что будет означать реагирование на Post запросы:

    [HttpPost]
    public async Task<string> Post(Answer data)
        {
            return "";
        }

Принимать наш метод будет класс **Answer**, который является десериализированным объектом из пришедшей к нам строки json. Для того, чтобы описать класс **Answer**, нам потребуется узнать, какой json будет к нам приходить.

Для этого можно воспользоваться удобным разделом **"Тестирование" - "Симуляция Webhooka"** в вашем личном кабинете

Справа мы можем видеть json тело, которое будет к нам приходить.

Воспользуемся сервисом [конвертации json в C#](http://json2csharp.com/). Либо опишем класс сами, ипользуя атрибуты библиотеки [Newtonsoft.Json](https://www.newtonsoft.com/json):

    public partial class Answer
        {
            [JsonProperty("instanceId")]
            public string InstanceId { get; set; }

            [JsonProperty("messages")]
            public Message[] Messages { get; set; }
        }

    public partial class Message
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("senderName")]
            public string SenderName { get; set; }

            [JsonProperty("fromMe")]
            public bool FromMe { get; set; }

            [JsonProperty("author")]
            public string Author { get; set; }

            [JsonProperty("time")]
            public long Time { get; set; }

            [JsonProperty("chatId")]
            public string ChatId { get; set; }

            [JsonProperty("messageNumber")]
            public long MessageNumber { get; set; }
        }

Теперь, когда у нас есть объектное представление пришедшего запроса, сделаем обработку его в контроллере.

Внутри контроллера создадим статическое поле, которым будет являться наш API ссылкой и токеном:

    private static readonly WaApi api = new WaApi("https://eu115.chat-api.com/instance12345/", "123456789token");

В цикле метода проходимся по всем пришедшим к нам сообщениям и делаем проверку на то, что обрабатываемое сообщение не является нашим собственным. Это нужно для того, чтобы бот не зацикливался сам на себе. Если все же сообщение от самого себя - пропускаем:

    [HttpPost]
    public async Task<string> Post(Answer data)
        {
            foreach (var message in data.Messages)
            {
                if (message.FromMe)
                    continue;
            }
        }

Далее опишем switch, в который будем передавать получаемые команды. Применим к свойству Body метод Split(), чтобы разбить сообщение по пробелам. Передадим в switch первую команду, вызвав метод ToLower(), чтобы стиль написания команды не играл роли и обрабатывался одинаково:

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
                        return await api.SendMessage(message.ChatId, $"Your ID: {message.ChatId}");
                    case "file":
                        var texts = message.Body.Split();
                        if (texts.Length > 1)
                            return await api.SendFile(message.ChatId, texts[1]);
                        break;
                    case "ogg":
                        return await api.SendOgg(message.ChatId);
                    case "geo":
                        return await api.SendGeo(message.ChatId);
                    case "group":
                        return await api.CreateGroup(message.Author);
                    default:
                        return await api.SendMessage(message.ChatId, welcomeMessage);
                }
            }
            return "";
        }

В **case** запишем все необходимые нам команды и будем вызывать методы из объекта нашего API, которые их реализауют.  
А **default** будет обрабатывать команды, которых не существует. Для этого просто будем отправлять сообщение из меню бота пользователю.

## Whatsapp бот на C#

Наш бот готов. Он уже может отвечать и обрабатывать команды пользователя, остается только добавить его на хостинг и указать домен в качестве webhook в личном кабинете пользователя chat-api.com.

Исходники бота будут доступны по ссылке на гитхаб: [https://github.com/chatapi/whatsapp-csharp-bot-ru](https://github.com/chatapi/whatsapp-csharp-bot-ru "Скачать исходники whatsapp бота на С#"). Не забудьте подставить свой токен из личного кабинета и номер инстанса.

[Получить ключ API](https://app.chat-api.com)  

А в следующих главах расскажем о base64 и возможных ошибках, с которыми вы можете столкнуться.

## Глава 4\. Base64

**Base64** - это стандарт кодирования данных. С помощью данного стандарта мы можем закодировать файлы в строку и передавать их таким образом.  
В вашем личном кабинете доступен [сервис](https://app.chat-api.com/base64), который поможет сгенерировать строку данного формата. Для написания этого бота необходимо было предоставить несколько статичных данных для теста, поэтому мы вставили полученные строки в вспомогательный класс и обращались к ним из кода.  
Вам же может быть полезнее кодировать файлы "на лету".

Для генерации таких строк можно также воспользоваться встроенными средствами языка C#.

## Глава 5\. Публикация сервера.

Для установки сервера в качестве weebhook требуется этот самый сервер загрузить в Интернет. Для этого можно воспользоваться сервисами по предоставлению услуг хостинга, vps или vds серверов. Для тестирования бота мы воспользовались одним из популярных хостингов (reg.ru)

Необходимо выбрать и оплатить услугу, которая поддерживает технологию ASP.Net. Далее можно воспользоваться данной [инструкцией](https://www.reg.ru/support/hosting-i-servery/kak-razmestit-sayt-na-hostinge/kak-razvernut-sait-na-asp-net-s-pomoshiu-web-deploy) по публикации сервера на хостинг.

### Возможные проблемы, с которыми вы можете столкнуться

*   Невозможно соединиться с сервером хостинга для публикации своего сервера. Решение: обратиться в техподдержку и попросить включить Web Deploy для вашей услуги

*   **HTTP ERROR 500.0** [Решение здесь](https://stackoverflow.com/questions/55731142/error-trying-to-host-an-asp-net-2-2-website-on-plesk-onyx-17-8-11-http-error-5)
