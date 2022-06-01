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

            string testString = "Добрый день, спасибо за ответ.\r\n\r\nСобственно, тогда ещё вопрос - добавляем колонку в CardUIExtension с помощью:\r\n<pre><code>\r\n    var history = context.Model.TryGetTaskHistory();\r\n    if (history != null)\r\n    {\r\n        var column = new TaskHistoryColumnViewModel(history.Columns.Scope)\r\n        {\r\n            Header = \"SomeColumn\",\r\n            TextWrapping = TextWrapping.WrapWithOverflow\r\n        };   \r\n        history.Columns.Add(column);\r\n    }\r\n</code></pre> \r\n\r\nКак я понял, параметр column.DisplayMemberPath напрямую связан с информацией из Model у TaskHistoryItemViewModel. Получается, что колонки привязаны к этому классу?\r\nКак в таком случае можно привязать к колонке какие-то произвольные данные?";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
