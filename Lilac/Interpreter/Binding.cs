using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Binding
    {
        public string Name { get; set; }
        public bool IsMutable { get; set; }
        public Value Value { get; set; }
        
    }
}