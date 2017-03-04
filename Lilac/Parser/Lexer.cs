using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lilac.Exceptions;

namespace Lilac.Parser
{
    public class Lexer : ILexer
    {
        private List<TokenDefinition> TokenDefinitions { get; }

        public Lexer(ITokenDefiner tokenDefiner)
        {
            TokenDefinitions = tokenDefiner.GetTokenDefinitions().OrderBy(def => def.Priority).ToList();
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

            if (string.IsNullOrWhiteSpace(line)) yield break;

            foreach (var token in GetIndentationTokens(line)) yield return token;

            var trimmed = line.Trim();

            foreach (var token in GetLineTokens(trimmed)) yield return token;

            yield return NewToken(TokenType.Newline, string.Empty, trimmed.Length + Indentations.Peek());
        }

        private IEnumerable<Token> GetLineTokens(string line) =>
            from Match match in Regex.Matches(line)
            let definition = GetTokenDefinition(match)
            where !definition.IsIgnored
            let column = Indentations.Peek() + match.Index
            select NewToken(definition.TokenType, match.Value, column);

        private TokenDefinition GetTokenDefinition(Match match)
        {
            if (match.Groups["Unrecognized"].Success)
            {
                throw new SyntaxException($"Unrecognized token '{match.Value}'.");
            }
            return TokenDefinitions.Find(definition => match.Groups[definition.TokenType.ToString()].Success);
        }

        private IEnumerable<Token> GetIndentationTokens(string line)
        {
            var indent = line.TakeWhile(char.IsWhiteSpace).Sum(c => c == '\t' ? TabWidth : 1);
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
    }
}