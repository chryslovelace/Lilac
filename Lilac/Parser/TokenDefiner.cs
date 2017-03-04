using System.Collections.Generic;
using System.Linq;

namespace Lilac.Parser
{
    public class TokenDefiner : ITokenDefiner
    {
        private static readonly TokenDefinition[] TokenDefinitions =
        {
            new TokenDefinition(TokenType.DecimalNumber, @"[+-]?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+)?", priority: -1),
            new TokenDefinition(TokenType.BinaryNumber, @"0[bB][01]+", priority: -2),
            new TokenDefinition(TokenType.HexNumber, @"0[xX][0-9a-fA-F]+", priority: -2),
            new TokenDefinition(TokenType.RationalNumber, @"[+-]?[0-9]+\s*\/\s*[0-9]+", priority: -2),
            new TokenDefinition(TokenType.ComplexNumber, @"[+-]?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+)?\s*[+-]\s*[0-9]+(\.[0-9]+)?([eE]-?[0-9]+)?i", priority: -2),
            new TokenDefinition(TokenType.Identifier, @"[^\s[\](){},\.""';`]+"),
            new TokenDefinition(TokenType.String, @"""(\\""|[^""])*"""),
            new TokenDefinition(TokenType.OpenGroup, @"\("),
            new TokenDefinition(TokenType.CloseGroup, @"\)"),
            new TokenDefinition(TokenType.OpenList, @"\["),
            new TokenDefinition(TokenType.CloseList, @"\]"),
            new TokenDefinition(TokenType.Period, @"\."),
            new TokenDefinition(TokenType.Comma, @","),
            new TokenDefinition(TokenType.Backquote, @"`"),
            new TokenDefinition(TokenType.Newline, @";"),
            new TokenDefinition(TokenType.Whitespace, @"\s+", true),
            new TokenDefinition(TokenType.Comment, @"'[^']*'?", true),
            ReservedWords("let", "ref", "if", "then", "else", "operator", "set!", "using", "namespace", "lambda")
        };

        private static TokenDefinition ReservedWords(params string[] reservedWords)
        {
            var regex = string.Join("|", reservedWords.Select(System.Text.RegularExpressions.Regex.Escape));
            return new TokenDefinition(TokenType.ReservedWord, regex, priority: -1);
        }

        public IEnumerable<TokenDefinition> GetTokenDefinitions() => TokenDefinitions;
    }
}