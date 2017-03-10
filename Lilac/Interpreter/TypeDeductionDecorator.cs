using System;
using Lilac.AST;
using Lilac.AST.Expressions;
using Lilac.Utilities;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class TypeDeductionDecorator : IExpressionConsumer<Value>
    {
        private IExpressionConsumer<Value> Evaluator { get; }
        private IExpressionConsumer<Type> TypeDeducer { get; }
        

        public TypeDeductionDecorator(IExpressionConsumer<Value> evaluator, IExpressionConsumer<Type> typeDeducer)
        {
            Evaluator = evaluator;
            TypeDeducer = typeDeducer;
        }

        public Value Consume(Expression expression)
        {
            try
            {
                var topType = TypeDeducer.Consume(expression);
                Console.Write($"{topType.LilacTypeName()} : ");
            }
            catch (Exception)
            {
                Console.Write("type not deduced : ");
            }

            return Evaluator.Consume(expression);
        }
    }
}