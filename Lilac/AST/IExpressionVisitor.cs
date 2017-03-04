using Lilac.AST.Expressions;

namespace Lilac.AST
{
    public interface IExpressionVisitor<out T>
    {
        T VisitAssignment(AssignmentExpression assignment);
        T VisitBinding(BindingExpression binding);
        T VisitConditional(ConditionalExpression conditional);
        T VisitFunctionCall(FunctionCallExpression functionCall);
        T VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition);
        T VisitGroup(GroupExpression group);
        T VisitIdentifier(IdentifierExpression identifier);
        T VisitMutableBinding(MutableBindingExpression mutableBinding);
        T VisitNumberLiteral(NumberLiteralExpression numberLiteral);
        T VisitOperatorCall(OperatorCallExpression operatorCall);
        T VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition);
        T VisitStringLiteral(StringLiteralExpression stringLiteral);
        T VisitTopLevelExpression(TopLevelExpression topLevelExpression);
        T VisitNamespacedIdentifier(NamespacedIdentifierExpression namespacedIdentifier);
        T VisitUsing(UsingExpression usingExpression);
        T VisitOperator(OperatorExpression operatorExpression);
        T VisitMemberAccess(MemberAccessExpression memberAccess);
        T VisitNamespace(NamespaceExpression namespaceExpression);
        T VisitMemberAssignment(MemberAssignmentExpression memberAssignment);
        T VisitList(ListExpression list);
        T VisitLambda(LambdaExpression lambdaExpression);
        T VisitLinkedList(LinkedListExpression linkedListExpression);
        T VisitError(ErrorExpression errorExpression);
    }
}