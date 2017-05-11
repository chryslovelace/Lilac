using System.Collections.Generic;
using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Exceptions;
using Lilac.Interpreter;
using Lilac.Utilities;
using Lilac.Values;

namespace Lilac.Parser
{
    public class PrecedenceResolver : IdentityExpressionTransformer
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

        public override Expression VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition)
        {
            PushScope();
            var expr = base.VisitFunctionDefinition(functionDefinition);
            PopScope();
            return expr;
        }

        public override Expression VisitGroup(GroupExpression group)
        {
            if (group.GroupType == GroupType.Line) return VisitLine(group);
            PushScope();
            var expr = base.VisitGroup(group);
            PopScope();
            return expr;
        }

        private Expression VisitLine(GroupExpression group)
        {
            if (!group.Expressions.Any())
                return group;
            if (group.Expressions.Count <= 3)
                return MakeOperatorCall(group.Expressions);

            var newLine = new LinkedList<Expression>(group.Expressions);
            LinkedListNode<Expression> current;
            while ((current = newLine.Find(e => GetOpInfo(e) != null)) != null)
            {
                if (current == newLine.First || current == newLine.Last)
                    throw new ParseException($"Operator must be in infix position, or be curried with one or no arguments. {newLine}");
                var opInfo = GetOpInfo(current.Value);
                
            }

            return newLine.Count == 1
                ? newLine.First.Value
                : new FunctionCallExpression
                {
                    Function = newLine.First.Value,
                    Arguments = newLine.Skip(1).ToList()
                };
        }

        private Expression MakeOperatorCall(List<Expression> expressions)
        {
            var call = new OperatorCallExpression();

            var ops = expressions.Select((e, i) => new {op = GetOpInfo(e), i}).Where(x => x.op != null);


            throw new System.NotImplementedException();
        }

        private static void CheckCount(LinkedList<Expression> newLine)
        {
            if (newLine.Count > 2)
                throw new ParseException($"Operator must be in infix position, or be curried with one or no arguments. {newLine}");
        }

        private OperatorInfo GetOpInfo(Expression expression)
        {
            var id = expression as IdentifierExpression;
            if (id != null) return CurrentScope.GetBoundItemOrDefault(id.Name);
            var aid = expression as AugmentedIdentifierExpression;
            if (aid != null) return CurrentScope.GetNamespaceBoundItemOrDefault(aid.Name, aid.Namespaces);
            return null;
        }

        public override Expression VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            CurrentScope.BindItem(operatorDefinition.Name, new OperatorInfo(operatorDefinition.Precedence, operatorDefinition.Association));
            PushScope();
            var expr = base.VisitOperatorDefinition(operatorDefinition);
            PopScope();
            return expr;
        }
    }
}