using System;
using System.Collections.Generic;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Interpreter;
using Lilac.Values;

namespace Lilac.Parser
{
    public class ApplicationPrecedenceResolver : ExpressionTransformerBase
    {
        private Stack<IScope<Value>> Scopes { get; set; }
        private IScope<Value> CurrentScope => Scopes.Peek();
        private IScope<Value> TopScope { get; set; }

        public ApplicationPrecedenceResolver(IScopeProvider<Value> scopeProvider)
        {
            TopScope = scopeProvider.GetScope();
            Scopes = new Stack<IScope<Value>>();
            Scopes.Push(TopScope.NewChild());
        }
        
        public override Expression VisitApplication(ApplicationExpression functionCall)
        {
            throw new NotImplementedException();
        }

        public override Expression VisitOperatorDefinition(OperatorDefinitionExpression operatorDefinition)
        {
            throw new NotImplementedException();
        }
    }
}