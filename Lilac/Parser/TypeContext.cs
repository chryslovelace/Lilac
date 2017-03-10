using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lilac.Parser
{
    public class TypeContext
    {
        private ImmutableDictionary<string, Type> LocalTypes { get; set; }
        public TypeContext Parent { get; private set; }

        private TypeContext() { }

        public TypeContext Add(string id, Type type)
        {
            return new TypeContext
            {
                LocalTypes = LocalTypes.Add(id, type),
                Parent = Parent
            };
        }

        public TypeContext Set(string id, Type type)
        {
            return new TypeContext
            {
                LocalTypes = LocalTypes.SetItem(id, type),
                Parent = Parent
            };
        }

        public Type Get(string id)
        {
            Type type;
            return LocalTypes.TryGetValue(id, out type) ? type : Parent?.Get(id);
        }

        public TypeContext Child()
        {
            return new TypeContext
            {
                LocalTypes = ImmutableDictionary<string, Type>.Empty,
                Parent = this
            };
        }
        

        public TypeContext(IReadOnlyDictionary<string, Type> initialTypes)
        {
            LocalTypes = ImmutableDictionary<string, Type>.Empty.AddRange(initialTypes);
        }
    }
}