using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

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
        /// </summary>
        private List<string> AttachedFileString = new List<string>();

        /// <summary>
        /// Открытие тегов стандартного HTML.
        /// </summary>
        private Dictionary<string, string> StandartOpeningTags = new Dictionary<string, string>();

        /// <summary>
        /// Закрытие тегов стандартного HTML.
        /// </summary>
        private Dictionary<string, string> StandartClosingTags = new Dictionary<string, string>();

        /// <summary>
        /// Открытие тегов HTML Tessa.
        /// </summary>
        private Dictionary<string, string> TessaOpeningTags = new Dictionary<string, string>();

        /// <summary>
        /// Закрытие тегов HTML Tessa.
        /// </summary>
        private Dictionary<string, string> TessaClosingTags = new Dictionary<string, string>();

        /// <summary>
        /// Список прикрепленных файлов.
        /// </summary>
        private Dictionary<string, Guid> AttachedFilesIssue = new Dictionary<string, Guid>();

        #region Tags Names

        private readonly string BoldItalicTagName = "BoldItalicTag";
        private readonly string BoldTagName = "BoldTag";
        private readonly string ItalicTagName = "ItalicTag";
        private readonly string UnderlineTagName = "UnderlineTag";
        private readonly string CrossedOutTagName = "CrossedOutTag";
        private readonly string NumberedListTagName = "NumberedListTag";
        private readonly string UnNumberedListTagName = "UnNumberedListTag";
        private readonly string ListItemTagName = "ListItemTag";
        private readonly string ParagraphTagName = "ParagraphTag";
        private readonly string PreTagName = "PreTag";
        private readonly string CodeTagName = "CodeTag";
        private readonly string CitationTagName = "CitationTag";

        #endregion

        #region Tags

        private string SpanClosedTag = "</span>";

        #region Standart Tags

        private readonly string StandartBoldItalicTag = "<strong><em>";
        private readonly string StandartBoldTag = "<strong>";
        private readonly string StandartItalicTag = "<em>";
        private readonly string StandartUnderlineTag = "<ins>";
        private readonly string StandartCrossedOutTag = "<del>";
        private readonly string StandartNumberedListTag = "<ol>";
        private readonly string StandartUnNumberedListTag = "<ul>";
        private readonly string StandartListItemTag = "<li>";
        private readonly string StandartParagraphTag = "<p>";
        private readonly string StandartPreTag = "<pre>";
        private readonly string StandartCodeTag = "<code>";
        private readonly string StandartCitationTag = "<citation>";

        private readonly string StandartBoldItalicClosedTag = "</strong></em>";
        private readonly string StandartBoldClosedTag = "</strong>";
        private readonly string StandartItalicClosedTag = "</em>";
        private readonly string StandartUnderlineClosedTag = "</ins>";
        private readonly string StandartCrossedOutClosedTag = "</del>";
        private readonly string StandartNumberedListClosedTag = "</ol>";
        private readonly string StandartUnNumberedListClosedTag = "</ul>";
        private readonly string StandartListItemClosedTag = "</li>";
        private readonly string StandartParagraptClosedTag = "</p>";
        private readonly string StandartPreClosedTag = "</pre>";
        private readonly string StandartCodeClosedTag = "</code>";
        private readonly string StandartCitationCloseTag = "</citation>";

        private string StandartNewLineTag = "<br />";

        #endregion

        #region Tessa Tags

        private readonly string TessaBoldItalicTag = "</span><span style=\\\"font-weight:bold;font-style:italic;\\\">";
        private readonly string TessaBoldTag = "</span><span style=\\\"font-weight:bold;\\\">";
        private readonly string TessaItalicTag = "</span><span style=\\\"font-style:italic;\\\">";
        private readonly string TessaUnderlineTag = "</span><span style=\\\"text-decoration:underline;\\\">";
        private readonly string TessaCrossedOutTag = "</span><span style=\\\"text-decoration:line-through;\\\">";
        private readonly string TessaNumberedListTag = "</span><ol class=\\\"forum-ol\\\">";
        private readonly string TessaUnNumberedListTag = "</span><ul class=\\\"forum-ul\\\">";
        private readonly string TessaListItemTag = "<li><p><span>";
        private readonly string TessaParagraphTag = "<p><span>";
        private readonly string TessaPreTag = "<div class=\\\"forum-block-monospace\\\"><p><span>";
        private readonly string TessaCitationTag = "<div class=\"forum-quote\"><p><span>";

        private readonly string TessaListItemClosedTag = "</span></p></li>";
        private readonly string TessaParagraphClosedTag = "</span></p>";
        private readonly string TessaPreCloseTag = "</span></p></div>";
        private readonly string TessaCitationCloseTag = "</span></p></div>";

        private readonly string TessaNewLineTag = "</span></p><p><span>";

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
        /// Шаблон регулярного выражения для тега изображения.
        /// </summary>
        private static readonly string imagesTagTemplate = "<img src=\"(.*?)\" .*? />";
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
        private static readonly string citationTemplates = "\\n&gt; (.*?)\\r";

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
        /// Регулярное выражение для секции тега изображения.
        /// </summary>
        private static readonly Regex _imagesTag = new Regex(imagesTagTemplate,
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
        private static readonly Regex _citation = new Regex(citationTemplates,
           RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Создать объект класса "Parser".
        /// </summary>
        public Parser()
        {
            this.InitialStandartTags();
            this.InitialTessaTags();
        }

        #endregion

        /// <summary>
        /// Получить преобразованную из "Textile" в "платформенные HTML" строку и список имен прикреаленных к этой строке файлов.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="issueDirectory">Расположение объекта "Инцидент" из редмайна.</param>
        /// <param name="attachedFileIssue">Прикрепленные к инциденту файлы.</param>
        /// <param name="isTopicText">Истина - это сообщение топика.</param>
        /// <returns>Преобразованная строка и список имен прикреаленных к этой строке файлов.</returns>
        public (string resultStrig, List<string> attachedFilesString) GetParseToTessaHTMLString(
            string mainString,
            string issueDirectory,
            Dictionary<string, Guid> attachedFileIssue,
            bool isTopicText = false)
        {
            this.AttachedFilesIssue = attachedFileIssue;
            this.AttachmentsString = "";

            var resultString = this.TextileParseString(mainString, isTopicText);
            if (!isTopicText)
            {
                resultString = resultString.Replace("\"", "&#8220;");
            }
            resultString = this.StandartHTMLParseString(
                resultString,
                issueDirectory,
                isTopicText);
            resultString = resultString.Replace("\n", TessaNewLineTag);

            return (resultString, this.AttachedFileString);
        }

        #region Private methods

        #region Initial methods

        /// <summary>
        /// Инициализация стандартных HTML тегов.
        /// </summary>
        private void InitialStandartTags()
        {
            StandartOpeningTags.Clear();
            StandartClosingTags.Clear();

            // тут важен порядок добавления.
            StandartOpeningTags.Add(ParagraphTagName, StandartParagraphTag);
            StandartOpeningTags.Add(BoldItalicTagName, StandartBoldItalicTag);
            StandartOpeningTags.Add(BoldTagName, StandartBoldTag);
            StandartOpeningTags.Add(ItalicTagName, StandartItalicTag);
            StandartOpeningTags.Add(UnderlineTagName, StandartUnderlineTag);
            StandartOpeningTags.Add(CrossedOutTagName, StandartCrossedOutTag);
            StandartOpeningTags.Add(NumberedListTagName, StandartNumberedListTag);
            StandartOpeningTags.Add(UnNumberedListTagName, StandartUnNumberedListTag);
            StandartOpeningTags.Add(ListItemTagName, StandartListItemTag);
            StandartOpeningTags.Add(PreTagName, StandartPreTag);
            StandartOpeningTags.Add(CodeTagName, StandartCodeTag);
            StandartOpeningTags.Add(CitationTagName, StandartCitationTag);

            StandartClosingTags.Add(ParagraphTagName, StandartParagraptClosedTag);
            StandartClosingTags.Add(BoldItalicTagName, StandartBoldItalicClosedTag);
            StandartClosingTags.Add(BoldTagName, StandartBoldClosedTag);
            StandartClosingTags.Add(ItalicTagName, StandartItalicClosedTag);
            StandartClosingTags.Add(UnderlineTagName, StandartUnderlineClosedTag);
            StandartClosingTags.Add(CrossedOutTagName, StandartCrossedOutClosedTag);
            StandartClosingTags.Add(NumberedListTagName, StandartNumberedListClosedTag);
            StandartClosingTags.Add(UnNumberedListTagName, StandartUnNumberedListClosedTag);
            StandartClosingTags.Add(ListItemTagName, StandartListItemClosedTag);
            StandartClosingTags.Add(PreTagName, StandartPreClosedTag);
            StandartClosingTags.Add(CodeTagName, StandartCodeClosedTag);
            StandartClosingTags.Add(CitationTagName, StandartCitationCloseTag);
        }

        /// <summary>
        /// Инициализация HTML тегов TESSA.
        /// </summary>
        private void InitialTessaTags()
        {
            TessaOpeningTags.Clear();
            TessaClosingTags.Clear();

            TessaOpeningTags.Add(ParagraphTagName, TessaParagraphTag);
            TessaOpeningTags.Add(BoldItalicTagName, TessaBoldItalicTag);
            TessaOpeningTags.Add(BoldTagName, TessaBoldTag);
            TessaOpeningTags.Add(ItalicTagName, TessaItalicTag);
            TessaOpeningTags.Add(UnderlineTagName, TessaUnderlineTag);
            TessaOpeningTags.Add(CrossedOutTagName, TessaCrossedOutTag);
            TessaOpeningTags.Add(NumberedListTagName, TessaNumberedListTag);
            TessaOpeningTags.Add(UnNumberedListTagName, TessaUnNumberedListTag);
            TessaOpeningTags.Add(ListItemTagName, TessaListItemTag);
            TessaOpeningTags.Add(PreTagName, TessaPreTag);
            TessaOpeningTags.Add(CodeTagName, TessaPreTag);
            TessaOpeningTags.Add(CitationTagName, TessaCitationTag);

            TessaClosingTags.Add(ParagraphTagName, TessaParagraphClosedTag);
            TessaClosingTags.Add(BoldItalicTagName, SpanClosedTag);
            TessaClosingTags.Add(BoldTagName, SpanClosedTag);
            TessaClosingTags.Add(ItalicTagName, SpanClosedTag);
            TessaClosingTags.Add(UnderlineTagName, SpanClosedTag);
            TessaClosingTags.Add(CrossedOutTagName, SpanClosedTag);
            TessaClosingTags.Add(NumberedListTagName, StandartNumberedListClosedTag);
            TessaClosingTags.Add(UnNumberedListTagName, StandartUnNumberedListClosedTag);
            TessaClosingTags.Add(ListItemTagName, TessaListItemClosedTag);
            TessaClosingTags.Add(PreTagName, TessaPreCloseTag);
            TessaClosingTags.Add(CodeTagName, TessaPreCloseTag);
            TessaClosingTags.Add(CitationTagName, TessaCitationCloseTag);
        }

        #endregion

        /// <summary>
        /// Преобразование исходной строки в стандартый HTML формат.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="isTopicText">Истина - преобразование в сообщениях.</param>
        /// <returns>Преобразованная строка.</returns>
        private string TextileParseString(string mainString, bool isTopicText = false)
        {
            string resultString = mainString;

            // инлайн код блоки превращаем в жирный текст.
            resultString = this.ParseInlineTags(resultString);
            // все блоки кода приводим к единому егу "@code" и "/@code".
            resultString = this.ParsePreCodeTags(resultString);
            resultString = this.ParseCollapseTags(resultString);
            // преобразуем символы "<" и ">" в символы "&lt;" и "&gt;".
            resultString = resultString.Replace("<", @"&lt;");
            resultString = resultString.Replace(">", @"&gt;");
            // преобразуем собсвенные теги кода в теги <code>.
            resultString = resultString.Replace("@code", "<code>");
            resultString = resultString.Replace("@/code", "</code>");
            if (isTopicText)
            {
                resultString = this.ParseCitationSection(resultString);
            }
            // преобразуем строку в стандартный HTML.
            resultString = TextileToHTML.TextileFormatter.FormatString(resultString);

            resultString = this.RemoveSumbolNewString(resultString);

            // подчищаем символ "&amp;", которые сгенирировался в процессе преобразования textile в HTML.
            resultString = resultString.Replace("&amp;", "&");

            return resultString;
        }

        /// <summary>
        /// Преобразование стандартного HTML в платформенный HTML.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <param name="issueDirectory">Расположение объекта "Инцидент" из редмайна.</param>
        /// <param name="isTopicText">Это сообщение топика.</param>
        /// <returns>Преобразованная строка.</returns>
        private string StandartHTMLParseString(
            string mainString,
            string issueDirectory,
            bool isTopicText = false)
        {
            string resultString = mainString;

            resultString = this.ParseSlashesSyblol(resultString);

            foreach (var tag in TessaOpeningTags)
            {
                resultString = this.ParseTag(resultString, tag);
            }

            // преобразуем тег <br /> в </p><p>.
            resultString = this.ParseNewLineTag(resultString);

            // т.к верстка в сообщениях отличается от вестки в описании
            // преобразуем код "&#8220" и "&#8221" в символы "\\\"".
            if (!isTopicText)
            {
                resultString = this.ParseQuotesSymbol(resultString);
            }

            // преобразуем заголовки.
            resultString = this.ParseHeaderString(resultString);

            this.AttachedFileString.Clear();
            while (this.TryGetMathes(resultString, _imagesTag, out MatchCollection matches))
            {
                resultString = this.ParseAttachmentImages(resultString, matches[0], issueDirectory, isTopicText);
            }

            if (isTopicText)
            {
                resultString = this.RemoveSlachSyblol(resultString);
            }

            resultString = this.ParsingHttpLink(resultString, isTopicText);

            // установка начала и конца строки.
            resultString = this.SetPreAndPostString(resultString, isTopicText);

            return resultString;
        }

        /// <summary>
        /// Заменяет тег стандартного HTML на тег TESSA HTML.
        /// </summary>
        /// <param name="tag">Ключ и значение стандартного HTML тега.</param>
        private string ParseTag(string mainString, KeyValuePair<string, string> tag)
        {
            string resultString = mainString;
            string tagName = tag.Key;

            if (resultString.Contains(StandartOpeningTags[tag.Key]))
            {
                resultString = resultString.Replace(StandartOpeningTags[tagName], TessaOpeningTags[tagName]);
                resultString = resultString.Replace(StandartClosingTags[tagName], TessaClosingTags[tagName]);
            }

            return resultString;
        }

        /// <summary>
        /// Привести теги "pre code" к промежуточному тегу "@code".
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParsePreCodeTags(string mainString)
        {
            string resultString = mainString;
            // для корректного преобразования символов сравнения используем собственные теги.
            while (Regex.IsMatch(resultString, preCodeTagsTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                resultString = _preCodeTag.Replace(resultString, "@code");
            }
            resultString = resultString.Replace("</code></pre>", "@/code");
            resultString = resultString.Replace("<pre>", "@code");
            resultString = resultString.Replace("</pre>", "@/code");

            return resultString;
        }

        /// <summary>
        /// Преобразование выделения инлайн блока в выделение жирным.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseInlineTags(string mainString)
        {
            string resultString = mainString;

            resultString = resultString.Replace("@", "*");

            return resultString;
        }

        /// <summary>
        /// Преобразование сворачивающейся секции кода.
        /// В TessaHTML нет сворачивающейся секции. 
        /// </summary>
        /// <param name="mainString"></param>
        /// <returns></returns>
        private string ParseCollapseTags(string mainString)
        {
            string resultString = mainString;

            while (this.TryGetMathes(resultString, _collapseTag, out MatchCollection matches))
            {
                foreach (Match match in matches)
                {
                    resultString = resultString.Replace(match.Value, match.Groups[1].Value);
                }
            }

            resultString = resultString.Replace("}}", "");

            return resultString;
        }

        /// <summary>
        /// Генерирует заглушки секций кода.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Шаблоны и результирующую строку для преобразования в секции кода.</returns>
        private (MatchCollection matches, string resultString) RemoveCodeSection(string mainString)
        {
            string resultString = mainString;
            (MatchCollection matches, string resultString) result = (null, resultString);
            if (this.TryGetMathes(mainString, _codeSection, out MatchCollection matches))
            {
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    resultString = resultString.Replace(matches[i].Value, $"@codeBloc{i}");
                }
                result.matches = matches;
                result.resultString = resultString;
            }

            return result;
        }

        /// <summary>
        /// Завернуть цитаты в блоки.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseCitationSection(string mainString)
        {
            string resultString = mainString;

            while (this.TryGetMathes(resultString, _citation, out MatchCollection matches))
            {
                var match = matches[0];
                var citationMessage = match.Groups[1].Value;
                if (citationMessage != "")
                {
                    resultString = resultString.Replace(match.Value, $"<citation>{citationMessage}</citation>");
                }
                else
                {
                    resultString = resultString.Remove(match.Index, match.Length);
                }
            }

            return resultString;
        }

        /// <summary>
        /// Заменяет заглушки секций кода на код.
        /// </summary>
        /// <param name="mainString">Строка для преобразованя.</param>
        /// <param name="matches">Шаблоны секций кода.</param>
        /// <returns></returns>
        private string AddCodeSection(string mainString, MatchCollection matches)
        {
            if (matches == null)
            {
                return mainString;
            }

            string resultString = mainString;

            for (int i = matches.Count - 1; i >= 0; i--)
            {
                resultString = resultString.Replace($"@codeBloc{i}", matches[i].Value);
            }

            return resultString;
        }

        /// <summary>
        /// Преобзазовывает один символ "\" в два символа "\\".
        /// </summary>
        private string ParseSlashesSyblol(string mainString)
        {
            return mainString.Replace(@"\", @"\\");
        }

        /// <summary>
        /// Удаление лишних символов новой строки.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string RemoveSumbolNewString(string mainString)
        {
            string resultString = mainString;
            MatchCollection matches;

            (matches, resultString) = this.RemoveCodeSection(mainString);

            //TODO: делаем не через Remove() т.к на "ничего" нельзя заменить.
            var symbolIndex = resultString.IndexOf('\n');
            while (symbolIndex != -1)
            {
                resultString = resultString.Remove(symbolIndex, 1);
                symbolIndex = resultString.IndexOf('\n');
            }

            resultString = this.AddCodeSection(resultString, matches);

            return resultString;
        }

        /// <summary>
        /// Преобразование тега новой строки из стандартного HTML в TESSA HTML.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseNewLineTag(string mainString)
        {
            string resultString = mainString;

            if (resultString.Contains(StandartNewLineTag))
            {
                resultString = resultString.Replace(StandartNewLineTag, TessaNewLineTag);
            }

            return resultString;
        }

        /// <summary>
        /// Преобразовать код "&#8220" и "&#8221" в символы "\\\"".
        /// </summary>
        /// <param name="mainString">Строка для преобразоваия.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseQuotesSymbol(string mainString)
        {
            string resultString = mainString;

            resultString = resultString.Replace("&#8220;", "\\\"");
            resultString = resultString.Replace("&#8221;", "\\\"");

            return resultString;
        }

        /// <summary>
        /// Преобразование тега "h[1-6]" в жирный 18 шрифт Tessa.
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string ParseHeaderString(string mainString)
        {
            string resultString = mainString;

            while (Regex.IsMatch(resultString, headerOpenTagTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                resultString = _headerOpenTag.Replace(resultString, "<p><span style=\\\"font-weight:bold;\\\" data-custom-style=\\\"font-size:18;\\\">");
            }
            while (Regex.IsMatch(resultString, headerCloseTagTemplate, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled))
            {
                resultString = _headerCloseTag.Replace(resultString, "</span></p>");
            }

            return resultString;
        }

        /// <summary>
        /// Получить совпадения с шаблоном regex в в строке.
        /// </summary>
        /// <param name="mainString">Строка для проверки.</param>
        /// <param name="_regex">Регулярное выражение.</param>
        /// <param name="matchCollection">Списко совпадений с шаблоном.</param>
        /// <returns>Истина - удалось получить.</returns>
        private bool TryGetMathes(
            string mainString,
            Regex _regex,
            out MatchCollection matchCollection)
        {
            matchCollection = _regex.Matches(mainString);
            if (matchCollection.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Преобразование строк с прикрепленными изображениями.
        /// </summary>
        /// <param name="mainString">Сторока для преобразования.</param>
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
                string resultString = mainString;
                resultString = resultString.Replace(matchImages.Value, matchImages.Groups[1].Value);
                return resultString;
            }
            string fileName = matchImages.Groups[1].Value.ToLower();
            this.AttachedFileString.Add(fileName);
            // TODO: т.к файлы с id перед наименованием - получаем путь к файлы с помощью Directory.
            var fileDirectory = Directory.GetFiles(issueDirectory, $"*_{fileName}");
            // Строка генерируется только в описании. Для топика такая строка не нужна.
            if (!isTopicText)
            {
                this.GenerateAttachemntsString(fileName);
            }

            return this.ParseAttachments(mainString, fileDirectory[0], fileName, isTopicText, matchImages);
        }

        /// <summary>
        /// Создает строку Attachments.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        private void GenerateAttachemntsString(string fileName)
        {
            var id = this.AttachedFilesIssue[fileName];
            var caption = id.ToString().Replace("-", "");
            var uri = $"https:\\\\{caption}";

            if (this.AttachmentsString != "")
            {
                this.AttachmentsString += ",";
            }

            this.AttachmentsString +=
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
            bool isTopicText,
            Match match)
        {
            string resultString = mainString;

            var id = this.AttachedFilesIssue[fileName];
            string caption;
            if (!isTopicText)
            {
                caption = id.ToString().Replace("-", "");
            }
            else
            {
                caption = fileName;
            }
            var mainImage = Image.FromFile(fileDirectory);
            var resizeImage = this.ResizeImage(mainImage, (int)(mainImage.Width * 0.3), (int)(mainImage.Height * 0.3));

            var base64FileString = this.GetBase64StringFromInage(resizeImage);

            var textString =
                $"<p><span><img data-custom-style=\\\"width:{resizeImage.Width};height:{resizeImage.Height};\\\" " +
                $"name=\\\"{caption}\\\" " +
                $"src=\\\"data:image/png;base64,{base64FileString}\\\"></span></p>";

            resultString = resultString.Remove(match.Index, match.Length);
            resultString = resultString.Insert(match.Index, textString);

            return resultString;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Получить строку Base64 из изображения.
        /// </summary>
        /// <param name="image">Изображение.</param>
        /// <returns>Строка Base64.</returns>
        private string GetBase64StringFromInage(Image image)
        {
            string base64String = null;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                base64String = Convert.ToBase64String(ms.ToArray());
            }

            return base64String;
        }

        /// <summary>
        /// Преобразуем символы "\\" в одиночный символ "\".
        /// </summary>
        /// <param name="mainString">Строка для преобразования.</param>
        /// <returns>Преобразованная строка.</returns>
        private string RemoveSlachSyblol(string mainString)
        {
            return mainString.Replace("\\\"", "\"");
        }

        /// <summary>
        /// Установка начала и конца строки.
        /// </summary>
        private string SetPreAndPostString(string mainString, bool isTopicText)
        {
            string resultString = mainString;

            if (this.AttachmentsString != "" && this.AttachmentsString != "{")
            {
                var preAttachmentString = "{\"Attachments\":[";
                var postAttachmentString = "],";

                this.AttachmentsString = $"{preAttachmentString}{AttachmentsString}{postAttachmentString}";
            }
            else
            {
                this.AttachmentsString = "{";
            }

            var preString = $"{this.AttachmentsString}\"Text\":\"<div class=\\\"forum-div\\\">";
            var postString = "</div>\"}";

            if (isTopicText)
            {
                preString = "<div class=\"forum-div\">";
                postString = "</div>";
            }

            resultString = resultString.Insert(0, preString);
            resultString += postString;

            return resultString;
        }

        /// <summary>
        /// Обработка ссылок.
        /// </summary>
        /// <param name="mainString"></param>
        /// <returns></returns>
        private string ParsingHttpLink(string mainString, bool isTopicText)
        {
            string resultString = mainString;

            if (this.TryGetMathes(resultString, _httpLink, out MatchCollection matches))
            {
                foreach (Match match in matches)
                {
                    var matchValue = match.Value.Remove(match.Value.Length - 1, 1);
                    var groupValue = match.Groups[1].Value.Replace("&", "&amp;");
                    resultString = resultString.Replace(matchValue, this.GenerateLinkSection(groupValue, isTopicText));
                }
            }

            return resultString;
        }

        /// <summary>
        /// Генерирование разметки для ссылки.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string GenerateLinkSection(string link, bool isTopicText)
        {
            if (!isTopicText)
            {
                return $"</span><a data-custom-href=\\\"{link}\\\" href=\\\"{link}\\\" class=\\\"forum-url\\\"><span>{link}</span></a><span>";
            }
            return $"</span><a data-custom-href=\"{link}\" href=\"{link}\" class=\"forum-url\"><span>{link}</span></a><span>";
        }

        #endregion
    }
}
