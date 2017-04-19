using System;
using Lilac.AST;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public interface IEvaluator : IExpressionVisitor<Value>
    {
        void InjectBuiltInValue(string name, Value value, OperatorInfo opInfo = null);
    }
}