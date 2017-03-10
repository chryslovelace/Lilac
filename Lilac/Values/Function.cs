using System;
using System.Collections.Generic;
using Lilac.AST.Expressions;
using Lilac.Interpreter;

namespace Lilac.Values
{
    public class Function : Value
    {
        public string DeclaringName { get; }
        public Scope DeclaringScope { get; }
        public IReadOnlyList<string> Parameters { get; }
        public Expression Body { get; }

        public Function(FunctionDefinitionExpression functionDefinition, Scope declaringScope)
        {
            DeclaringName = functionDefinition.Name;
            Parameters = functionDefinition.Parameters;
            Body = functionDefinition.Body;
            DeclaringScope = declaringScope;
        }

        public Function(OperatorDefinitionExpression operatorDefinition, Scope declaringScope)
        {
            DeclaringName = operatorDefinition.Name;
            Parameters = operatorDefinition.Parameters;
            Body = operatorDefinition.Body;
            DeclaringScope = declaringScope;
        }

        public Function(LambdaExpression lambdaExpression, Scope declaringScope)
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