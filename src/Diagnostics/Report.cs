using System.Text;

namespace AbsoluteFriends.Diagnostics;

internal static class Report
{
    private const string Fence = "```";

    extension(string text)
    {
        public string Item => $"- {text}";

        public string Bold => $"**{text}**";

        public string Code => $"`{text}`";

        public string Heading(int level) => $"{new string('#', level)} {text}";
    }

    extension(IEnumerable<string> values)
    {
        public string Code => string.Join(", ", values.Select(value => value.Code));
    }

    extension(StringBuilder builder)
    {
        public void AppendHeading(string heading, int level)
        {
            builder.AppendLine(heading.Heading(level)).AppendLine();
        }

        public void AppendLines(List<string> lines)
        {
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            builder.AppendLine();
        }

        public void AppendBlock(List<string> lines, string language = "")
        {
            if (lines.Count == 0)
            {
                return;
            }

            builder.AppendLine(Fence + language);

            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            builder.AppendLine(Fence).AppendLine();
        }

        public void AppendSection(string heading, List<string> lines, string language = "")
        {
            if (lines.Count == 0)
            {
                return;
            }

            builder.AppendHeading(heading, 2);
            builder.AppendBlock(lines, language);
        }
    }
}
