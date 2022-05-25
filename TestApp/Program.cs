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

            string testString = "Syntellect wrote:\r\n> Задания с вариантами завершения Отозвать и Вернуть документ на доработку будут исключены из отчетов по завершенным заданиям в будущих версиях Tessa.\r\n\r\nДобрый день!\r\nПодскажите, пожалуйста, по данному пункту пока нет планового срока?"

+"> Но почему - то при загрузке сохраненного документа, фильтрация по группе не работает. Оба файловых контрола отображают все файлы в кучу.\r\n\r\nПотому что событие ContainerFileAdding, как можно догадаться, работает при добавлении файлов.После сохранения карточки файлы уже добавлены. И их надо отдельным проходом по списку файлов удалить.\r\n\r\nВ расширении @CarUIExtension.Initialized@ есть пример работы с несколькими файловыми контролами.\r\n\r\n<pre> < code class=\"java\">\r\nIFileControl imagesFilesControl = ((FileListViewModel)context.Model.Controls[\"ImageFilesControl\"]).FileControl;\r\nforeach (IFile file in imagesFilesControl.Files.ToArray())\r\n{\r\n    // разрешены только файлы с категорией \"Изображения\"\r\n    if (file.Category == null || file.Category.Caption != \"Image\")\r\n    {\r\n        imagesFilesControl.Files.Remove(file);\r\n    }\r\n}\r\n</code></pre>"

+"Syntellect писал(а):\r\n> Добрый день,\r\n> В настоящий момент я немного занят доработкой решения Архив ЦБ.В середине недели смогу приступить к указанной выше доработке.\r\n\r\nПрошу сильно не оттягивать решение вопроса - на контроле у \"Самого высокого\" руководителя. Разработка по Архиву ЦБ длительная - её на 1-2 дня можно подвинуть. ";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, true);
        }
    }
}
