﻿using System.Collections.Generic;
using System.Linq;
using Lilac.Utilities;

namespace Lilac.AST.Expressions
{
    public class FunctionDefinitionExpression : Expression
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public Expression Body { get; set; }

        public override string ToString()
        {
            return $"let {Name} {Parameters.PrettyPrintParameters()} = {Body.ToString()}";
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitFunctionDefinition(this);
        }
    }
}