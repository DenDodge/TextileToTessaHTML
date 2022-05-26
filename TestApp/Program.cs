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

            string testString = "Константин, добрый день.\r\n\r\nОшибка с расчетом количества заданий в отчетах будет исправлена в следующей версии Tessa (2.6).\r\n\r\nЗадания с вариантами завершения Отозвать и Вернуть документ на доработку будут исключены из отчетов по завершенным заданиям в будущих версиях Tessa.";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
