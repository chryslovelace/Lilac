using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Interpreter;

namespace Lilac.Parser
{
    public abstract class ExpressionTransformerBase : IExpressionVisitor<Expression>
    {
        public virtual Expression VisitAssignment(AssignmentExpression assignment)
        {
            assignment.ValueExpression = assignment.ValueExpression.Accept(this);
            return assignment;
        }

        public virtual Expression VisitBinding(BindingExpression binding)
        {
            binding.ValueExpression = binding.ValueExpression.Accept(this);
            return binding;
        }

        public virtual Expression VisitConditional(ConditionalExpression conditional)
        {
            conditional.Condition = conditional.Condition.Accept(this);
            conditional.ThenExpression = conditional.ThenExpression.Accept(this);
            conditional.ElseExpression = conditional.ElseExpression.Accept(this);
            return conditional;
        }

        public virtual Expression VisitApplication(ApplicationExpression functionCall)
        {
            functionCall.Function = functionCall.Function.Accept(this);
            functionCall.Argument = functionCall.Argument.Accept(this);
            return functionCall;
        }

        public virtual Expression VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition)
        {
            functionDefinition.Body = functionDefinition.Body.Accept(this);
            return functionDefinition;
        }

        public virtual Expression VisitGroup(GroupExpression group)
        {
            group.Expressions = group.Expressions.Select(expr => expr.Accept(this)).ToList();
            return group;
        }

        public virtual Expression VisitIdentifier(IdentifierExpression identifier)
        {
            return identifier;
        }

        public virtual Expression VisitMutableBinding(MutableBindingExpression mutableBinding)
        {
            mutableBinding.ValueExpression = mutableBinding.ValueExpression.Accept(this);
            return mutableBinding;
        }

        public virtual Expression VisitNumberLiteral(NumberLiteralExpression numberLiteral)
        {
            return numberLiteral;
        }

        public virtual Expression VisitOperatorCall(OperatorCallExpression operatorCall)
        {
            operatorCall.Lhs = operatorCall.Lhs.Accept(this);
            operatorCall.Rhs = operatorCall.Rhs.Accept(this);
            return operatorCall;
        }

        public virtual Expression VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            operatorDefinition.Body = operatorDefinition.Body.Accept(this);
            return operatorDefinition;
        }

        public virtual Expression VisitStringLiteral(StringLiteralExpression stringLiteral)
        {
            return stringLiteral;
        }

        public virtual Expression VisitTopLevelExpression(TopLevelExpression topLevelExpression)
        {
            topLevelExpression.Expressions = topLevelExpression.Expressions.Select(expr => expr.Accept(this)).ToList();
            return topLevelExpression;
        }

        public virtual Expression VisitNamespacedIdentifier(AugmentedIdentifierExpression namespacedIdentifier)
        {
            return namespacedIdentifier;
        }

        public virtual Expression VisitUsing(UsingExpression usingExpression)
        {
            return usingExpression;
        }

        public virtual Expression VisitOperator(OperatorExpression operatorExpression)
        {
            return operatorExpression;
        }

        public virtual Expression VisitMemberAccess(MemberAccessExpression memberAccess)
        {
            memberAccess.Target = memberAccess.Target.Accept(this);
            return memberAccess;
        }

        public virtual Expression VisitNamespace(NamespaceExpression namespaceExpression)
        {
            namespaceExpression.Expressions = namespaceExpression.Expressions.Select(expr => expr.Accept(this)).ToList();
            return namespaceExpression;
        }

        public virtual Expression VisitMemberAssignment(MemberAssignmentExpression memberAssignment)
        {
            memberAssignment.Target = memberAssignment.Target.Accept(this);
            memberAssignment.ValueExpression = memberAssignment.ValueExpression.Accept(this);
            return memberAssignment;
        }

        public virtual Expression VisitList(ListExpression list)
        {
            list.Expressions = list.Expressions.Select(expr => expr.Accept(this)).ToList();
            return list;
        }

        public virtual Expression VisitLambda(LambdaExpression lambdaExpression)
        {
            lambdaExpression.Body = lambdaExpression.Body.Accept(this);
            return lambdaExpression;
        }

        public virtual Expression VisitLinkedList(LinkedListExpression linkedListExpression)
        {
            linkedListExpression.Expressions = linkedListExpression.Expressions.Select(expr => expr.Accept(this)).ToList();
            return linkedListExpression;
        }

        public virtual Expression VisitError(ErrorExpression errorExpression)
        {
            return errorExpression;
        }
    }
}