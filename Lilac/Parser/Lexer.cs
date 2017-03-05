using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lilac.Exceptions;

namespace Lilac.Parser
{
    public class Lexer : ILexer
    {

        public Lexer(ITokenDefiner tokenDefiner)
        {
            TokenDefinitions = tokenDefiner.GetTokenDefinitions().OrderBy(def => def.Priority).ToList();
            TabWidth = 4;
            Regex = new Regex(string.Join("|",
                TokenDefinitions.Select(td => $"(?<{td.TokenType}>{td.Regex})")) + "|(?<Unrecognized>.)",
                RegexOptions.ExplicitCapture);
        }

        private List<TokenDefinition> TokenDefinitions { get; }
        public int TabWidth { get; set; }
        private Regex Regex { get; set; }
        private int Line { get; set; }
        private Stack<int> Indentations { get; set; }

        private Token NewToken(TokenType type, string content, int column = 0)
        {
            return new Token { TokenType = type, Content = content, Column = column, Line = Line };
        }

        public IEnumerable<Token> Tokenize(TextReader reader)
        {
            Indentations = new Stack<int>();
            Indentations.Push(0);
            Line = 0;

            yield return NewToken(TokenType.OpenGroup, "bof");

            foreach (var token in TokenizeLines(reader))
                yield return token;

            foreach (var token in CloseHangingIndents())
                yield return token;

            yield return NewToken(TokenType.CloseGroup, "eof");
        }

        private IEnumerable<Token> TokenizeLines(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ++Line;
                foreach (var token in TokenizeLine(line))
                    yield return token;
            }
        }

        private IEnumerable<Token> CloseHangingIndents()
        {
            while (Indentations.Count > 1)
            {
                Indentations.Pop();
                yield return NewToken(TokenType.CloseGroup, "dedent");
                yield return NewToken(TokenType.Newline, string.Empty);
            }
        }


        private IEnumerable<Token> TokenizeLine(string line)
        {
            var trimmed = line.Trim();
            var lineTokens = GetLineTokens(trimmed).ToList();
            if (!lineTokens.Any()) yield break;

            foreach (var token in GetIndentationTokens(line))
                yield return token;

            foreach (var token in lineTokens)
                yield return NewToken(token.TokenType, token.Content, token.Column + Indentations.Peek());

            yield return NewToken(TokenType.Newline, string.Empty, trimmed.Length + Indentations.Peek());
        }

        private IEnumerable<Token> GetLineTokens(string line) =>
            from Match match in Regex.Matches(line)
            let definition = GetTokenDefinition(match)
            where !definition.IsIgnored
            let column = match.Index
            select NewToken(definition.TokenType, match.Value, column);

        private TokenDefinition GetTokenDefinition(Match match)
        {
            if (match.Groups["Unrecognized"].Success)
                throw new SyntaxException($"Unrecognized token '{match.Value}'.");
            return TokenDefinitions.Find(definition => match.Groups[definition.TokenType.ToString()].Success);
        }

        private IEnumerable<Token> GetIndentationTokens(string line)
        {
            var indent = line.TakeWhile(char.IsWhiteSpace).Sum(c => c == '\t' ? TabWidth : 1);
            var currentIndent = Indentations.Peek();

            if (indent == currentIndent)
                yield break;

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