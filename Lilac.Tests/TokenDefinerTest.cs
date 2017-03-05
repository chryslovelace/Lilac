using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Lilac.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lilac.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenDefinerTest
    {
        private TokenDefiner TokenDefiner { get; set; }

        [TestInitialize]
        public void Init()
        {
            TokenDefiner = new TokenDefiner();
        }

        [TestMethod]
        public void TestDefinitions()
        {
            var definitions = TokenDefiner.GetTokenDefinitions().ToList();

            definitions.Should().NotBeNullOrEmpty();
            definitions.Should().NotContainNulls();
            definitions.All(definition =>
                    Enum.IsDefined(typeof(TokenType), definition.TokenType) &&
                    !string.IsNullOrWhiteSpace(definition.Regex))
                .Should().BeTrue();
            definitions.Should().Contain(definition => !definition.IsIgnored);
            foreach (TokenType tokenType in Enum.GetValues(typeof(TokenType)))
            {
                if (tokenType == TokenType.Unrecognized || tokenType == TokenType.Number) continue;
                definitions.Should().Contain(definition => definition.TokenType == tokenType);
            }
        }
    }
}
