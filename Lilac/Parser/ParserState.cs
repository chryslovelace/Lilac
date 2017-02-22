using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lilac.AST;
using Lilac.AST.Definitions;
using Lilac.Utilities;

namespace Lilac.Parser
{
    public class ParserState
    {
        private IBidirectionalIterator<Token> TokenStream { get; set; }
        public Context Context { get; private set; }
        private ImmutableList<string> Messages { get; set; }
        private ImmutableHashSet<string> TempReservedWords { get; set; }

        private ParserState() { }

        public ParserState(IEnumerable<Token> tokens, Context context)
        {
            TokenStream = new BidirectionalIterator<Token>(tokens);
            TokenStream.MoveNext();
            Messages = ImmutableList<string>.Empty;
            Context = context;
            TempReservedWords = ImmutableHashSet<string>.Empty;
        }

        public ParserState New(IEnumerable<Token> tokens)
        {
            return new ParserState(tokens, Context);
        }

        public bool IsDefinedOperator(string name)
        {
            return Context.GetDefinition(name) is OperatorDefinition;
        }

        public ParserState Copy()
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState AddDefinition(Definition definition)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.AddDefinition(definition),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState NextToken()
        {
            var state = new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
            state.TokenStream.MoveNext();
            return state;
        }

        public ParserState NextToken(string message)
        {
            var state = new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages.Add(message),
                TempReservedWords = TempReservedWords
            };
            state.TokenStream.MoveNext();
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

        public ParserState PushContext()
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.NewChild(),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState PopContext()
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.Parent,
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState AddNamespacedDefinition(IList<string> namespaces, Definition definition)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.AddNamespacedDefinition(namespaces, definition),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState UseNamespace(IList<string> namespaces)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.UseNamespace(namespaces),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public bool IsDefinedNamespace(IList<string> namespaces)
        {
            return Context.GetNamespace(namespaces) != null;
        }

        public ParserState AddNamespace(IList<string> namespaces, Context context)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context.AddNamespace(namespaces, context),
                Messages = Messages,
                TempReservedWords = TempReservedWords
            };
        }

        public ParserState TempReserveWords(IEnumerable<string> words)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords.Union(words)
            };
        }

        public ParserState TempReserveWords(params string[] words)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords.Union(words)
            };
        }

        public ParserState UnReserveWords(IEnumerable<string> words)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords.Except(words)
            };
        }

        public ParserState UnReserveWords(params string[] words)
        {
            return new ParserState
            {
                TokenStream = TokenStream.Copy(),
                Context = Context,
                Messages = Messages,
                TempReservedWords = TempReservedWords.Except(words)
            };
        }

        public bool IsTempReserved(string word) => TempReservedWords.Contains(word);
    }
}