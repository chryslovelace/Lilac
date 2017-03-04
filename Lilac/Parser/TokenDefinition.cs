using JetBrains.Annotations;

namespace Lilac.Parser
{
    public class TokenDefinition
    {
        public string Regex { get; }
        public TokenType TokenType { get; }
        public int Priority { get; }
        public bool IsIgnored { get; }

        public TokenDefinition(TokenType type, [RegexPattern] string regex, bool isIgnored = false, int priority = 0)
        {
            TokenType = type;
            Regex = regex;
            IsIgnored = isIgnored;
            Priority = priority;
        }
    }
}