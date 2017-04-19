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
        private ImmutableHashSet<string> TempReservedWords { get; set; }

        private ParserState() { }

        public ParserState(IEnumerable<Token> tokens)
        {
            TokenStream = new BidirectionalIterator<Token>(tokens);
            TokenStream.MoveNext();
            Messages = ImmutableList<string>.Empty;
            TempReservedWords = ImmutableHashSet<string>.Empty;
        }

        public ParserState New(IEnumerable<Token> tokens)
        {
            return new ParserState(tokens);
        }
        

        public ParserState Copy()
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }
        
        public ParserState NextToken()
        {
            var state = new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Messages = Messages,
                TempReservedWords = TempReservedWords
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