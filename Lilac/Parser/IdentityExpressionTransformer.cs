using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;

namespace Lilac.Parser
{
    public class IdentityExpressionTransformer : IExpressionVisitor<Expression>
    {
        public virtual Expression VisitAssignment(AssignmentExpression assignment) => new AssignmentExpression
        { 
            Name = assignment.Name,
            ValueExpression = assignment.ValueExpression.Accept(this)
        };

        public virtual Expression VisitBinding(BindingExpression binding) => new BindingExpression
        {
            Name = binding.Name,
            ValueExpression = binding.ValueExpression.Accept(this)
        };

        public virtual Expression VisitConditional(ConditionalExpression conditional) => new ConditionalExpression
        {
            Condition = conditional.Condition.Accept(this),
            ThenExpression = conditional.ThenExpression.Accept(this),
            ElseExpression = conditional.ElseExpression?.Accept(this)
        };

        public virtual Expression VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition) => new FunctionDefinitionExpression
        {
            Name = functionDefinition.Name,
            Parameters = functionDefinition.Parameters,
            Body = functionDefinition.Body.Accept(this)
        };

        public virtual Expression VisitGroup(GroupExpression @group) => new GroupExpression
        {
            GroupType = group.GroupType,
            Expressions = group.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public virtual Expression VisitIdentifier(IdentifierExpression identifier) => identifier;

        public virtual Expression VisitMutableBinding(MutableBindingExpression mutableBinding) => new MutableBindingExpression
        {
            Name = mutableBinding.Name,
            ValueExpression = mutableBinding.ValueExpression.Accept(this)
        };

        public virtual Expression VisitNumberLiteral(NumberLiteralExpression numberLiteral) => numberLiteral;

        public virtual Expression VisitOperatorCall(OperatorCallExpression operatorCall) => new OperatorCallExpression
        {
            Name = operatorCall.Name,
            Lhs = operatorCall.Lhs.Accept(this),
            Rhs = operatorCall.Rhs.Accept(this)
        };

        public virtual Expression VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition) => new OperatorDefinitionExpression
        {
            Association = operatorDefinition.Association,
            Body = operatorDefinition.Body.Accept(this),
            Name = operatorDefinition.Name,
            Parameters = operatorDefinition.Parameters,
            Precedence = operatorDefinition.Precedence
        };

        public virtual Expression VisitStringLiteral(StringLiteralExpression stringLiteral) => stringLiteral;

        public virtual Expression VisitTopLevelExpression(TopLevelExpression topLevelExpression) => new TopLevelExpression
        {
            GroupType = topLevelExpression.GroupType,
            Expressions = topLevelExpression.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public virtual Expression VisitNamespacedIdentifier(AugmentedIdentifierExpression namespacedIdentifier)
            => namespacedIdentifier;

        public virtual Expression VisitUsing(UsingExpression usingExpression) => usingExpression;

        public virtual Expression VisitOperator(OperatorExpression operatorExpression) => operatorExpression;

        public virtual Expression VisitMemberAccess(MemberAccessExpression memberAccess) => new MemberAccessExpression
        {
            Member = memberAccess.Member,
            Target = memberAccess.Target.Accept(this)
        };

        public virtual Expression VisitNamespace(NamespaceExpression namespaceExpression) => new NamespaceExpression
        {
            GroupType = namespaceExpression.GroupType,
            Namespaces = namespaceExpression.Namespaces,
            Expressions = namespaceExpression.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public virtual Expression VisitMemberAssignment(MemberAssignmentExpression memberAssignment) => new MemberAssignmentExpression
        {
            Member = memberAssignment.Member,
            Target = memberAssignment.Target.Accept(this),
            ValueExpression = memberAssignment.ValueExpression.Accept(this)
        };

        public virtual Expression VisitList(ListExpression list) => new ListExpression
        {
            Expressions = list.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public virtual Expression VisitLambda(LambdaExpression lambdaExpression) => new LambdaExpression
        {
            Parameters = lambdaExpression.Parameters,
            Body = lambdaExpression.Body.Accept(this)
        };

        public virtual Expression VisitLinkedList(LinkedListExpression linkedListExpression) => new LinkedListExpression
        {
            Expressions = linkedListExpression.Expressions.Select(e => e.Accept(this)).ToList()
        };

        public virtual Expression VisitError(ErrorExpression errorExpression) => errorExpression;

        public virtual Expression VisitFunctionCall(FunctionCallExpression functionCallExpression)
        {
            throw new System.NotImplementedException();
        }
    }
}