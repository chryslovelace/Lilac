using System;
using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Exceptions;
using Lilac.Interpreter;
using Lilac.Utilities;
using Lilac.Values;

namespace Lilac.Parser
{
    public class TypeDeducer : IExpressionVisitor<Type>, IExpressionConsumer<Type>
    {
        private TypeContext TypeContext { get; set; }

        public TypeDeducer(IScopeDefiner scopeDefiner)
        {
            var scope = scopeDefiner.GetScope();

            TypeContext = new TypeContext(scope.GetAllBindings().ToDictionary(b => b.Name, b => b.Value.GetValueType()));
        }

        public Type VisitAssignment(AssignmentExpression assignment)
        {
            var bodyType = assignment.ValueExpression.Accept(this);
            var bindingType = TypeContext.Get(assignment.Name);
            if (bindingType != bodyType)
                TypeContext = TypeContext.Set(assignment.Name, typeof(Value));
            assignment.ExpressionType = typeof(Unit);
            return typeof(Unit);
        }

        public Type VisitBinding(BindingExpression binding)
        {
            var bodyType = binding.ValueExpression.Accept(this);
            TypeContext = TypeContext.Add(binding.Name, bodyType);
            binding.ExpressionType = typeof(Unit);
            return typeof(Unit);
        }

        public Type VisitConditional(ConditionalExpression conditional)
        {
            var thenType = conditional.ThenExpression.Accept(this);
            var elseType = conditional.ElseExpression?.Accept(this);
            conditional.ExpressionType = elseType == null || thenType != elseType ? typeof(Value) : thenType;
            return conditional.ExpressionType;
        }

        public Type VisitFunctionCall(FunctionCallExpression functionCall)
        {
            var functionType = functionCall.Function.Accept(this);
            if (!typeof(Delegate).IsAssignableFrom(functionType))
                throw new TypeResolutionException($"Expression {functionCall.Function} is of type {functionType} and is not callable!");
            var typeArguments = functionType.GenericTypeArguments;
            var argumentType = functionCall.Argument.Accept(this);
            var firstParamType = typeArguments[0];

            if (typeArguments.Length == 1)
            {
                functionCall.ExpressionType = firstParamType;
            }
            else
            {
                if (!firstParamType.IsAssignableFrom(argumentType))
                    throw new TypeResolutionException($"Function expected parameter of type {firstParamType} but got {argumentType}!");
                functionCall.ExpressionType = typeArguments.Length == 2 ? typeArguments[1] : functionType.GetCurriedType();
            }
            return functionCall.ExpressionType;
        }

        public Type VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition)
        {
            throw new NotImplementedException();
        }

        public Type VisitGroup(GroupExpression @group)
        {
            var types = group.Expressions.Select(expr => expr.Accept(this));
            group.ExpressionType = types.LastOrDefault() ?? typeof(Unit);
            return group.ExpressionType;
        }

        public Type VisitIdentifier(IdentifierExpression identifier)
        {
            identifier.ExpressionType = TypeContext.Get(identifier.Name);
            return identifier.ExpressionType;
        }

        public Type VisitMutableBinding(MutableBindingExpression mutableBinding)
        {
            var bodyType = mutableBinding.ValueExpression.Accept(this);
            TypeContext = TypeContext.Add(mutableBinding.Name, bodyType);
            mutableBinding.ExpressionType = typeof(Unit);
            return typeof(Unit);
        }

        public Type VisitNumberLiteral(NumberLiteralExpression numberLiteral)
        {
            numberLiteral.ExpressionType = typeof(Number);
            return typeof(Number);
        }

        public Type VisitOperatorCall(OperatorCallExpression operatorCall)
        {
            throw new NotImplementedException();
        }

        public Type VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            throw new NotImplementedException();
        }

        public Type VisitStringLiteral(StringLiteralExpression stringLiteral)
        {
            stringLiteral.ExpressionType = typeof(Values.String);
            return typeof(Values.String);
        }

        public Type VisitTopLevelExpression(TopLevelExpression topLevelExpression)
        {
            var types = topLevelExpression.Expressions.Select(expr => expr.Accept(this));
            topLevelExpression.ExpressionType = types.LastOrDefault() ?? typeof(Unit);
            return topLevelExpression.ExpressionType;
        }

        public Type VisitNamespacedIdentifier(NamespacedIdentifierExpression namespacedIdentifier)
        {
            namespacedIdentifier.ExpressionType =
                TypeContext.Get(string.Join(".",
                    namespacedIdentifier.Namespaces.Concat(new[] {namespacedIdentifier.Name})));

            return namespacedIdentifier.ExpressionType;
        }

        public Type VisitUsing(UsingExpression usingExpression)
        {
            throw new NotImplementedException();
        }

        public Type VisitOperator(OperatorExpression operatorExpression)
        {
            throw new NotImplementedException();
        }

        public Type VisitMemberAccess(MemberAccessExpression memberAccess)
        {
            throw new NotImplementedException();
        }

        public Type VisitNamespace(NamespaceExpression namespaceExpression)
        {
            throw new NotImplementedException();
        }

        public Type VisitMemberAssignment(MemberAssignmentExpression memberAssignment)
        {
            throw new NotImplementedException();
        }

        public Type VisitList(ListExpression list)
        {
            throw new NotImplementedException();
        }

        public Type VisitLambda(LambdaExpression lambdaExpression)
        {
            throw new NotImplementedException();
        }

        public Type VisitLinkedList(LinkedListExpression linkedListExpression)
        {
            throw new NotImplementedException();
        }

        public Type VisitError(ErrorExpression errorExpression)
        {
            throw new NotImplementedException();
        }

        public Type Consume(Expression expression)
        {
            return expression.Accept(this);
        }
    }
}