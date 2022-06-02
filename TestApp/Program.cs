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

            string testString = "Денис, добрый день!\r\n\r\nПосле обновления views визуально ничего не поменялось.\r\nКарты по прежнему Active = no.\r\nПредполагаю, что причина в дате окончания контракта:\r\nCASE  WHEN t.ExpiryDate>@dt THEN 'Yes' ELSE 'No' END AS 'Active'\r\n\r\nНо это условие не учитывает:ъ\r\n1. Тип контракта = fixed date with automatic prolongation\r\n2. И сценарий, когда создается допик на расторжение договора.\r\n\r\nВозможно сделать следующее:\r\n1. Добавить  новый тип доп соглашения - Contract termination (в разделе Attached Documents -> Addendum). Этот тип допика также как и обычный допик может менять или не менять дату окончания контракта.\r\n2. Проверять активность контракта - с учетом типа контракта, наличия доп соглашения на расторжения (для тех контрактов где идет автоматическая пролонгация) и даты окончания контракта.\r\n\r\nRegards,\r\nKanat\r\nSyntellect wrote:\r\n> Здравствуйте, Малика.\r\n> \r\n> Поправил views.\r\n> \r\n> С уважением, Денис Афанасьев.\r\n\r\n";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
