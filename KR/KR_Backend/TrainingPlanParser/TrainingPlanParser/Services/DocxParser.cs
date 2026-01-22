using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using System.Text.RegularExpressions;

namespace TrainingPlanParser.Services
{
    public class DocxParser
    {
        public (string rawText, string structured) ExtractBoth(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"DOCX not found at: {filePath}");

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            StringBuilder raw = new();
            StringBuilder structured = new();

            foreach (var element in body.ChildElements)
            {
                switch (element)
                {
                    case Paragraph p:
                        ProcessParagraph(p, raw, structured);
                        break;

                    case Table tbl:
                        ProcessTable(tbl, raw, structured);
                        break;
                }
            }

            return (raw.ToString(), structured.ToString());
        }

        // PARAGRAPH PROCESSING
        private void ProcessParagraph(Paragraph para, StringBuilder raw, StringBuilder structured)
        {
            string text = para.InnerText?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            raw.AppendLine(text);

            int heading = GetHeadingLevel(para);
            // SAFE WEEK DETECTION 
            if (heading == 1 && LooksLikeValidWeekHeader(text))
            {
                structured.AppendLine($"[WEEK name=\"{text}\"]");
                return;
            }
            // MODULE HEADERS
            if (heading == 2)
            {
                structured.AppendLine($"[MODULE] {text}");
                return;
            }

            // TOPIC HEADERS
            if (heading == 3)
            {
                structured.AppendLine($"[TOPIC] {text}");
                return;
            }
            // LIST ITEMS
            if (IsListItem(para))
            {
                int level = GetListLevel(para);
                structured.AppendLine($"[LIST level=\"{level}\"] {text}");
                return;
            }
            structured.AppendLine($"[PARAGRAPH] {text}");
        }
        // TABLE PROCESSING — WITH CORRECT WEEK DETECTION
        private void ProcessTable(Table table, StringBuilder raw, StringBuilder structured)
        {
            structured.AppendLine("[TABLE-START]");

            foreach (var row in table.Elements<TableRow>())
            {
                structured.AppendLine("  [ROW]");

                var cells = row.Elements<TableCell>().ToList();

                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = cells[i];
                    string cellText = string.Join(" ",
                        cell.Descendants<Paragraph>().Select(p => p.InnerText.Trim()));

                    if (string.IsNullOrWhiteSpace(cellText))
                        continue;

                    raw.AppendLine(cellText);

                    // SAFE WEEK DETECTION INSIDE TABLE CELLS
                    if (LooksLikeValidWeekHeader(cellText))
                    {
                        structured.AppendLine($"    [WEEK name=\"{cellText}\"]");
                        continue;
                    }

                    structured.AppendLine($"    [CELL index=\"{i}\"] {cellText}");
                }

                structured.AppendLine("  [/ROW]");
            }

            structured.AppendLine("[TABLE-END]");
        }
        private bool LooksLikeValidWeekHeader(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            // Match only true headers:
            // Week 1
            // Week 2 Content
            // Week 3 - Details
            // WEEK 4:
            // Week 5 Overview
            string pattern = @"^week\s*\d+(\s*[\w\-\:\(\)]*)?$";

            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private int GetHeadingLevel(Paragraph para)
        {
            var props = para.ParagraphProperties;
            string style = props?.ParagraphStyleId?.Val?.Value ?? "";

            if (style.StartsWith("Heading") &&
                int.TryParse(style.Replace("Heading", ""), out int lvl))
                return lvl;

            return 0;
        }

        private bool IsListItem(Paragraph para)
        {
            var props = para.ParagraphProperties;
            return props?.NumberingProperties?.NumberingId != null;
        }

        private int GetListLevel(Paragraph para)
        {
            var props = para.ParagraphProperties;
            return (int)(props?.NumberingProperties?.NumberingLevelReference?.Val ?? 0);
        }
    }
}
