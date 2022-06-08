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
                { "contract.png", new Guid("49ba1622-55d9-48c5-a4d5-9a85801fd62a") },
                { "2.png", new Guid("8166b72b-3a3b-4198-9cb9-657da5d9f8ee") },
                { "07.11.12.png", new Guid("8b22ba42-6404-4130-bd1b-ff2fb037f893") }
            };

            string filesDirectory = $"D:\\WORK_SYNTELLECT\\OtherFiles\\Migration\\12504";

            string testString =
                "> чтобы пользователи добавлялись в оба юр.лица ДТ, но не добавлялись в наши роли.\r\n\r\nКоллеги, написал вам скрипт, который добавляет в обе Legal Entity Донского Табака всех пользователей, заведённых в Docsvision.\r\nСкрипт @AddEmployeesToCompanies.sql@\r\n\r\n\r\nС уважением, Денис Афанасьев.";
                Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
