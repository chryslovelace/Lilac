using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Binding
    {
        public string Name { get; set; }
        public bool IsMutable { get; set; }
        public Value Value { get; set; }


        public Binding WithPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return this;
            return new Binding
            {
                Name = $"{prefix}.{Name}",
                IsMutable = IsMutable,
                Value = Value
            };
        }
    }
}