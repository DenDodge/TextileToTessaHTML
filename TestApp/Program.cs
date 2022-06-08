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
                "Оксана Харина wrote:\r\n> Оксана Харина wrote:\r\n> > Джордж, а что конкретно согласовано?\r\n> \r\n> Правильно ли я понимаю, что в поле Статья БДДС должен отображать выбор из справочника Статья БДДС в зависимости от Признака договора (карточка Договор) и значения поля Приход/Расход (карточка Статья БДДС)?\r\n> \r\n> Признак Договора = Доходный и Приход/Расход = Приход\r\n> Признак Договора = Расходный и Приход/Расход = Расход\r\n> \r\n> И что делать со Статьями БДДС, у которых данный атрибут не заполнен. По ТЗ поле Приход/Расход не обязателен для заполнения?\r\n\r\n\r\nДа, вы все правильно поняли. По ТЗ поле Статья БДДС не обязательна для заполнения. \r\n";
                Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
