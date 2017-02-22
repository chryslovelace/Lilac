using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Scope
    {
        public HashSet<Scope> UsedNamespaces { get; } = new HashSet<Scope>();
        public Dictionary<string, Scope> Namespaces { get; set; } = new Dictionary<string, Scope>();
        public Dictionary<string, Binding> Bindings { get; set; } = new Dictionary<string, Binding>();
        public Scope ParentScope { get; set; }

        public Scope(){}

        public Scope(Scope parent)
        {
            ParentScope = parent;
        }

        public string ListBindings(string prefix = "")
        {
            var sb = new StringBuilder();

            foreach (var binding in Bindings)
            {
                sb.Append(prefix).Append(binding.Key).AppendLine();
            }

            foreach (var ns in Namespaces)
            {
                sb.Append(ns.Value.ListBindings($"{prefix}{ns.Key}."));
            }

            return sb.ToString();
        }

        public bool BindingExists(string name) 
            => Bindings.ContainsKey(name) || ParentScope?.BindingExists(name) == true;

        public Value GetValue(string name)
        {
            return GetBinding(name).Value;
        }

        private bool TryGetBinding(string name, out Binding binding)
        {
            if (TryGetLocalBinding(name, out binding)) return true;
            return ParentScope?.TryGetBinding(name, out binding) == true;
        }

        private bool TryGetLocalBinding(string name, out Binding binding)
        {
            if (Bindings.TryGetValue(name, out binding)) return true;
            foreach (var ns in UsedNamespaces)
            {
                if (ns.TryGetLocalBinding(name, out binding)) return true;
            }
            return false;
        }

        private bool TryGetNamespace(string name, out Scope ns)
        {
            if (Namespaces.TryGetValue(name, out ns)) return true;
            foreach (var usedNs in UsedNamespaces)
            {
                if (usedNs.TryGetNamespace(name, out ns)) return true;
            }
            return ParentScope?.TryGetNamespace(name, out ns) == true;
        }
        
        public Binding GetBinding(string name)
        {
            Binding binding;
            if (!TryGetBinding(name, out binding))
                throw new Exception($"Binding {name} not found!");
            return binding;
        }

        public Scope GetNamespace(string name)
        {
            Scope space;
            if (!TryGetNamespace(name, out space))
                throw new Exception($"Namespace {name} not found!");
            return space;
        }

        public Scope GetNamespace(IList<string> namespaces)
        {
            if (namespaces.Count == 0)
                return this;
            var space = GetNamespace(namespaces[0]);
            return space.GetNamespace(namespaces.Skip(1).ToList());
        }

        public void UseNamespace(IList<string> namespaces)
        {
            UsedNamespaces.Add(GetNamespace(namespaces));
        }

        public Binding GetNamespacedBinding(string name, IList<string> namespaces)
        {
            if (namespaces.Count == 0)
                return GetBinding(name);
            var space = GetNamespace(namespaces[0]);
            return space.GetNamespacedBinding(name, namespaces.Skip(1).ToArray());
        }

        public void BindValue(string name, Value value, bool isMutable = false)
        {
            if (Bindings.ContainsKey(name)) throw new Exception($"Local binding {name} already exists!");
            Bindings[name] = new Binding {Name = name, Value = value, IsMutable = isMutable};
        }

        public void BindNamespacedValue(string name, Value value, IList<string> namespaces, bool isMutable = false)
        {
            if (namespaces.Count == 0)
            {
                BindValue(name, value, isMutable);
                return;
            }
            Scope space;
            if (!Namespaces.TryGetValue(namespaces[0], out space))
            {
                Namespaces[namespaces[0]] = space = new Scope();
            }
            space.BindNamespacedValue(name, value, namespaces.Skip(1).ToArray(), isMutable);
        }

        public void SetValue(string name, Value value)
        {
            var binding = GetBinding(name);
            if (!binding.IsMutable) throw new Exception($"Binding {name} is not mutable!");
            binding.Value = value;
        }

        public void AddNamespace(List<string> namespaces, Scope ns)
        {
            if (namespaces.Count == 1)
            {
                Namespaces.Add(namespaces[0], ns);
                return;
            }
            Scope space;
            if (!Namespaces.TryGetValue(namespaces[0], out space))
            {
                Namespaces[namespaces[0]] = space = new Scope();
            }
            space.AddNamespace(namespaces.Skip(1).ToList(), ns);
        }
    }
}