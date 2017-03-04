using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Lilac.Exceptions;
using Lilac.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lilac.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class LexerTest
    {
        private Lexer Lexer { get; set; }
        
        [TestInitialize]
        public void Init()
        {
            var tokenDefiner = new Mock<ITokenDefiner>();
            tokenDefiner.Setup(t => t.GetTokenDefinitions()).Returns(new []
            {
                new TokenDefinition(TokenType.Identifier, @"[a-zA-Z]+"),
                new TokenDefinition(TokenType.Number, @"\d+"),  
                new TokenDefinition(TokenType.Whitespace, @"\s+", true),
            });

            Lexer = new Lexer(tokenDefiner.Object);
        }

        [TestMethod]
        public void TestTokenize()
        {
            var input = new StringReader("\thello 123 \n\n    321 goodbye \nmy \n\tlove ");
            var tokens = Lexer.Tokenize(input);
            tokens.Should().BeEquivalentTo(
                new Token {TokenType = TokenType.OpenGroup, Content = "bof", Line = 0, Column = 0},
                new Token {TokenType = TokenType.OpenGroup, Content = "indent", Line = 1, Column = 0},
                new Token {TokenType = TokenType.Identifier, Content = "hello", Line = 1, Column = 4},
                new Token {TokenType = TokenType.Number, Content = "123", Line = 1, Column = 10},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 1, Column = 13},
                new Token {TokenType = TokenType.Number, Content = "321", Line = 3, Column = 4},
                new Token {TokenType = TokenType.Identifier, Content = "goodbye", Line = 3, Column = 8},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 3, Column = 15},
                new Token {TokenType = TokenType.CloseGroup, Content = "dedent", Line = 4, Column = 0},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 4, Column = 0},
                new Token {TokenType = TokenType.Identifier, Content = "my", Line = 4, Column = 0},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 4, Column = 2},
                new Token {TokenType = TokenType.OpenGroup, Content = "indent", Line = 5, Column = 0},
                new Token {TokenType = TokenType.Identifier, Content = "love", Line = 5, Column = 4},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 5, Column = 8},
                new Token {TokenType = TokenType.CloseGroup, Content = "dedent", Line = 5, Column = 0},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 5, Column = 0},
                new Token {TokenType = TokenType.CloseGroup, Content = "eof", Line = 5, Column = 0}
            );

            var failingInput = new StringReader("!");
            Lexer.Tokenize(failingInput).Enumerating().ShouldThrow<SyntaxException>();
        }
    }
}
