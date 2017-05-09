using System;
using System.Collections.Generic;
using System.Linq;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Interpreter;
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

        public override Expression VisitApplication(ApplicationExpression functionCall)
        {
            var exprs = ExpandApplications(functionCall);
            var opInfo = exprs.Select(e =>
            {
                var id = e as IdentifierExpression;
                if (id != null) return CurrentScope.GetBoundItemOrDefault(id.Name);
                var aid = e as AugmentedIdentifierExpression;
                if (aid != null) return CurrentScope.GetNamespaceBoundItemOrDefault(aid.Name, aid.Namespaces);
                return null;
            });
            throw new NotImplementedException();
        }

        private List<Expression> ExpandApplications(ApplicationExpression functionCall)
        {
            var curr = functionCall;
            var exprs = new List<Expression>();
            do
            {
                exprs.Add(curr.Argument);
                var func = curr.Function as ApplicationExpression;
                if (func == null) break;
                curr = func;
            } while (true);
            exprs.Add(curr.Function);
            exprs.Reverse();
            return exprs;
        }

        public override Expression VisitFunctionDefinition(FunctionDefinitionExpression functionDefinition)
        {
            PushScope();
            var expr = base.VisitFunctionDefinition(functionDefinition);
            PopScope();
            return expr;
        }

        public override Expression VisitGroup(GroupExpression @group)
        {
            PushScope();
            var expr = base.VisitGroup(@group);
            PopScope();
            return expr;
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