using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lilac.Utilities;

namespace Lilac.Parser
{
    public class ParserState
    {
        private IBidirectionalIterator<Token> TokenStream { get; set; }
        private ImmutableList<string> Messages { get; set; }

        private ParserState() { }

        public ParserState(IEnumerable<Token> tokens)
        {
            TokenStream = new BidirectionalIterator<Token>(tokens);
            TokenStream.MoveNext();
            Messages = ImmutableList<string>.Empty;
        }
        
        public ParserState NextToken()
        {
            var state = new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Messages = Messages,
            };
            state.TokenStream.MoveNext();
            return state;
        }

        public ParserState NextToken(string message)
        {
            var state = NextToken();
            state.Messages = state.Messages.Add(message);
            return state;
        }

        public Maybe<Token> GetToken()
        {
            try
            {
                return TokenStream.Current.ToMaybe();
            }
            catch (Exception)
            {
                return Maybe<Token>.Nothing;
            }
        }
    }
}