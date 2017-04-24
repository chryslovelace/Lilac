using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Parser;
using Lilac.Values;
using String = Lilac.Values.String;

namespace Lilac.Interpreter
{
    public class Evaluator : IEvaluator
    {
        private Stack<IScope<Value>> Scopes { get; set; }
        public IScope<Value> CurrentScope => Scopes.Peek();
        private IScope<Value> TopScope { get; set; }

        public Evaluator(IScopeProvider<Value> scopeProvider)
        {
            TopScope = scopeProvider.GetScope();
            Scopes = new Stack<IScope<Value>>();
            Scopes.Push(TopScope.NewChild());
        }

        private void PushScope()
        {
            Scopes.Push(CurrentScope.NewChild());
        }

        private void PushNamespace(List<string> namespaces)
        {
            Scopes.Push(CurrentScope.NewNamespace(namespaces));
        }

        private void PushScope(IScope<Value> scope)
        {
            Scopes.Push(scope);
        }

        private void PopScope()
        {
            Scopes.Pop();
        }

        public Value VisitAssignment(AssignmentExpression assignment)
        {
            var value = assignment.ValueExpression.Accept(this);
            CurrentScope.SetBoundItem(assignment.Name, value);
            return Unit.Value;
        }

        public Value VisitBinding(BindingExpression binding)
        {
            var value = binding.ValueExpression.Accept(this);
            CurrentScope.BindItem(binding.Name, value);
            return Unit.Value;
        }

        public Value VisitConditional(ConditionalExpression conditional)
        {
            var condition = conditional.Condition.Accept(this);
            if (condition.AsBool())
                return conditional.ThenExpression.Accept(this);
            return conditional.ElseExpression != null ? conditional.ElseExpression.Accept(this) : Unit.Value;
        }

        public Value VisitApplication(ApplicationExpression functionCall)
        {
            var callable = functionCall.Function.Accept(this);
            var argument = functionCall.Argument.Accept(this);
            return Call(callable, argument);
        }

        public Value VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition)
        {
            var value = new Function(functionDefinition, CurrentScope);
            CurrentScope.BindItem(functionDefinition.Name, value);
            return Unit.Value;
        }

        public Value VisitGroup(GroupExpression group)
        {
            PushScope();
            var value = group.Expressions.Select(expression => expression.Accept(this)).LastOrDefault();
            PopScope();
            return value ?? Unit.Value;
        }

        public Value VisitIdentifier(IdentifierExpression identifier)
        {
            return CurrentScope.GetBoundItem(identifier.Name);
        }

        public Value VisitMutableBinding(MutableBindingExpression mutableBinding)
        {
            var value = mutableBinding.ValueExpression.Accept(this);
            CurrentScope.BindItem(mutableBinding.Name, value, true);
            return Unit.Value;
        }

        public Value VisitNumberLiteral(NumberLiteralExpression numberLiteral)
        {
            switch (numberLiteral.LiteralType)
            {
                case TokenType.DecimalNumber:
                    return Number.ParseDecimalLiteral(numberLiteral.Value);
                case TokenType.BinaryNumber:
                    return Number.ParseBinaryLiteral(numberLiteral.Value);
                case TokenType.HexNumber:
                    return Number.ParseHexLiteral(numberLiteral.Value);
                case TokenType.RationalNumber:
                    return Number.ParseRationalLiteral(numberLiteral.Value);
                case TokenType.ComplexNumber:
                    return Number.ParseComplexLiteral(numberLiteral.Value);
                default:
                    return Number.Parse(numberLiteral.Value);
            }
        }

        public Value VisitOperatorCall(OperatorCallExpression operatorCall)
        {
            var op = CurrentScope.GetBoundItem(operatorCall.Name);
            var lhs = operatorCall.Lhs.Accept(this);
            var rhs = operatorCall.Rhs.Accept(this);
            return Call(Call(op, lhs), rhs);
        }

        public Value VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            var value = new Function(operatorDefinition, CurrentScope);
            CurrentScope.BindItem(operatorDefinition.Name, value);
            return Unit.Value;
        }

        public Value VisitStringLiteral(StringLiteralExpression stringLiteral)
        {
            return String.Parse(stringLiteral.Value);
        }

        public Value VisitTopLevelExpression(TopLevelExpression topLevelExpression)
        {
            var value = topLevelExpression.Expressions.Select(expression => expression.Accept(this)).LastOrDefault();
            return value ?? Unit.Value;
        }

        public Value VisitNamespacedIdentifier(AugmentedIdentifierExpression namespacedIdentifier)
        {
            return CurrentScope.GetNamespacedBinding(namespacedIdentifier.Name, namespacedIdentifier.Namespaces).BoundItem;
        }

        public Value VisitUsing(UsingExpression usingExpression)
        {
            CurrentScope.UseNamespace(usingExpression.Namespaces);
            return Unit.Value;
        }

        public Value VisitOperator(OperatorExpression operatorExpression)
        {
            return CurrentScope.GetBoundItem(operatorExpression.Name);
        }

        public Value VisitMemberAccess(MemberAccessExpression memberAccess)
        {
            var target = memberAccess.Target.Accept(this);
            var value = target.GetMember(memberAccess.Member);
            if (value == null) throw new Exception("Member not found!");
            return value;
        }

        public Value VisitNamespace(NamespaceExpression namespaceExpression)
        {
            PushNamespace(namespaceExpression.Namespaces);
            var value = namespaceExpression.Expressions.Select(expression => expression.Accept(this)).LastOrDefault();
            PopScope();
            return value ?? Unit.Value;
        }

        public Value VisitMemberAssignment(MemberAssignmentExpression memberAssignment)
        {
            var target = memberAssignment.Target.Accept(this);
            var value = memberAssignment.ValueExpression.Accept(this);
            if (!target.SetMember(memberAssignment.Member, value))
                throw new Exception("Member not found!");
            return Unit.Value;
        }

        public Value VisitList(ListExpression list)
        {
            return new List(list.Expressions.Select(expression => expression.Accept(this)));
        }

        public Value VisitLambda(LambdaExpression lambdaExpression)
        {
            return new Function(lambdaExpression, CurrentScope);
        }

        public Value VisitLinkedList(LinkedListExpression linkedList)
        {
            return Pair.LinkedList(linkedList.Expressions.Select(e =>
            {
                var group = e as GroupExpression;
                return group != null
                    ? VisitLinkedList(new LinkedListExpression {Expressions = group.Expressions})
                    : e.Accept(this);
            }).ToList());
        }

        public Value VisitError(ErrorExpression errorExpression)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorExpression);
            Console.ForegroundColor = ConsoleColor.White;
            return Unit.Value;
        }

        private Value Call(Value callable, Value argument)
        {
            if (!callable.IsCallable())
                throw new Exception($"{callable} is not callable!");

            var function = callable as Function;
            if (function != null)
            {
                return CallFunction(function, argument);
            }
            var curried = callable as CurriedFunction;
            if (curried != null)
            {
                return CallCurried(curried, argument);
            }
            var builtIn = callable as BuiltInFunction;
            if (builtIn != null)
            {
                return CallBuiltIn(builtIn, argument);
            }

            throw new Exception($"{callable} is not callable!");
        }

        private Value CallBuiltIn(BuiltInFunction builtIn, Value argument)
        {
            return builtIn.ParameterCount > 1 ? new CurriedFunction(builtIn).Apply(argument) : ExecuteBuiltIn(builtIn, new[] { argument });
        }

        private Value CallCurried(CurriedFunction curried, Value argument)
        {
            curried = curried.Apply(argument);

            var function = curried.Callable as Function;
            if (function != null)
            {
                return curried.AppliedArguments.Count < function.Parameters.Count ? curried : ExecuteFunction(function, curried.AppliedArguments);
            }

            var builtIn = curried.Callable as BuiltInFunction;
            if (builtIn != null)
            {
                return curried.AppliedArguments.Count < builtIn.ParameterCount ? curried : ExecuteBuiltIn(builtIn, curried.AppliedArguments);
            }

            throw new Exception($"{curried} is not callable!");
        }

        private Value ExecuteFunction(Function function, IReadOnlyList<Value> arguments)
        {
            PushScope(function.DeclaringScope);
            PushScope();
            for (var i = 0; i < function.Parameters.Count; i++)
            {
                CurrentScope.BindItem(function.Parameters[i], arguments[i]);
            }
            var value = function.Body.Accept(this);
            PopScope();
            PopScope();
            return value;
        }

        private Value ExecuteBuiltIn(BuiltInFunction function, IEnumerable<Value> arguments)
        {
            try
            {
                return (Value)function.Method.DynamicInvoke(arguments.Take(function.ParameterCount).ToArray<object>()) ?? Unit.Value;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        private Value CallFunction(Function function, Value argument)
        {
            return function.Parameters.Count > 1 ? new CurriedFunction(function).Apply(argument) : ExecuteFunction(function, new[] { argument });
        }

        public void InjectBuiltInValue(string name, Value value, OperatorInfo opInfo = null)
        {
            TopScope.BindItem(name, value);
        }
    }
}