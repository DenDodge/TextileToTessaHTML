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
                "Пропадает строка с ролью *сотрудника* из таблицы @RoleUsers@ на которой завязаны некоторые вещи, такие как высчитывание ролей для отображения\\отправки заданий на определенную роль, отправка на ознакомление или расчет прав доступа.\r\n\r\nДобавление новой строки в таблицу *@RoleUsers@* должно решать проблему, скрипт для добавления нужной записи приложен ниже, если будут возникать аналогичные ситуации (если сотрудник не входит в \"свою\" роль это точно неправильно)\r\n\r\n<pre><code class=\"SQL\">\r\ninsert into RoleUsers\r\nselect ID, NEWID(), 1, ID, FullName, 0\r\nfrom PersonalRoles\r\nwhere ID in ('<ID сотрудника>')\r\n--where ID in ('<ID сотрудника1>', '<ID сотрудника2>', '<ID сотрудникаN>') /*Для добавления сразу нескольких сотрудников указываем ID через запятые*/\r\n</code></pre>\r\n\r\nДля данного сотрудника роль исправлена, но обратите внимание на остальных удаленных сотрудников.";
                Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
