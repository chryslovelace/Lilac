using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Binding<T>
    {
        public string Name { get; set; }
        public bool IsMutable { get; set; }
        public T BoundItem { get; set; }


        public Binding<T> WithPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return this;
            return new Binding<T>
            {
                Name = $"{prefix}.{Name}",
                IsMutable = IsMutable,
                BoundItem = BoundItem
            };
        }
    }
}