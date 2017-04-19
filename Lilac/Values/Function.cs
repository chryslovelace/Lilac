using System.Collections.Generic;
using Lilac.AST.Expressions;
using Lilac.Interpreter;

namespace Lilac.Values
{
    public class Function : Value
    {
        public string DeclaringName { get; }
        public IScope<Value> DeclaringScope { get; }
        public IReadOnlyList<string> Parameters { get; }
        public Expression Body { get; }

        public Function(FunctionDefinitionExpression functionDefinition, IScope<Value> declaringScope)
        {
            DeclaringName = functionDefinition.Name;
            Parameters = functionDefinition.Parameters;
            Body = functionDefinition.Body;
            DeclaringScope = declaringScope;
        }

        public Function(OperatorDefinitionExpression operatorDefinition, IScope<Value> declaringScope)
        {
            DeclaringName = operatorDefinition.Name;
            Parameters = operatorDefinition.Parameters;
            Body = operatorDefinition.Body;
            DeclaringScope = declaringScope;
        }

        public Function(LambdaExpression lambdaExpression, IScope<Value> declaringScope)
        {
            DeclaringName = "anonymous function";
            Parameters = lambdaExpression.Parameters;
            Body = lambdaExpression.Body;
            DeclaringScope = declaringScope;
        }
        
        public override string ToString()
        {
            return $"<#function {DeclaringName}:{Parameters.Count}>";
        }

        public override bool IsCallable()
        {
            return true;
        }
    }
}