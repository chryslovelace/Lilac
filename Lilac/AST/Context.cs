using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lilac.AST.Definitions;
using Lilac.Exceptions;

namespace Lilac.AST
{
    public class Context
    {
        public ImmutableDictionary<string, Definition> Definitions { get; private set; }
        public ImmutableDictionary<string, Context> Namespaces { get; private set; }
        public ImmutableHashSet<Context> UsedNamespaces { get; private set; }
        public Context Parent { get; private set; }

        private Context()
        {
            Definitions = ImmutableDictionary<string, Definition>.Empty;
            Namespaces = ImmutableDictionary<string, Context>.Empty;
            UsedNamespaces = ImmutableHashSet<Context>.Empty;
        }

        public Context(IEnumerable<Definition> definitions)
        {
            Definitions = definitions.ToImmutableDictionary(d => d.Name);
            Namespaces = ImmutableDictionary<string, Context>.Empty;
            UsedNamespaces = ImmutableHashSet<Context>.Empty;
        }

        public Context NewChild()
        {
            return new Context
            {
                Definitions = ImmutableDictionary<string, Definition>.Empty,
                Namespaces = Namespaces,
                Parent = this,
                UsedNamespaces = ImmutableHashSet<Context>.Empty
            };
        }

        public Context AddDefinition(Definition definition)
        {
            return new Context
            {
                Definitions = Definitions.Add(definition.Name, definition),
                Namespaces = Namespaces,
                Parent = Parent,
                UsedNamespaces = UsedNamespaces
            };
        }

        public Context AddNamespace(IList<string> namespaces, Context context)
        {
            var ns = namespaces[0];
            if (namespaces.Count == 1)
            {
                return new Context
                {
                    Definitions = Definitions,
                    Namespaces = Namespaces.Add(ns, context),
                    UsedNamespaces = UsedNamespaces,
                    Parent = Parent
                };
            }
            Context nextContext;
            if (Namespaces.TryGetValue(ns, out nextContext))
                return new Context
                {
                    Definitions = Definitions,
                    Namespaces = Namespaces.SetItem(ns, nextContext.AddNamespace(namespaces.Skip(1).ToList(), context)),
                    UsedNamespaces = UsedNamespaces,
                    Parent = Parent
                };
            else
                return new Context
                {
                    Definitions = Definitions,
                    Namespaces = Namespaces.Add(ns, new Context().AddNamespace(namespaces.Skip(1).ToList(), context)),
                    UsedNamespaces = UsedNamespaces,
                    Parent = Parent
                };
        }

        public Context AddNamespacedDefinition(IList<string> namespaces, Definition definition)
        {
            if (namespaces.Count == 0) return AddDefinition(definition);
            Context context;
            var ns = namespaces[0];
            if (Namespaces.TryGetValue(ns, out context))
                return new Context
                {
                    Definitions = Definitions,
                    Namespaces = Namespaces.SetItem(ns, context.AddNamespacedDefinition(namespaces.Skip(1).ToList(), definition)),
                    UsedNamespaces = UsedNamespaces,
                    Parent = Parent
                };
            else
                return new Context
                {
                    Definitions = Definitions,
                    Namespaces = Namespaces.Add(ns, new Context().AddNamespacedDefinition(namespaces.Skip(1).ToList(), definition)),
                    UsedNamespaces = UsedNamespaces,
                    Parent = Parent
                };
        }

        public Definition GetDefinition(string name)
        {
            Definition definition;
            return Definitions.TryGetValue(name, out definition)
                ? definition
                : (UsedNamespaces.Any(ns => ns.Definitions.TryGetValue(name, out definition))
                    ? definition
                    : Parent?.GetDefinition(name));
        }
        

        public Definition GetNamespacedDefinition(IList<string> namespaces, string name)
        {
            if (namespaces.Count == 0) return GetDefinition(name);
            Context context;
            var ns = namespaces[0];
            return Namespaces.TryGetValue(ns, out context)
                ? context.GetNamespacedDefinition(namespaces.Skip(1).ToList(), name)
                : Parent?.GetNamespacedDefinition(namespaces, name);
        }
        

        public Context GetNamespace(IList<string> namespaces)
        {
            if (namespaces.Count == 0) return this;
            Context context;
            var ns = namespaces[0];
            if (Namespaces.TryGetValue(ns, out context)) return context.GetNamespace(namespaces.Skip(1).ToList());
            foreach (var used in UsedNamespaces)
            {
                context = used.GetNamespace(namespaces);
                if (context != null) return context;
            }
            return Parent?.GetNamespace(namespaces);
        }

        public Context UseNamespace(IList<string> namespaces)
        {
            var ns = GetNamespace(namespaces);
            if (ns == null) throw new ParseException($"Could not find namespace '{string.Join(".", namespaces)}'!");
            return new Context
            {
                Definitions = Definitions,
                Namespaces = Namespaces,
                Parent = Parent,
                UsedNamespaces = UsedNamespaces.Add(ns)
            };
        }
    }
}