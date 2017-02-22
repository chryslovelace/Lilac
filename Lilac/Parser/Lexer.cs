using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lilac.Exceptions;

namespace Lilac.Parser
{
    public class Lexer
    {
        public List<TokenDefinition> TokenDefinitions { get; }

        public Lexer(IEnumerable<TokenDefinition> definitions)
        {
            TokenDefinitions = definitions.OrderBy(def => def.Priority).ToList();
            TabWidth = 4;
        }

        public int TabWidth { get; set; }
        private Regex Regex { get; set; }
        private int Line { get; set; }
        private Stack<int> Indentations { get; set; }

        private Token NewToken(TokenType type, string content)
        {
            return new Token {TokenType = type, Content = content, Column = 0, Line = Line};
        }
        private Token NewToken(TokenType type, string content, int column)
        {
            return new Token {TokenType = type, Content = content, Column = column, Line = Line};
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            Indentations = new Stack<int>(new []{0});
            Line = 0;
            Regex = new Regex(string.Join("|",
                TokenDefinitions.Select(td => $"(?<{td.TokenType}>{td.Regex})")) + "|(?<Unrecognized>.)",
                RegexOptions.ExplicitCapture);

            yield return NewToken(TokenType.OpenGroup, "bof");

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                foreach (var token in TokenizeLine(line))
                    yield return token;
            }

            while (Indentations.Count > 1)
            {
                Indentations.Pop();
                yield return NewToken(TokenType.CloseGroup, "dedent");
                yield return NewToken(TokenType.Newline, string.Empty);
            }

            yield return NewToken(TokenType.CloseGroup, "eof");
        }
        

        private IEnumerable<Token> TokenizeLine(string line)
        {
            ++Line;

            var tokens = GetLineTokens(line);

            if (!tokens.Any()) yield break;

            foreach (var token in GetIndentationTokens(line)) yield return token;

            foreach (var token in tokens) yield return token;

            yield return NewToken(TokenType.Newline, string.Empty);
        }

        private List<Token> GetLineTokens(string line)
        {
            return GetRegexMatches(line).Select(match =>
                new
                {
                    match.Value,
                    Definition = GetTokenDefinition(match),
                    Column = Indentations.Peek() + match.Index
                })
                .Where(match => !match.Definition.IsIgnored)
                .Select(match => NewToken(match.Definition.TokenType, match.Value, match.Column))
                .ToList();
        }

        private TokenDefinition GetTokenDefinition(Match match)
        {
            if (match.Groups["Unrecognized"].Success)
            {
                throw new SyntaxException($"Unrecognized token '{match.Value}'.");
            }
            return TokenDefinitions.Find(definition => match.Groups[definition.TokenType.ToString()].Success);
        }

        private IEnumerable<Match> GetRegexMatches(string line)
        {
            return Regex.Matches(line.TrimStart(GetLeadingWhitespace(line)))
                .Cast<Match>();
        }

        private IEnumerable<Token> GetIndentationTokens(string line)
        {
            var whitespace = GetLeadingWhitespace(line);
            var indent = GetIndentationLevel(whitespace);
            var currentIndent = Indentations.Peek();

            if (indent == currentIndent) yield break;
            
            if (indent > currentIndent)
            {
                Indentations.Push(indent);
                yield return NewToken(TokenType.OpenGroup, "indent");
            }
            else
            {
                while (indent != Indentations.Peek())
                {
                    Indentations.Pop();
                    yield return NewToken(TokenType.CloseGroup, "dedent");
                    yield return NewToken(TokenType.Newline, string.Empty);
                }
            }
        }

        private static char[] GetLeadingWhitespace(string line)
        {
            return line.TakeWhile(char.IsWhiteSpace).ToArray();
        }

        private int GetIndentationLevel(IEnumerable<char> whitespace)
        {
            return whitespace.Sum(c =>
            {
                switch (c)
                {
                    case '\t': return TabWidth;
                    case ' ': return 1;
                    default: return 0;
                }
            });
        }
    }
}