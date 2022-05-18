using System;
using System.Collections.Generic;
using TextileToTessaHTML;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, Guid> attachemntsIds = new Dictionary<string, Guid>()
            {
                { "photo_2021-10-05_11-52-45.jpg", new Guid("49ba1622-55d9-48c5-a4d5-9a85801fd62a") },
                { "2.png", new Guid("8166b72b-3a3b-4198-9cb9-657da5d9f8ee") },
                { "07.11.12.png", new Guid("8b22ba42-6404-4130-bd1b-ff2fb037f893") }
            };

            string filesDirectory = $"D:\\WORK_SYNTELLECT\\OtherFiles\\Migration\\29906";

            string testString ="Добрый день, коллеги! Вынужден эскалировать.\r\n\r\nПросьба возобновить исследование.Проблема с подписанием стала воспроизводиться у *третьего * заказчика(версия 3.5.12).\r\n\r\nСудя по симптомам, файлы(см.журнал из первичного сообщения) держит приложение TessaHost: после перезапуска TessaClient перезапускается и TessaHost и файлы подписываются без ошибок.\r\n\r\nТа же ситуация с проверкой: подпись на сервере корректная. У клиента при проверке возникает ошибка:\r\n!photo_2021-10-05_11-52-45.jpg!";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, false);
        }
    }
}
