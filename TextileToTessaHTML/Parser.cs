using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TextileToTessaHTML
{
    public class Parser
    {
        #region Private Fields

        /// <summary>
        /// Строка прикрепленных файлов к сообщению.
        /// </summary>
        private string AttachmentsString;

        /// <summary>
        /// Прикрепленные к сообщению файлы.
        /// Возвращаются вместе с преобразованной строкой.
        /// </summary>
        private HashSet<Guid> AttachedFileIds;

        /// <summary>
        /// Открывающие теги стандартной HTML разметки.
        /// </summary>
        private Dictionary<string, string> StandardOpeningTags = new();

        /// <summary>
        /// Закрывающие теги стандартной HTML разметки.
        /// </summary>
        private Dictionary<string, string> StandardClosingTags = new();

        /// <summary>
        /// Открывающие теги HTML разметки TESSA.
        /// </summary>
        private Dictionary<string, string> TessaOpeningTags = new();

        /// <summary>
        /// Закрывающие теги HTML разметки TESSA.
        /// </summary>
        private Dictionary<string, string> TessaClosingTags = new();

        /// <summary>
        /// Приложенные к инциденту файлы.
        /// </summary>
        private Dictionary<Guid, (string Name, string FileName)> AttachedFilesIssue = new();

        /// <summary>
        /// Пустое описание инцидента.
        /// Применяется, если входящая строка пустая или с пробелами.
        /// </summary>
        private const string EmptyString = "{\"Text\":\"<div class=\\\"forum-div\\\"><p><span>Описание инцидента отсутствует.</span></p></div>\"}";

        #region Tags Names

        private const string BoldItalicTagName = "BoldItalicTag";
        private const string BoldTagName = "BoldTag";
        private const string ItalicTagName = "ItalicTag";
        private const string UnderlineTagName = "UnderlineTag";
        private const string UnderlineBoldTagName = "UnderlineBoldTag";
        private const string CrossedOutTagName = "CrossedOutTag";
        private const string NumberedListTagName = "NumberedListTag";
        private const string UnNumberedListTagName = "UnNumberedListTag";
        private const string ListItemTagName = "ListItemTag";
        private const string ParagraphTagName = "ParagraphTag";
        private const string PreTagName = "PreTag";
        private const string CodeTagName = "CodeTag";
        private const string CitationTagName = "CitationTag";

        #endregion

        #region Tags

        private const string SpanClosedTag = "</span>";

        #region Standard Tags

        private const string StandardBoldOpenTag = "<strong>";
        private const string StandardBoldItalicOpenTag = "<strong><em>";
        private const string StandardItalicOpenTag = "<em>";
        private const string StandardUnderlineOpenTag = "<ins>";
        private const string StandardUnderlineBoldOpenTag = "<ins><strong>";
        private const string StandardCrossedOutOpenTag = "<del>";
        private const string StandardNumberedListOpenTag = "<ol>";
        private const string StandardUnNumberedListOpenTag = "<ul>";
        private const string StandardListItemOpenTag = "<li>";
        private const string StandardParagraphOpenTag = "<p>";
        private const string StandardPreOpenTag = "<pre>";
        private const string StandardCodeOpenTag = "<code>";
        private const string StandardCitationOpenTag = "<citation>";

        private const string StandardBoldItalicClosedTag = "</strong></em>";
        private const string StandardBoldClosedTag = "</strong>";
        private const string StandardItalicClosedTag = "</em>";
        private const string StandardUnderlineClosedTag = "</ins>";
        private const string StandardUnderlineBoldClosedTag = "</ins></strong>";
        private const string StandardCrossedOutClosedTag = "</del>";
        private const string StandardNumberedListClosedTag = "</ol>";
        private const string StandardUnNumberedListClosedTag = "</ul>";
        private const string StandardListItemClosedTag = "</li>";
        private const string StandardParagraphClosedTag = "</p>";
        private const string StandardPreClosedTag = "</pre>";
        private const string StandardCodeClosedTag = "</code>";
        private const string StandardCitationCloseTag = "</citation>";

        private const string StandardNewLineTag = "<br />";

        #endregion

        #region Tessa Tags

        private const string TessaBoldItalicOpenTag = "</span><span style=\\\"font-weight:bold;font-style:italic;\\\">";
        private const string TessaBoldOpenTag = "</span><span style=\\\"font-weight:bold;\\\">";
        private const string TessaItalicOpenTag = "</span><span style=\\\"font-style:italic;\\\">";
        private const string TessaUnderlineOpenTag = "</span><span style=\\\"text-decoration:underline;\\\">";
        private const string TessaUnderlineBoldOpenTag = "</span><span style=\\\"text-decoration:underline;font-weight:bold;\\\">";
        private const string TessaCrossedOutOpenTag = "</span><span style=\\\"text-decoration:line-through;\\\">";
        private const string TessaNumberedListOpenTag = "</span><ol class=\\\"forum-ol\\\">";
        private const string TessaUnNumberedListOpenTag = "</span><ul class=\\\"forum-ul\\\">";
        private const string TessaListItemOpenTag = "<li><p><span>";
        private const string TessaParagraphOpenTag = "<p><span>";
        private const string TessaPreOpenTag = "<div class=\\\"forum-block-monospace\\\"><p><span>";
        private const string TessaCitationOpenTag = "<div class=\"forum-quote\"><p><span>";

        private const string TessaListItemClosedTag = "</span></p></li>";
        private const string TessaParagraphClosedTag = "</span></p>";
        private const string TessaPreCloseTag = "</span></p></div>";
        private const string TessaCitationCloseTag = "</span></p></div>";

        private const string TessaNewLineTag = "</span></p><p><span>";

        #endregion

        #endregion

        #region Regex Templates

        /// <summary>
        /// Шаблон регулярного выражения открытия тега заголовка.
        /// </summary>
        private static readonly string headerOpenTagTemplate = @"<h[1-6]>";
        /// <summary>
        /// Шаблон регулярного выражения открытия тега заголовка.
        /// </summary>
        private static readonly string headerCloseTagTemplate = @"<\/h[1-6]>";
        /// <summary>
        /// Шаблон регулярного выражения для тега изображения (описание).
        /// </summary>
        private static readonly string imagesTagDescriptionTemplate = "<img src=&#8220;(.*?)&#8220; .*? />";
        /// <summary>
        /// Шаблон регулярного выражения для тега изображения (сообщение).
        /// </summary>
        private static readonly string imagesTagTopicTemplate = "<img src=\"(.*?)\" .*? />";
        /// <summary>
        /// Шаблон регулярного выражения для секции кода с "pre".
        /// </summary>
        private static readonly string preCodeTagsTemplate = "<pre><code.*?>";
        /// <summary>
        /// Шаблон регулярного выражения для сворачиваемой секции.
        /// </summary>
        private static readonly string collapseTagTemplate = "{{collapse\\((.*?)\\)";
        /// <summary>
        /// Шаблон регулярного выражения для секции кода.
        /// </summary>
        private static readonly string codeSectionTemplate = "<code>.*?</code>";
        /// <summary>
        /// Шаблон регулярного выражения для ссылки.
        /// </summary>
        private static readonly string httpLinkTemplate = "(http.*?)[ ,<]";
        /// <summary>
        /// Шаблон регулярного выражения для цитирования.
        /// </summary>
        private static readonly string citationTemplate = "\\n&gt;(.*?)\\r";
        /// <summary>
        /// Шаблон регулярного выражения для вложенного цитирования.
        /// </summary>
        private static readonly string nestedCitationTemplate = "<citation> &gt;(.*?)</citation>";
        
        #endregion

        #region Regex

        /// <summary>
        /// Регулярное выражение для секции открытия тега заголовка.
        /// </summary>
        private static readonly Regex _headerOpenTag = new Regex(headerOpenTagTemplate,
           RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражения для секции закрытия тега заголовка.
        /// </summary>
        private static readonly Regex _headerCloseTag = new Regex(headerCloseTagTemplate,
           RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для секции тега изображения в описании.
        /// </summary>
        private static readonly Regex _imagesDescriptionTag = new Regex(imagesTagDescriptionTemplate,
            RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для секции тега изображения в сообщении.
        /// </summary>
        private static readonly Regex _imagesTopicTag = new Regex(imagesTagTopicTemplate,
            RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для секции кода с "pre".
        /// </summary>
        private static readonly Regex _preCodeTag = new Regex(preCodeTagsTemplate,
            RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для сворачиваемой секции.
        /// </summary>
        private static readonly Regex _collapseTag = new Regex(collapseTagTemplate,
            RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для секции кода.
        /// </summary>
        private static readonly Regex _codeSection = new Regex(codeSectionTemplate,
           RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// Регулярное выражение для ссылки.
        /// </summary>
        private static readonly Regex _httpLink = new Regex(httpLinkTemplate,
           RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Регулярное выражение для цитаты.
        /// </summary>
        private static readonly Regex _citation = new Regex(citationTemplate,
           RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Регулярное выражение для вложенной цитаты.
        /// </summary>
        private static readonly Regex _nestedCitation = new Regex(nestedCitationTemplate,
            RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Создать объект класса "Parser".
        /// </summary>
        public Parser()
        {
            InitialStandardTags();
            InitialTessaTags();
        }

        #endregion

        /// <summary>
        /// Получить преобразованную из "Textile" в "платформенные HTML" строку и список приложенных к этой строке идентификаторов файлов.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="issueDirectory">Расположение объекта "Инцидент" из редмайна.</param>
        /// <param name="attachedFileIssue">Прикрепленные к инциденту файлы.</param>
        /// <param name="isTopicText">Истина - это сообщение топика.</param>
        /// <returns>Преобразованная строка и список приложенных к этой строке идентификаторов файлов.</returns>
        public (string ResultString, HashSet<Guid> AttachedFileIds) GetParseToTessaHtmlString(
            string mainString,
            string issueDirectory,
            Dictionary<Guid, (string, string)> attachedFileIssue,
            bool isTopicText = false)
        {
            AttachedFileIds = new HashSet<Guid>();

            if (string.IsNullOrWhiteSpace(mainString))
            {
                return (EmptyString, AttachedFileIds);
            }
            
            AttachedFilesIssue = attachedFileIssue;
            AttachmentsString = "";

            var resultString = TextileParseString(mainString, isTopicText);
            if (!isTopicText)
            {
                resultString = resultString.Replace("\"", "&#8220;");
            }
            resultString = StandardHtmlParseString(
                resultString,
                issueDirectory,
                isTopicText);
            resultString = resultString.Replace("\n", TessaNewLineTag);

            return (resultString, AttachedFileIds);
        }

        #region Private methods

        #region Initial methods

        /// <summary>
        /// Инициализация стандартных HTML тегов.
        /// </summary>
        private void InitialStandardTags()
        {
            StandardOpeningTags.Clear();
            StandardClosingTags.Clear();

            // тут важен порядок добавления.
            StandardOpeningTags.Add(ParagraphTagName, StandardParagraphOpenTag);
            StandardOpeningTags.Add(BoldItalicTagName, StandardBoldItalicOpenTag);
            StandardOpeningTags.Add(UnderlineBoldTagName, StandardUnderlineBoldOpenTag);
            StandardOpeningTags.Add(BoldTagName, StandardBoldOpenTag);
            StandardOpeningTags.Add(ItalicTagName, StandardItalicOpenTag);
            StandardOpeningTags.Add(UnderlineTagName, StandardUnderlineOpenTag);
            StandardOpeningTags.Add(CrossedOutTagName, StandardCrossedOutOpenTag);
            StandardOpeningTags.Add(NumberedListTagName, StandardNumberedListOpenTag);
            StandardOpeningTags.Add(UnNumberedListTagName, StandardUnNumberedListOpenTag);
            StandardOpeningTags.Add(ListItemTagName, StandardListItemOpenTag);
            StandardOpeningTags.Add(PreTagName, StandardPreOpenTag);
            StandardOpeningTags.Add(CodeTagName, StandardCodeOpenTag);
            StandardOpeningTags.Add(CitationTagName, StandardCitationOpenTag);

            StandardClosingTags.Add(ParagraphTagName, StandardParagraphClosedTag);
            StandardClosingTags.Add(BoldItalicTagName, StandardBoldItalicClosedTag);
            StandardClosingTags.Add(UnderlineBoldTagName, StandardUnderlineBoldClosedTag);
            StandardClosingTags.Add(BoldTagName, StandardBoldClosedTag);
            StandardClosingTags.Add(ItalicTagName, StandardItalicClosedTag);
            StandardClosingTags.Add(UnderlineTagName, StandardUnderlineClosedTag);
            StandardClosingTags.Add(CrossedOutTagName, StandardCrossedOutClosedTag);
            StandardClosingTags.Add(NumberedListTagName, StandardNumberedListClosedTag);
            StandardClosingTags.Add(UnNumberedListTagName, StandardUnNumberedListClosedTag);
            StandardClosingTags.Add(ListItemTagName, StandardListItemClosedTag);
            StandardClosingTags.Add(PreTagName, StandardPreClosedTag);
            StandardClosingTags.Add(CodeTagName, StandardCodeClosedTag);
            StandardClosingTags.Add(CitationTagName, StandardCitationCloseTag);
        }

        /// <summary>
        /// Инициализация HTML тегов TESSA.
        /// </summary>
        private void InitialTessaTags()
        {
            TessaOpeningTags.Clear();
            TessaClosingTags.Clear();

            TessaOpeningTags.Add(ParagraphTagName, TessaParagraphOpenTag);
            TessaOpeningTags.Add(BoldItalicTagName, TessaBoldItalicOpenTag);
            TessaOpeningTags.Add(UnderlineBoldTagName, TessaUnderlineBoldOpenTag);
            TessaOpeningTags.Add(BoldTagName, TessaBoldOpenTag);
            TessaOpeningTags.Add(ItalicTagName, TessaItalicOpenTag);
            TessaOpeningTags.Add(UnderlineTagName, TessaUnderlineOpenTag);
            TessaOpeningTags.Add(CrossedOutTagName, TessaCrossedOutOpenTag);
            TessaOpeningTags.Add(NumberedListTagName, TessaNumberedListOpenTag);
            TessaOpeningTags.Add(UnNumberedListTagName, TessaUnNumberedListOpenTag);
            TessaOpeningTags.Add(ListItemTagName, TessaListItemOpenTag);
            TessaOpeningTags.Add(PreTagName, TessaPreOpenTag);
            TessaOpeningTags.Add(CodeTagName, TessaPreOpenTag);
            TessaOpeningTags.Add(CitationTagName, TessaCitationOpenTag);

            TessaClosingTags.Add(ParagraphTagName, TessaParagraphClosedTag);
            TessaClosingTags.Add(BoldItalicTagName, SpanClosedTag);
            TessaClosingTags.Add(UnderlineBoldTagName, SpanClosedTag);
            TessaClosingTags.Add(BoldTagName, SpanClosedTag);
            TessaClosingTags.Add(ItalicTagName, SpanClosedTag);
            TessaClosingTags.Add(UnderlineTagName, SpanClosedTag);
            TessaClosingTags.Add(CrossedOutTagName, SpanClosedTag);
            TessaClosingTags.Add(NumberedListTagName, StandardNumberedListClosedTag);
            TessaClosingTags.Add(UnNumberedListTagName, StandardUnNumberedListClosedTag);
            TessaClosingTags.Add(ListItemTagName, TessaListItemClosedTag);
            TessaClosingTags.Add(PreTagName, TessaPreCloseTag);
            TessaClosingTags.Add(CodeTagName, TessaPreCloseTag);
            TessaClosingTags.Add(CitationTagName, TessaCitationCloseTag);
        }

        #endregion

        /// <summary>
        /// Преобразование исходной строки в стандартный HTML формат.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="isTopicText">Истина - преобразование в сообщениях.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string TextileParseString(string mainString, bool isTopicText = false)
        {
            // преобразуем "пользовательские символы" в html символы.
            mainString = mainString.Replace("--->", "&#129042;");
            mainString = mainString.Replace("---", "&mdash;");
            mainString = mainString.Replace("->", "&#129046;");
            
            // если строка начинается с цитаты,
            // для верной отработки регулярного выражения добавляем:
            // - в начало комбинацию "новая строка";
            if (mainString[0] == '>')
            {
                mainString = mainString.Insert(0, "\r\n");
            }
            // - в конец символы переноса строки.
            mainString += "\r\n";

            // все блоки кода приводим к единому егу "@code" и "/@code".
            mainString = ParsePreCodeTags(mainString);
            mainString = ParseCollapseTags(mainString);
            // преобразуем символы "<" и ">" в символы "&lt;" и "&gt;".
            mainString = mainString.Replace("<", @"&lt;");
            mainString = mainString.Replace(">", @"&gt;");
            
            mainString = mainString.Replace("[", @"&#91;");
            mainString = mainString.Replace("]", @"&#93;");
            // преобразуем собственные теги кода в теги <code>.
            mainString = mainString.Replace("@code", "<code>");
            mainString = mainString.Replace("@/code", "</code>");
            if (isTopicText)
            {
                mainString = ParseCitationSection(mainString);
            }
            // преобразуем строку в стандартный HTML.
            mainString = TextileToHTML.TextileFormatter.FormatString(mainString);

            mainString = RemoveSymbolNewString(mainString);

            // подчищаем символ "&amp;", которые генерировался в процессе преобразования textile в HTML.
            mainString = mainString.Replace("&amp;", "&");

            return mainString;
        }

        /// <summary>
        /// Преобразование стандартного HTML в платформенный HTML.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="issueDirectory">Расположение объекта "Инцидент" из редмайна.</param>
        /// <param name="isTopicText">Это сообщение топика.</param>
        /// <returns>Преобразованная строка.</returns>
        private string StandardHtmlParseString(
            string mainString,
            string issueDirectory,
            bool isTopicText = false)
        {
            if (!isTopicText)
            {
                mainString = ParseSlashesSymbol(mainString);
            }

            foreach (var tag in TessaOpeningTags)
            {
                mainString = ParseTag(mainString, tag);
            }

            // преобразуем тег <br /> в </p><p>.
            mainString = mainString.Replace(StandardNewLineTag, TessaNewLineTag, StringComparison.CurrentCulture);
            
            // преобразуем заголовки.
            mainString = ParseHeaderString(mainString);

            while (TryGetMatсhes(mainString, isTopicText ? _imagesTopicTag : _imagesDescriptionTag, out var matches))
            {
                mainString = ParseAttachmentImages(mainString, matches[0], issueDirectory, isTopicText);
            }

            mainString = isTopicText switch
            {
                // т.к верстка в сообщениях отличается от верстки в описании
                // преобразуем код "&#8220" и "&#8221" в символы "\\\"".
                false => ParseQuotesSymbol(mainString),
                true => mainString.Replace("\\\"", "\"")
            };

            mainString = ParsingHttpLink(mainString, isTopicText);

            // инлайн код блоки превращаем в жирный текст.
            mainString = ParseInlineTags(mainString);

            // установка начала и конца строки.
            mainString = SetPreAndPostString(mainString, isTopicText);

            return mainString;
        }

        /// <summary>
        /// Заменяет тег стандартного HTML на тег TESSA HTML.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="tag">Ключ и значение стандартного HTML тега.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseTag(string mainString, KeyValuePair<string, string> tag)
        {
            var tagName = tag.Key;

            if (!mainString.Contains(StandardOpeningTags[tag.Key]))
            {
                return mainString;
            }
            
            mainString = mainString.Replace(StandardOpeningTags[tagName], TessaOpeningTags[tagName]);
            mainString = mainString.Replace(StandardClosingTags[tagName], TessaClosingTags[tagName]);

            return mainString;
        }

        /// <summary>
        /// Привести теги "pre code" к промежуточному тегу "@code".
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParsePreCodeTags(string mainString)
        {
            // для корректного преобразования символов сравнения используем собственные теги.
            while (Regex.IsMatch(mainString, preCodeTagsTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                mainString = _preCodeTag.Replace(mainString, "@code");
            }
            mainString = mainString.Replace("</code></pre>", "@/code");
            
            // если в <pre> есть пример тега <pre>
            // - второй тег заключаем к символы html.
            mainString = mainString.Replace("<pre><pre>", "<pre>&lt;pre&gt;");
            mainString = mainString.Replace("</pre></pre>", "&lt;/pre&gt;</pre>");
            
            mainString = mainString.Replace("<pre>", "@code");
            mainString = mainString.Replace("</pre>", "@/code");

            return mainString;
        }

        /// <summary>
        /// Преобразование выделения инлайн блока в выделение жирным.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParseInlineTags(string mainString)
        {
            // сначала для @...@ делаем теги "inlinecode".
            // ">@" - любой тег перед "@".
            // "@<" - любой тег после "@".
            mainString = mainString.Replace(">@", "><inlinecode>");
            mainString = mainString.Replace("@<", "</inlinecode><");

            // меняем все "inlinecode" теги на жирный шрифт.
            mainString = mainString.Replace("<inlinecode>", "<span style=\"font-weight:bold;\">");
            mainString = mainString.Replace("</inlinecode>", "</span>");

            return mainString;
        }

        /// <summary>
        /// Преобразование сворачивающейся секции кода.
        /// В TessaHTML нет сворачивающейся секции. 
        /// </summary>
        /// <param name="mainString"></param>
        /// <returns></returns>
        private static string ParseCollapseTags(string mainString)
        {
            while (TryGetMatсhes(mainString, _collapseTag, out MatchCollection matches))
            {
                foreach (Match match in matches)
                {
                    mainString = mainString.Replace(match.Value, match.Groups[1].Value);
                }
            }

            mainString = mainString.Replace("}}", "");

            return mainString;
        }

        /// <summary>
        /// Генерирует заглушки секций кода.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Шаблоны и результирующую строку для преобразования в секции кода.</returns>
        private static (MatchCollection matches, string resultString) RemoveCodeSection(string mainString)
        {
            (MatchCollection matches, string resultString) result = (null, mainString);

            if (!TryGetMatсhes(mainString, _codeSection, out MatchCollection matches))
            {
                return result;
            }
 
            for (var i = matches.Count - 1; i >= 0; i--)
            {
                mainString = mainString.Replace(matches[i].Value, $"@codeBloc{i}");
            }
            result.matches = matches;
            result.resultString = mainString;

            return result;
        }

        /// <summary>
        /// Завернуть цитаты в блоки.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParseCitationSection(string mainString)
        {
            // преобразовываем внешние цитаты.
            while (TryGetMatсhes(mainString, _citation, out MatchCollection matches))
            {
                var match = matches[0];
                var citationMessage = ParseCitationText(match.Groups[1].Value);
                mainString = !string.IsNullOrWhiteSpace(citationMessage) 
                    ? mainString.Replace(match.Groups[0].Value, $"<citation>{citationMessage}</citation>") 
                    : mainString.Remove(match.Index, match.Length);
            }

            // преобразовываем вложенные цитаты.
            while (TryGetMatсhes(mainString, _nestedCitation, out MatchCollection matches))
            {
                var match = matches[0];
                var citationMessage = match.Groups[1].Value;
                mainString = !string.IsNullOrWhiteSpace(citationMessage) 
                    ? mainString.Replace(match.Groups[0].Value, $"<citation><citation>{citationMessage}</citation></citation>") 
                    : mainString.Remove(match.Index, match.Length);
            }

            return mainString;
        }

        /// <summary>
        /// Заменяет заглушки секций кода на код.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="matches">Шаблоны секций кода.</param>
        /// <returns></returns>
        private static string AddCodeSection(string mainString, MatchCollection matches)
        {
            if (matches == null)
            {
                return mainString;
            }

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                mainString = mainString.Replace($"@codeBloc{i}", matches[i].Value);
            }

            return mainString;
        }

        /// <summary>
        /// Преобразовать один символ "\" в два символа "\\".
        /// </summary>
        private static string ParseSlashesSymbol(string mainString)
        {
            return mainString.Replace(@"\", @"\\");
        }

        /// <summary>
        /// Удаление лишних символов новой строки.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string RemoveSymbolNewString(string mainString)
        {
            MatchCollection matches;

            (matches, mainString) = RemoveCodeSection(mainString);

            //TODO: делаем не через Remove() т.к на "ничего" нельзя заменить.
            var symbolIndex = mainString.IndexOf('\n');
            while (symbolIndex != -1)
            {
                mainString = mainString.Remove(symbolIndex, 1);
                symbolIndex = mainString.IndexOf('\n');
            }

            mainString = AddCodeSection(mainString, matches);

            return mainString;
        }

        /// <summary>
        /// Преобразовать код "&#8220" и "&#8221" в символы "\\\"".
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParseQuotesSymbol(string mainString)
        {
            mainString = mainString.Replace("&#8220;", "\\\"");
            mainString = mainString.Replace("&#8221;", "\\\"");

            return mainString;
        }

        /// <summary>
        /// Преобразование тега "h[1-6]" в жирный 18 шрифт Tessa.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParseHeaderString(string mainString)
        {
            while (Regex.IsMatch(mainString, headerOpenTagTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                mainString = _headerOpenTag.Replace(mainString, "<p><span style=\\\"font-weight:bold;\\\" data-custom-style=\\\"font-size:18;\\\">");
            }
            while (Regex.IsMatch(mainString, headerCloseTagTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                mainString = _headerCloseTag.Replace(mainString, "</span></p>");
            }

            return mainString;
        }

        /// <summary>
        /// Получить совпадения с шаблоном regex в в строке.
        /// </summary>
        /// <param name="mainString">Строка для проверки.</param>
        /// <param name="regex">Регулярное выражение.</param>
        /// <param name="matchCollection">Список совпадений с шаблоном.</param>
        /// <returns>Истина - удалось получить.</returns>
        private static bool TryGetMatсhes(
            string mainString,
            Regex regex,
            out MatchCollection matchCollection)
        {
            matchCollection = regex.Matches(mainString);
            return matchCollection.Count > 0;
        }

        /// <summary>
        /// Преобразование строк с прикрепленными изображениями.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="matchImages">Совпадение с шаблоном регулярного выражения.</param>
        /// <param name="issueDirectory">Расположение файла инцидента.</param>
        /// <param name="isTopicText">Истина - это сообщение из топика.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseAttachmentImages(
            string mainString,
            Match matchImages,
            string issueDirectory,
            bool isTopicText)
        {
            if (matchImages.Groups[1].Value.Contains("http"))
            {
                mainString = mainString.Replace(matchImages.Value, matchImages.Groups[1].Value);
                return mainString;
            }

            var fileName = matchImages.Groups[1].Value;
            AttachedFileIds.Add(AttachedFilesIssue.Where(a => string.Equals(a.Value.Name, fileName, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Key).First());
            
            var fileDirectory = Directory.GetFiles(issueDirectory, AttachedFilesIssue
                .Where(a => string.Equals(a.Value.Name, fileName, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Value.FileName).First());
            
            // Строка генерируется только в описании. Для топика такая строка не нужна.
            if (!isTopicText)
            {
                GenerateAttachmentsString(fileName);
            }

            return ParseAttachments(mainString, fileDirectory[0], fileName, matchImages);
        }

        /// <summary>
        /// Создает строку Attachments.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        private void GenerateAttachmentsString(string fileName)
        {
            var id = AttachedFilesIssue.Where(a => string.Equals(a.Value.Name, fileName, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Key).First();
            var caption = id.ToString().Replace("-", "");
            var uri = $"https:\\\\{caption}";

            if (AttachmentsString != "")
            {
                AttachmentsString += ",";
            }

            AttachmentsString +=
                $"{{\"Caption\":\"{caption}\"," +
                $"\"FileName\":\"\"," +
                $"\"Uri\":\"{uri}\"," +
                $"\"ID::uid\":\"{id}\"," +
                $"\"MessageID::uid\":\"00000000-0000-0000-0000-000000000000\"," +
                $"\"StoreMode::int\":0," +
                $"\"Type::int\":2}}";
        }

        /// <summary>
        /// Преобразуем строку с прикрепленными изображениями.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="fileDirectory">Расположение файла.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="match">Совпадение с шаблоном регулярного выражения.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseAttachments(
            string mainString,
            string fileDirectory,
            string fileName,
            Match match)
        {
            var id =  AttachedFilesIssue.Where(a => string.Equals(a.Value.Name, fileName, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Key).First();
            var mainImage = Image.FromFile(fileDirectory);
            var resizeImage = ResizeImage(mainImage, (int)(mainImage.Width * 0.3), (int)(mainImage.Height * 0.3));

            var base64FileString = GetBase64StringFromImage(resizeImage);

            var textString =
                $"<p><span><img data-custom-style=\\\"width:{resizeImage.Width};height:{resizeImage.Height};\\\" " +
                $"name=\\\"{id:N}\\\" " +
                $"src=\\\"data:image/png;base64,{base64FileString}\\\"></span></p>";

            mainString = mainString.Remove(match.Index, match.Length);
            mainString = mainString.Insert(match.Index, textString);

            return mainString;
        }

        /// <summary>
        /// Изменение размера изображения до указанной ширины и высоты.
        /// </summary>
        /// <param name="image">Изображение.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        /// <returns>Изображение с измененными размерами.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }

        /// <summary>
        /// Получить строку Base64 из изображения.
        /// </summary>
        /// <param name="image">Изображение.</param>
        /// <returns>Строка Base64.</returns>
        private static string GetBase64StringFromImage(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Установка начала и конца строки.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="isTopicText">Истина - сообщение топика.</param>
        private string SetPreAndPostString(string mainString, bool isTopicText)
        {
            if (AttachmentsString != "" && AttachmentsString != "{")
            {
                const string preAttachmentString = "{\"Attachments\":[";
                const string postAttachmentString = "],";

                AttachmentsString = $"{preAttachmentString}{AttachmentsString}{postAttachmentString}";
            }
            else
            {
                AttachmentsString = "{";
            }

            var preString = $"{AttachmentsString}\"Text\":\"<div class=\\\"forum-div\\\">";
            var postString = "</div>\"}";

            if (isTopicText)
            {
                preString = "<div class=\"forum-div\">";
                postString = "</div>";
            }

            mainString = mainString.Insert(0, preString);
            mainString += postString;

            return mainString;
        }

        /// <summary>
        /// Обработка ссылок.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="isTopicText">Истина - сообщение топика.</param>
        /// <returns>Преобразованная строка.</returns>
        private static string ParsingHttpLink(string mainString, bool isTopicText)
        {
            if (!TryGetMatсhes(mainString, _httpLink, out MatchCollection matches))
            {
                return mainString;
            }

            foreach (Match match in matches)
            {
                var matchValue = match.Value.Remove(match.Value.Length - 1, 1);
                var groupValue = match.Groups[1].Value.Replace("&", "&amp;");
                mainString = mainString.Replace(matchValue, GenerateLinkSection(groupValue, isTopicText));
            }

            return mainString;
        }

        /// <summary>
        /// Генерирование разметки для ссылки.
        /// </summary>
        /// <param name="link">Ссылка.</param>
        /// <param name="isTopicText">Истина - сообщение топика.</param>
        /// <returns>Строка разметки ссылки.</returns>
        private static string GenerateLinkSection(string link, bool isTopicText)
        {
            return !isTopicText 
                ? $"</span><a data-custom-href=\\\"{link}\\\" href=\\\"{link}\\\" class=\\\"forum-url\\\"><span>{link}</span></a><span>" 
                : $"</span><a data-custom-href=\"{link}\" href=\"{link}\" class=\"forum-url\"><span>{link}</span></a><span>";
        }

        /// <summary>
        /// Преобразует символы в тексте цитаты в html код.
        /// </summary>
        /// <param name="mainString"></param>
        /// <returns></returns>
        private static string ParseCitationText(string mainString)
        {
            // чтобы внутри цитаты не парсился тег <a>.
            mainString = mainString.Replace(":", "&#58;");

            return mainString;
        }

        #endregion
    }
}
