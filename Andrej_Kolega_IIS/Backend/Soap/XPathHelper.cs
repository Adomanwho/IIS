using System.Text;

namespace Andrej_Kolega_IIS.Backend.Soap
{
    public static class XPathHelper
    {
        private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZČĆĐŠŽ";
        private const string LowerChars = "abcdefghijklmnopqrstuvwxyzčćđšž";

        public static string ToLowerCaseExpression(string elementName)
        {
            return $"translate({elementName}, '{UpperChars}', '{LowerChars}')";
        }

        public static string ToStringLiteral(string value)
        {
            if (!value.Contains('\''))
            {
                return $"'{value}'";
            }

            var parts = value.Split('\'');
            var builder = new StringBuilder("concat(");
            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", \"'\", ");
                }
                builder.Append('\'').Append(parts[i]).Append('\'');
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
}
