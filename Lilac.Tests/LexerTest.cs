using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
            var input = new StringReader("\thello 123 321 goodbye");
            var tokens = Lexer.Tokenize(input).ToList();
            tokens.Should().BeEquivalentTo(
                new Token {TokenType = TokenType.OpenGroup, Content = "bof", Line = 0, Column = 0},
                new Token {TokenType = TokenType.OpenGroup, Content = "indent", Line = 1, Column = 0},
                new Token {TokenType = TokenType.Identifier, Content = "hello", Line = 1, Column = 4},
                new Token {TokenType = TokenType.Number, Content = "123", Line = 1, Column = 10},
                new Token {TokenType = TokenType.Number, Content = "321", Line = 1, Column = 14},
                new Token {TokenType = TokenType.Identifier, Content = "goodbye", Line = 1, Column = 18},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 1, Column = 0},
                new Token {TokenType = TokenType.CloseGroup, Content = "dedent", Line = 1, Column = 0},
                new Token {TokenType = TokenType.Newline, Content = "", Line = 1, Column = 0},
                new Token {TokenType = TokenType.CloseGroup, Content = "eof", Line = 1, Column = 0});

            var failingInput = new StringReader("!");
            Lexer.Tokenize(failingInput).Enumerating().ShouldThrow<SyntaxException>();
        }
    }
}
