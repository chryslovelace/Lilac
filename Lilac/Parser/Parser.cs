using System;
using System.Collections.Generic;
using Lilac.AST;
using Lilac.AST.Definitions;
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

        public static Expression Parse<T>(this Parser<T> parser, ParserState state) where T : Expression
            => parser(state).Match(
                ok => ok.Item2,
                error => { throw error; });

        public static Expression Parse<T>(this Parser<T> parser, ref ParserState state) where T : Expression
        {
            var result = parser(state);
            var ok = result as Ok<Tuple<ParserState, T>, ParseException>;
            if (ok == null) throw ((Error<Tuple<ParserState, T>, ParseException>) result).Value;
            state = ok.Value.Item1;
            return ok.Value.Item2;
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
                var star = (Ok<Tuple<ParserState, List<T>>, ParseException>) parser.Star()(state);
                return star.Value.Map((newstate, list) => list.Count == 0)
                    ? Error<List<T>>(errorMessage ?? "Expected one or more of something, got none.", state)
                    : star;
            };

        public static Parser<List<T>> PlusSep<T, TSep>(this Parser<T> parser, Parser<TSep> sepParser, string errorMessage = null)
            => state =>
            {
                var star = (Ok<Tuple<ParserState, List<T>>, ParseException>) parser.StarSep(sepParser)(state);
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
                where predicate(t) || l is Nothing<TLeading>
                select t;

        public static Parser<T> WithOptLeading<T, TLeading>(this Parser<T> parser, Parser<TLeading> leading) =>
            from l in leading.Opt()
            from t in parser
            select t;

        public static Parser<EmptyExpression> AddDefinition(Definition definition)
            => state => Ok(state.AddDefinition(definition), new EmptyExpression());

        public static Parser<EmptyExpression> PushContext()
            => state => Ok(state.PushContext(), new EmptyExpression());

        public static Parser<Context> PopContext()
            => state => Ok(state.PopContext(), state.Context);

        public static Parser<EmptyExpression> UseNamespace(IList<string> namespaces)
            => state => Ok(state.UseNamespace(namespaces), new EmptyExpression());

        public static Parser<bool> IsDefinedNamespace(IList<string> namespaces)
            => state => Ok(state, state.IsDefinedNamespace(namespaces));

        public static Parser<EmptyExpression> AddNamespace(IList<string> namespaces, Context context)
            => state => Ok(state.AddNamespace(namespaces, context), new EmptyExpression());

        public static Parser<string[]> TempReserveWords(params string[] words)
            => state => Ok(state.TempReserveWords(words), words);

        public static Parser<string[]> UnReserveWords(params string[] words)
            => state => Ok(state.TempReserveWords(words), words);

        #endregion

        #region Terminals

        public static Parser<string> Id()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Identifier && !state.IsDefinedOperator(token.Content) && !state.IsTempReserved(token.Content)
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<string> Id(string name)
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.Identifier && token.Content == name && !state.IsDefinedOperator(token.Content)
                select Tuple.Create(state.NextToken($"Parsed {token}"), token.Content))
                .OnFailure($"Expected identifier, got {state.GetToken()}.", state);

        public static Parser<EmptyExpression> ReservedWord(string name)
            => state => (
                from token in state.GetToken()
                where (token.TokenType == TokenType.ReservedWord || state.IsTempReserved(token.Content)) && token.Content == name
                select Tuple.Create(state.NextToken($"Parsed {token}"), new EmptyExpression()))
                .OnFailure($"Expected '{name}', got {state.GetToken()}.", state);

        public static Parser<string> IdNotEquals()
            => state => (
                from token in state.GetToken()
                where
                    token.Content != "=" && token.TokenType == TokenType.Identifier &&
                    !state.IsDefinedOperator(token.Content)
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
                where token.TokenType == TokenType.Identifier && state.IsDefinedOperator(token.Content)
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
                    Tuple.Create(state.NextToken($"Parsed {token}"), new NumberLiteralExpression {Value = token.Content, LiteralType = token.TokenType}))
                .OnFailure($"Expected number, got {state.GetToken()}.", state);

        public static Parser<StringLiteralExpression> String()
            => state => (
                from token in state.GetToken()
                where token.TokenType == TokenType.String
                select
                    Tuple.Create(state.NextToken($"Parsed {token}"), new StringLiteralExpression {Value = token.Content}))
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
                .Or(NamespacedId())
                .Or(Identifier())
                .Or(Cond())
                .Or(Group())
                .Or(List())
                .Or(LinkedList())
                .Or(String())
                .Or(OperatorFunction())
                .Or(Lambda())
                .PostfixExpression();
        
        public static Parser<IdentifierExpression> Identifier() =>
            from id in Id()
            select new IdentifierExpression {Name = id};

        public static Parser<OperatorExpression> OperatorFunction() =>
            from open in GroupOpen(GroupType.Parenthesized)
            from op in Operator()
            from close in GroupClose(GroupType.Parenthesized)
            select new OperatorExpression {Name = op};

        public static Parser<FunctionCallExpression> FunctionCall(this Expression function) => 
            from argument in Expression()
            select new FunctionCallExpression {Function = function, Argument = argument};

        public static Parser<OperatorCallExpression> OperatorCall(this Expression lhs) => 
            from op in Operator()
            from rhs in Expression()
            select new OperatorCallExpression {Lhs = lhs, Name = op, Rhs = rhs};

        public static Parser<MemberAccessExpression> MemberAccess(this Expression target) => 
            from dot in Period()
            from member in Id()
            select new MemberAccessExpression {Target = target, Member = member};

        public static Parser<Expression> PostfixExpression<T>(this Parser<T> parser) where T : Expression =>
            from expr in parser
            from maybe in expr.OperatorCall()
                .Or(expr.FunctionCall())
                .Or(expr.MemberAccess())
                .IfSoContinueWith(just => just.AsParser().PostfixExpression())
            select maybe.Match(e => e.ResolvePrecedence(), () => expr);

        public static Parser<GroupExpression> Group() =>
            from groupType in GroupOpen()
            //.Or<GroupType>(
            //    from nl in Newline()
            //    from groupType in GroupOpen(GroupType.Indented)
            //    select groupType)
            from nls1 in Newline().Star()
            from pushScope in PushContext()
            from exprs in Expression().Or<Expression>(Definition()).StarSep(Newline().Plus())
            from popScope in PopContext()
            from nls2 in Newline().Star()
            from close in GroupClose(groupType)
            select new GroupExpression {Expressions = exprs, GroupType = groupType};

        public static Parser<TopLevelExpression> TopLevel() =>
            from open in GroupOpen(GroupType.TopLevel)
            from exprs in Expression().Or<Expression>(Definition()).StarSep(Newline().Plus())
            from nls in Newline().Star()
            from close in GroupClose(GroupType.TopLevel)
            select new TopLevelExpression {Expressions = exprs, GroupType = GroupType.TopLevel};

        public static Parser<BindingExpression> Let() =>
            from letId in ReservedWord("let")
            from id in Id()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression || exp is ListExpression)
            select new BindingExpression {Name = id, ValueExpression = expr};

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
            from pushScope in PushContext()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            from popScope in PopContext()
            from addFunction in AddDefinition(new Definition(id))
            select new FunctionDefinitionExpression
            {
                Name = id,
                Parameters = args,
                Body = expr
            };

        public static Parser<OperatorDefinitionExpression> OperatorDef() =>
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
                select (Association) Enum.Parse(typeof(Association), a)).Opt()
            let association = a.GetValueOrDefault()
            from id in Id()
            from args in ArgList()
            from eq in Equals()
            from pushScope in PushContext()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            from popScope in PopContext()
            from addOperator in AddDefinition(new OperatorDefinition(id, precedence, association))
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
            from target in Expression()
            let eqExpr = target as OperatorCallExpression
            where eqExpr?.Name == "="
            let lhs = eqExpr.Lhs as MemberAccessExpression
            where lhs != null
            select new MemberAssignmentExpression
            {
                Target = lhs.Target,
                Member = lhs.Member,
                ValueExpression = eqExpr.Rhs
            };

    public static Parser<NamespacedIdentifierExpression> NamespacedId() =>
            from namespaces in (from id in Id() from dot in Period() select id).Plus()
            from isDefined in IsDefinedNamespace(namespaces)
            where isDefined
            from id in Id()
            select new NamespacedIdentifierExpression { Namespaces = namespaces, Name = id };

        public static Parser<UsingExpression> Using() =>
            from usingId in ReservedWord("using")
            from namespaces in Id().PlusSep(Period())
            from use in UseNamespace(namespaces)
            select new UsingExpression {Namespaces = namespaces};

        public static Parser<NamespaceExpression> Namespace() =>
            from nsId in ReservedWord("namespace")
            from namespaces in Id().PlusSep(Period())
            from eq in Equals()
            from nl in Newline().Opt()
            from groupType in GroupOpen()
            from nls1 in Newline().Star()
            from pushScope in PushContext()
            from exprs in Expression().Or<Expression>(Definition()).StarSep(Newline().Plus())
            from popScope in PopContext()
            from addNamespace in AddNamespace(namespaces, popScope)
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
            select new ListExpression {Expressions = exprs};

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
            select new LinkedListExpression { Expressions = exprs};

        public static Parser<LambdaExpression> Lambda() =>
            from lambda in ReservedWord("lambda")
            from args in ArgList()
            from eq in Equals()
            from expr in Expression().WithOptLeadingIf(Newline(), exp => exp is GroupExpression)
            select new LambdaExpression {Parameters = args, Body = expr};



        #endregion
    }
}