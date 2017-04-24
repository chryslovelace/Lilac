using System.Collections.Generic;
using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Interpreter;
using Lilac.Values;

namespace Lilac.Parser
{
    public class PrecedenceResolver : IExpressionVisitor<Expression>
    {
        private Stack<IScope<OperatorInfo>> Scopes { get; set; }
        public IScope<OperatorInfo> CurrentScope => Scopes.Peek();
        private IScope<OperatorInfo> TopScope { get; set; }

        public PrecedenceResolver(IScopeProvider<OperatorInfo> scopeProvider)
        {
            TopScope = scopeProvider.GetScope();
            Scopes = new Stack<IScope<OperatorInfo>>();
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

        private void PushScope(IScope<OperatorInfo> scope)
        {
            Scopes.Push(scope);
        }

        private void PopScope()
        {
            Scopes.Pop();
        }

        public Expression VisitAssignment(AssignmentExpression assignment) => new AssignmentExpression
        {
            Name = assignment.Name,
            ValueExpression = assignment.ValueExpression.Accept(this)
        };

        public Expression VisitBinding(BindingExpression binding) => new BindingExpression
        {
            Name = binding.Name,
            ValueExpression = binding.ValueExpression.Accept(this)
        };

        public Expression VisitConditional(ConditionalExpression conditional) => new ConditionalExpression
        {
            Condition = conditional.Condition.Accept(this),
            ThenExpression = conditional.ThenExpression.Accept(this),
            ElseExpression = conditional.ElseExpression?.Accept(this)
        };

        public Expression VisitApplication(ApplicationExpression functionCall)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition) => new FunctionDefinitionExpression
        {
            Name = functionDefinition.Name,
            Body = functionDefinition.Body.Accept(this),
            Parameters = functionDefinition.Parameters
        };

        public Expression VisitGroup(GroupExpression @group) => new GroupExpression
        {
            GroupType = @group.GroupType,
            Expressions = @group.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public Expression VisitIdentifier(IdentifierExpression identifier) => identifier;

        public Expression VisitMutableBinding(MutableBindingExpression mutableBinding) => new MutableBindingExpression
        {
            Name = mutableBinding.Name,
            ValueExpression = mutableBinding.ValueExpression.Accept(this)
        };

        public Expression VisitNumberLiteral(NumberLiteralExpression numberLiteral)
        {
            return numberLiteral;
        }

        public Expression VisitOperatorCall(OperatorCallExpression operatorCall)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitStringLiteral(StringLiteralExpression stringLiteral)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitTopLevelExpression(TopLevelExpression topLevelExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitNamespacedIdentifier(AugmentedIdentifierExpression namespacedIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitUsing(UsingExpression usingExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitOperator(OperatorExpression operatorExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitMemberAccess(MemberAccessExpression memberAccess)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitNamespace(NamespaceExpression namespaceExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitMemberAssignment(MemberAssignmentExpression memberAssignment)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitList(ListExpression list)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitLambda(LambdaExpression lambdaExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitLinkedList(LinkedListExpression linkedListExpression)
        {
            throw new System.NotImplementedException();
        }

        public Expression VisitError(ErrorExpression errorExpression)
        {
            throw new System.NotImplementedException();
        }
    }
}