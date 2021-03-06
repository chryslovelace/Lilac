﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Exceptions;
using Lilac.Utilities;

namespace Lilac.Parser
{
    public delegate Result<Tuple<ParserState, T>, ParseException> Parser<T>(ParserState state);

    public static class Parser
    {
        #region Monad

        public static Parser<T> AsParser<T>(this T value)
            => state => new Ok<Tuple<ParserState, T>, ParseException>(Tuple.Create(state, value));

        public static Parser<T2> Bind<T1, T2>(this Parser<T1> parser, Func<T1, Parser<T2>> func)
            => state =>
                from result1 in parser(state)
                from result2 in result1.Map((newState, output) => func(output)(newState))
                select result2;

        public static Parser<T3> SelectMany<T1, T2, T3>(this Parser<T1> parser, Func<T1, Parser<T2>> func,
            Func<T1, T2, T3> select)
            => parser.Bind(val1 => func(val1).Bind(val2 => select(val1, val2).AsParser()));

        public static Parser<T2> Select<T1, T2>(this Parser<T1> parser, Func<T1, T2> func)
            => parser.Bind(val => func(val).AsParser());

        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate) => state =>
            from result in parser(state)
            where predicate(result.Item2)
            select result;

        #endregion

        #region Utility

        public static Result<Tuple<ParserState, T>, ParseException> Ok<T>(Tuple<ParserState, T> tuple)
        {
            return new Ok<Tuple<ParserState, T>, ParseException>(tuple);
        }
        public static Result<Tuple<ParserState, T>, ParseException> Ok<T>(ParserState state, T value)
        {
            return new Ok<Tuple<ParserState, T>, ParseException>(Tuple.Create(state, value));
        }

        public static Result<Tuple<ParserState, T>, ParseException> Error<T>(string message, ParserState state)
        {
            return new Error<Tuple<ParserState, T>, ParseException>(new ParseException(message, state));
        }

        public static Result<Tuple<ParserState, T>, ParseException> OnFailure<T>(this Maybe<Tuple<ParserState, T>> maybe, string message, ParserState state)
           => Result<Tuple<ParserState, T>, ParseException>.FromMaybe(maybe, new ParseException(message, state));

        public static Token CheckValid(Token token)
        {
            if (token.TokenType == TokenType.Unrecognized)
                throw new ParseException($"Cannot parse unrecognized token: {token}.");
            if (token.TokenType == TokenType.Number)
                throw new ParseException($"Cannot parse abstract number token, use a specific number type: {token}.");
            return token;
        }

        public static T Parse<T>(this Parser<T> parser, IEnumerable<Token> tokens)
        {
            
            return parser(new ParserState(tokens.Select(CheckValid))).Match(
                ok => ok.Item2,
                error => { throw error; });
        }
        
        public static Parser<Expression> AsExpression<T>(this Parser<T> parser) where T : Expression
            => state =>
                from result in parser(state)
                select result.Map((parserState, expression) => Tuple.Create(parserState, (Expression)expression));

        public static Parser<Expression> Or<T1, T2>(this Parser<T1> parser1, Parser<T2> parser2)
            where T1 : Expression where T2 : Expression
            => state => parser1.AsExpression()(state).Match(
                Ok,
                error => parser2.AsExpression()(state));

        public static Parser<T> Or<T>(this Parser<T> parser1, Parser<T> parser2)
            => state => parser1(state).Match(
                Ok,
                error => parser2(state));

        public static Parser<List<T>> Star<T>(this Parser<T> parser)
            => state =>
            {
                Ok<Tuple<ParserState, T>, ParseException> result;
                var list = new List<T>();
                while ((result = parser(state) as Ok<Tuple<ParserState, T>, ParseException>) != null)
                {
                    result.Value.Map((newState, output) =>
                    {
                        state = newState;
                        list.Add(output);
                    });
                }
                return Ok(state, list);
            };

        public static Parser<List<T>> StarSep<T, TSep>(this Parser<T> parser, Parser<TSep> sepParser)
            => state =>
            {
                Ok<Tuple<ParserState, T>, ParseException> result;
                var list = new List<T>();

                var first = parser(state) as Ok<Tuple<ParserState, T>, ParseException>;
                if (first == null)
                {
                    return Ok(state, list);
                }

                first.Value.Map((newState, output) =>
                {
                    state = newState;
                    list.Add(output);
                });

                var combined = from sep in sepParser from elem in parser select elem;

                while ((result = combined(state) as Ok<Tuple<ParserState, T>, ParseException>) != null)
                {
                    result.Value.Map((newState, output) =>
                    {
                        state = newState;
                        list.Add(output);
                    });
                }
                return Ok(state, list);
            };

        public static Parser<List<T>> Plus<T>(this Parser<T> parser, string errorMessage = null)
            => state =>
            {
                var star = (Ok<Tuple<ParserState, List<T>>, ParseException>)parser.Star()(state);
                return star.Value.Map((newstate, list) => list.Count == 0)
                    ? Error<List<T>>(errorMessage ?? "Expected one or more of something, got none.", state)
                    : star;
            };

        public static Parser<List<T>> PlusSep<T, TSep>(this Parser<T> parser, Parser<TSep> sepParser, string errorMessage = null)
            => state =>
            {
                var star = (Ok<Tuple<ParserState, List<T>>, ParseException>)parser.StarSep(sepParser)(state);
                return star.Value.Map((newstate, list) => list.Count == 0)
                    ? Error<List<T>>(errorMessage ?? "Expected separated list of one or more of something, got none.", state)
                    : star;
            };

        public static Parser<Maybe<T>> Opt<T>(this Parser<T> parser)
            => state => parser(state).Match(
                ok => ok.Map((newState, output) => Ok(newState, output.ToMaybe())),
                error => Ok(state, Maybe<T>.Nothing));

        public static Parser<Maybe<T>> IfSoContinueWith<T>(this Parser<T> parser, Func<T, Parser<T>> cont)
            => state => parser(state).Match(
                ok => ok.Map((newState, output) => cont(output).Opt()(newState)),
                error => Ok(state, Maybe<T>.Nothing));

        public static Parser<T> WithOptLeadingIf<T, TLeading>(this Parser<T> parser, Parser<TLeading> leading,
            Func<T, bool> predicate) =>
                from l in leading.Opt()
                from t in parser
                where l is Nothing<TLeading> || predicate(t)
                select t;

        public static Parser<T> WithOptLeading<T, TLeading>(this Parser<T> parser, Parser<TLeading> leading) =>
            from l in leading.Opt()
            from t in parser
            select t;

        #endregion

        #region Terminals

        public static Parser<string> Id()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Identifier
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<string> Id(string name)
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Identifier && token.Content == name
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> ReservedWord(string name)
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.ReservedWord && token.Content == name
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected '{name}', got {state.GetToken()}.", state);

        public static Parser<string> IdNotEquals()
            => state => (
                from token in state.GetToken()
                where token.Content != "=" && token.TokenType == TokenType.Identifier
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> Equals() => state => (
                 from token in state.GetToken()
                 where token.TokenType == TokenType.Identifier && token.Content == "="
                 select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                 .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<string> Operator()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Identifier
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected operator, got {state.GetToken()}.", state);

        public static Maybe<GroupType> OpenGroupType(string name)
        {
            switch (name)
            {
                case "bof":
                    return GroupType.TopLevel.ToMaybe();
                case "(":
                    return GroupType.Parenthesized.ToMaybe();
                case "indent":
                    return GroupType.Indented.ToMaybe();
                default:
                    return Maybe<GroupType>.Nothing;
            }
        }

        public static Maybe<GroupType> CloseGroupType(string name)
        {
            switch (name)
            {
                case "eof":
                    return GroupType.TopLevel.ToMaybe();
                case ")":
                    return GroupType.Parenthesized.ToMaybe();
                case "dedent":
                    return GroupType.Indented.ToMaybe();
                default:
                    return Maybe<GroupType>.Nothing;
            }
        }

        public static Parser<GroupType> GroupOpen()
            => state => (
                from token in state.GetToken()
                from type in OpenGroupType(token.Content)
                select Tuple.Create(state.NextToken($"Parsed {token}"), type))
                .OnFailure($"Expected group opener, got {state.GetToken()}.", state);

        public static Parser<GroupType> GroupOpen(GroupType type)
            => state => (
                from token in state.GetToken()
                from openType in OpenGroupType(token.Content)
                where type == openType
                select Tuple.Create(state.NextToken($"Parsed {token}"), type))
                .OnFailure($"Expected {type} group opener, got {state.GetToken()}.", state);

        public static Parser<GroupType> GroupClose(GroupType type)
            => state => (
                from token in state.GetToken()
                from closeType in CloseGroupType(token.Content)
                where type == closeType
                select Tuple.Create(state.NextToken($"Parsed {token}"), type))
                .OnFailure($"Expected {type} group closer, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> ListOpen()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.OpenList
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected list opener, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> ListClose()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.CloseList
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected list closer, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> Period()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Period
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected period, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> Backquote()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Backquote
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected period, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> Comma()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Comma
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected comma, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> Newline()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Newline
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected newline, got {state.GetToken()}.", state);

        public static Parser<Token> NonDelimiter()
            => state => (
                from token in state.GetToken()
                where
                    !token.TokenType.In(new[]
                    {
                        TokenType.Newline, TokenType.OpenGroup, TokenType.OpenList, TokenType.CloseGroup,
                        TokenType.CloseList
                    })
                select Tuple.Create(state.NextToken($"Parsed {token}"), token))
                .OnFailure($"Expected newline, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> EmptyGroup()
            => state => (
                from open in state.GetToken()
                where open.TokenType == TokenType.OpenGroup && open.Content == "("
                from close in state.NextToken().GetToken()
                where close.TokenType == TokenType.CloseGroup && close.Content == ")"
                select Tuple.Create(state.NextToken().NextToken($"Parsed {open} and {close}"), new EmptyExpression()))
                .OnFailure($"Expected (), got {state.GetToken()}.", state);

        public static Parser<NumberLiteralExpression> Number()
            => state => (
                from token in state.GetToken()
                where token.TokenType < TokenType.Number
                select
                    Tuple.Create(state.NextToken($"Parsed {token}"), new NumberLiteralExpression { Value = token.Content, LiteralType = token.TokenType }))
.OnFailure($"Expected number, got {state.GetToken()}.", state);

        public static Parser<StringLiteralExpression> String()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.String
                select
                    Tuple.Create(state.NextToken($"Parsed {token}"), new StringLiteralExpression { Value = token.Content }))
                .OnFailure($"Expected string, got {state.GetToken()}.", state);

        #endregion

        #region NonTerminals

        public static Parser<Expression> Definition() =>
            Let()
                .Or(LetRef())
                .Or(FunctionDef())
                .Or(OperatorDef())
                .Or(Using())
                .Or(Namespace())
                .Or(Assignment())
                .Or(MemberAssignment());

        public static Parser<Expression> Expression() =>
            Number()
                .Or(AugmentedId())
                .Or(Identifier())
                .Or(Cond())
                .Or(Group())
                .Or(List())
                .Or(LinkedList())
                .Or(String())
                .Or(OperatorFunction())
                .Or(Lambda())
                .PostfixExpression();

        public static Parser<ErrorExpression> Error() =>
            from tokens in NonDelimiter().Star()
            select new ErrorExpression { ErrorTokens = tokens };

        public static Parser<IdentifierExpression> Identifier() =>
            from id in Id()
            select new IdentifierExpression { Name = id };

        public static Parser<OperatorExpression> OperatorFunction() =>
            from open in GroupOpen(GroupType.Parenthesized)
            from op in Operator()
            from close in GroupClose(GroupType.Parenthesized)
            select new OperatorExpression { Name = op };

        public static GroupExpression FlattenLine(params Expression[] expressions)
        {
            var line = new GroupExpression { GroupType = GroupType.Line, Expressions = new List<Expression>() };
            foreach (var expr in expressions)
            {
                var grp = expr as GroupExpression;
                if (grp?.GroupType == GroupType.Line)
                    line.Expressions.AddRange(grp.Expressions);
                else
                {
                    line.Expressions.Add(expr);
                }
            }
            return line;
        }

        public static Parser<GroupExpression> Line(this Expression expr) =>
            from next in Expression()
            select FlattenLine(expr, next);

        public static Parser<MemberAccessExpression> MemberAccess(this Expression target) =>
            from dot in Period()
            from member in Id()
            select new MemberAccessExpression { Target = target, Member = member };

        public static Parser<Expression> PostfixExpression<T>(this Parser<T> parser) where T : Expression =>
            from expr in parser
            from maybe in expr.Line()
                .Or(expr.MemberAccess())
                .IfSoContinueWith(just => just.AsParser().PostfixExpression())
            select maybe.Match(e => e, () => expr);

        public static Parser<GroupExpression> Group() =>
            from groupType in GroupOpen()
            from nls1 in Newline().Star()
            from exprs in Lines()
            from nls2 in Newline().Star()
            from close in GroupClose(groupType)
            select new GroupExpression { Expressions = exprs, GroupType = groupType };

        private static Parser<List<Expression>> Lines()
        {
            return Expression().Or<Expression, Expression>(Definition()).Or(Error()).StarSep(Newline().Plus());
        }

        public static Parser<TopLevelExpression> TopLevel() =>
            from open in GroupOpen(GroupType.TopLevel)
            from exprs in Lines()
            from nls in Newline().Star()
            from close in GroupClose(GroupType.TopLevel)
            select new TopLevelExpression { Expressions = exprs, GroupType = GroupType.TopLevel };

        public static Parser<BindingExpression> Let() =>
            from letId in ReservedWord("let")
            from id in Id()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression || exp is ListExpression)
            select new BindingExpression { Name = id, ValueExpression = expr };

        public static Parser<MutableBindingExpression> LetRef() =>
            from letId in ReservedWord("let")
            from refId in ReservedWord("ref")
            from id in Id()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression || exp is ListExpression)
            select new MutableBindingExpression { Name = id, ValueExpression = expr };

        public static Parser<List<string>> ArgList() =>
            (from empty in EmptyGroup()
             select new List<string>())
                .Or<List<string>>(IdNotEquals().Plus("Expected argument list."));

        public static Parser<FunctionDefinitionExpression> FunctionDef() =>
            from letId in ReservedWord("let")
            from id in Id()
            from args in ArgList()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            select new FunctionDefinitionExpression
            {
                Name = id,
                Parameters = args,
                Body = expr
            };

        public static Parser<OperatorDefinitionExpression> OperatorDef() =>
            from letId in ReservedWord("let")
            from opId in ReservedWord("operator")
            from id in Id()
            from args in ArgList()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            select new OperatorDefinitionExpression
            {
                Name = id,
                Parameters = args,
                Body = expr
            };

        public static Parser<OperatorDefinitionExpression> OperatorDefWithPrecedence() =>
            from letId in ReservedWord("let")
            from opId in ReservedWord("operator")
            from p in (
                from _ in Id("precedence")
                from p in Number()
                select decimal.Parse(p.Value)).Opt()
            let precedence = p.GetValueOrDefault()
            from a in (
                from _ in Id("associates")
                from a in Id("L").Or<string>(Id("R"))
                select (Association)Enum.Parse(typeof(Association), a)).Opt()
            let association = a.GetValueOrDefault()
            from id in Id()
            from args in ArgList()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            select new OperatorDefinitionExpression
            {
                Name = id,
                Parameters = args,
                Body = expr,
                Precedence = precedence,
                Association = association
            };

        public static Parser<Expression> CondBranch() => Expression().Or(Assignment()).Or(MemberAssignment());

        public static Parser<ConditionalExpression> Cond() =>
            from ifId in ReservedWord("if")
            from condExpr in Expression()
            from nl0 in Newline().Opt()
            from thenId in ReservedWord("then")
            from nl1 in Newline().Opt()
            from thenExpr in CondBranch()
            from elseExpr in (
                from nl2 in Newline().Opt()
                from elseId in ReservedWord("else")
                from nl3 in Newline().Opt()
                from elseExpr in CondBranch()
                select elseExpr).Opt()
            select new ConditionalExpression
            {
                Condition = condExpr,
                ThenExpression = thenExpr,
                ElseExpression = elseExpr.GetValueOrDefault()
            };

        public static Parser<AssignmentExpression> Assignment() =>
            from set in ReservedWord("set!")
            from id in Id()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression || exp is ListExpression)
            select new AssignmentExpression { Name = id, ValueExpression = expr };

        public static Parser<MemberAssignmentExpression> MemberAssignment() =>
            from set in ReservedWord("set!")
            from expr in Expression()
            let line = expr as GroupExpression
            where line?.GroupType == GroupType.Line
            let memberAssign = GetMemberAssignment(line)
            where memberAssign != null
            select memberAssign;

        public static MemberAssignmentExpression GetMemberAssignment(GroupExpression line)
        {
            var memberAssign = new MemberAssignmentExpression();
            // Expression.Id = Expression.....
            if (line.Expressions.Count < 3)
                return null;
            var memAccess = line.Expressions[0] as MemberAccessExpression;
            if (memAccess != null)
            {
                memberAssign.Target = memAccess.Target;
                memberAssign.Member = memAccess.Member;
            }
            else
            {
                var augId = line.Expressions[0] as AugmentedIdentifierExpression;
                if (augId == null)
                    return null;
                memberAssign.Member = augId.Name;
                if (augId.Namespaces.Count > 1)
                {
                    memberAssign.Target = new AugmentedIdentifierExpression
                    {
                        Name = augId.Namespaces.Last(),
                        Namespaces = augId.Namespaces.Take(augId.Namespaces.Count - 1).ToList()
                    };
                }
                else
                {
                    memberAssign.Target = new IdentifierExpression { Name = augId.Namespaces[0] };
                }
            }

            var second = line.Expressions[1] as IdentifierExpression;
            if (second?.Name != "=")
                return null;
            memberAssign.ValueExpression = line.Expressions.Count == 3 ? line.Expressions[2] : new GroupExpression
            {
                GroupType = GroupType.Line,
                Expressions = line.Expressions.Skip(2).ToList()
            };
            return memberAssign;
        }

        public static Parser<AugmentedIdentifierExpression> AugmentedId() =>
                from namespaces in (from id in Id() from dot in Period() select id).Plus()
                from id in Id()
                select new AugmentedIdentifierExpression { Namespaces = namespaces, Name = id };

        public static Parser<UsingExpression> Using() =>
            from usingId in ReservedWord("using")
            from namespaces in Id().PlusSep(Period())
            select new UsingExpression { Namespaces = namespaces };

        public static Parser<NamespaceExpression> Namespace() =>
            from nsId in ReservedWord("namespace")
            from namespaces in Id().PlusSep(Period())
            from eq in Equals()
            from nl in Newline().Opt()
            from groupType in GroupOpen()
            from nls1 in Newline().Star()
            from exprs in Expression().Or<Expression>(Definition()).StarSep(Newline().Plus())
            from nls2 in Newline().Star()
            from close in GroupClose(groupType)
            select new NamespaceExpression { Namespaces = namespaces, Expressions = exprs, GroupType = groupType };

        public static Parser<ListExpression> List() =>
            from open in ListOpen()
            from exprs in (
                from nl1 in Newline()
                from grp in Group()
                where grp.GroupType == GroupType.Indented
                from nl2 in Newline()
                select grp.Expressions
                ).Or<List<Expression>>(Expression().StarSep(Newline()))
            from close in ListClose()
            select new ListExpression { Expressions = exprs };

        public static Parser<LinkedListExpression> LinkedList() =>
            from quote in Backquote()
            from open in GroupOpen(GroupType.Parenthesized)
            from exprs in (
                from nl1 in Newline()
                from grp in Group()
                where grp.GroupType == GroupType.Indented
                from nl2 in Newline()
                select grp.Expressions
                ).Or<List<Expression>>(Expression().StarSep(Newline()))
            from close in GroupClose(GroupType.Parenthesized)
            select new LinkedListExpression { Expressions = exprs };

        public static Parser<LambdaExpression> Lambda() =>
            from lambda in ReservedWord("lambda")
            from args in ArgList()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            select new LambdaExpression { Parameters = args, Body = expr };



        #endregion
    }
}