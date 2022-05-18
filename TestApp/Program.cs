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

            string testString = "This can occur in situations that raise exceptions such as passing a file name with invalid characters or too many characters, a failing or missing disk, or if the *caller does not have permission to read the file*.\r\n\r\nhttps://docs.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-5.0\r\n\r\nТаким образом, если в процессе TessaHost действительно нет прав на чтение файла, как и выводится в предварительной ошибке, то да, TessaHost ругается именно исключением с указанным текстом.\r\n\r\n> Судя по логам, данной ошибке обычно предшествует предупреждение вида: The process cannot access the file 'C:\\Users\\yavorskiy\\AppData\\Local\\Temp\\Tessa\\Files\\2xwyn2mg.n3r\\eds-sign.bin' because it is being used by another process.\r\n\r\nhttps://link\r\n\r\n";
            Parser parser = new Parser();
            var result = parser.GetParseToTessaHTMLString(testString, filesDirectory, attachemntsIds, false);
        }
    }
}
