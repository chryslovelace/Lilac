using System;
using FluentAssertions;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Exceptions;
using Lilac.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lilac.Tests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestId()
        {
            Parser.Parser.Id()
                .Parse(new[] {new Token {Content = "identifier", TokenType = TokenType.Identifier}})
                .Should().Be("identifier");

            Parser.Parser.Id()
                .Invoking(p => p.Parse(new[] {new Token {TokenType = TokenType.ReservedWord}}))
                .ShouldThrow<ParseException>();

            Parser.Parser.Id("hello")
                .Parse(new[] {new Token {Content = "hello", TokenType = TokenType.Identifier}})
                .Should().Be("hello");

            Parser.Parser.Id("hello")
                .Invoking(p => p.Parse(new[] {new Token {Content = "goodbye", TokenType = TokenType.Identifier}}))
                .ShouldThrow<ParseException>();

            Parser.Parser.Id("hello")
                .Invoking(p => p.Parse(new[] {new Token {Content = "hello", TokenType = TokenType.ReservedWord}}))
                .ShouldThrow<ParseException>();
        }

        [TestMethod]
        public void TestReservedWord()
        {
            Parser.Parser.ReservedWord("word")
                .Parse(new[] {new Token {Content = "word", TokenType = TokenType.ReservedWord}})
                .Should().BeOfType<EmptyExpression>();

            Parser.Parser.ReservedWord("word")
                .Invoking(p => p.Parse(new[] {new Token {Content = "otherword", TokenType = TokenType.ReservedWord}}))
                .ShouldThrow<ParseException>();

            Parser.Parser.ReservedWord("word")
                .Invoking(p => p.Parse(new[] {new Token {Content = "word", TokenType = TokenType.Identifier}}))
                .ShouldThrow<ParseException>();
        }

        [TestMethod]
        public void TestIdNotEquals()
        {
            Parser.Parser.IdNotEquals()
                .Parse(new[] { new Token { Content = "identifier", TokenType = TokenType.Identifier } })
                .Should().Be("identifier");

            Parser.Parser.IdNotEquals()
                .Invoking(p => p.Parse(new[] { new Token { Content = "=", TokenType = TokenType.Identifier } }))
                .ShouldThrow<ParseException>();

            Parser.Parser.IdNotEquals()
                .Invoking(p => p.Parse(new[] { new Token { TokenType = TokenType.ReservedWord } }))
                .ShouldThrow<ParseException>();
        }

        [TestMethod]
        public void TestEquals()
        {
            Parser.Parser.Equals()
                .Parse(new[] {new Token {Content = "=", TokenType = TokenType.Identifier}})
                .Should().BeOfType<EmptyExpression>();

            Parser.Parser.Equals()
                .Invoking(p => p.Parse(new[] { new Token { Content = "identifier", TokenType = TokenType.Identifier } }))
                .ShouldThrow<ParseException>();

            Parser.Parser.Equals()
                .Invoking(p => p.Parse(new[] { new Token { TokenType = TokenType.ReservedWord } }))
                .ShouldThrow<ParseException>();
        }

        [TestMethod]
        public void TestGroupOpen()
        {
            Parser.Parser.GroupOpen()
                .Parse(new[] {new Token {Content = "indent", TokenType = TokenType.OpenGroup}})
                .Should().Be(GroupType.Indented);

            Parser.Parser.GroupOpen()
                .Parse(new[] {new Token {Content = "(", TokenType = TokenType.OpenGroup}})
                .Should().Be(GroupType.Parenthesized);

            Parser.Parser.GroupOpen()
                .Parse(new[] {new Token {Content = "bof", TokenType = TokenType.OpenGroup}})
                .Should().Be(GroupType.TopLevel);

            Parser.Parser.GroupOpen()
                .Invoking(p => p.Parse(new[] {new Token {Content = "", TokenType = TokenType.OpenGroup}}))
                .ShouldThrow<ParseException>();

        }
    }
}