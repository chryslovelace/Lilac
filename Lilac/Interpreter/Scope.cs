using System;
using System.Collections.Generic;
using System.Linq;
using Lilac.Values;

namespace Lilac.Interpreter
{
    public class Scope<T> : IScope<T>
    {
        public IScope<T> NewChild()
        {
            return new Scope<T>(this);
        }
        public IScope<T> NewNamespace(List<string> names)
        {
            var ns = new Scope<T>(this);
            AddNamespace(names, ns);
            return ns;
        }

        private HashSet<Scope<T>> UsedNamespaces { get; } = new HashSet<Scope<T>>();
        private Dictionary<string, Scope<T>> Namespaces { get; set; } = new Dictionary<string, Scope<T>>();
        private Dictionary<string, Binding<T>> Bindings { get; set; } = new Dictionary<string, Binding<T>>();
        private Scope<T> ParentScope { get; set; }

        public Scope(){}

        private Scope(Scope<T> parent)
        {
            ParentScope = parent;
        }

        public string ListBindings()
        {
            return string.Join(Environment.NewLine, GetAllBindings().Select(b => b.Name));
        }

        private IEnumerable<Binding<T>> GetAllBindings(string prefix = "")
        {
            return Bindings.Values.Select(b => b.WithPrefix(prefix)).Concat(
                Namespaces.SelectMany(ns => ns.Value.GetAllBindings(ns.Key)).Select(b => b.WithPrefix(prefix)));
        }

        public bool BindingExists(string name) 
            => Bindings.ContainsKey(name) || ParentScope?.BindingExists(name) == true;

        public T GetBoundItem(string name)
        {
            return GetBinding(name).BoundItem;
        }

        private bool TryGetBinding(string name, out Binding<T> binding)
        {
            if (TryGetLocalBinding(name, out binding)) return true;
            return ParentScope?.TryGetBinding(name, out binding) == true;
        }

        private bool TryGetLocalBinding(string name, out Binding<T> binding)
        {
            if (Bindings.TryGetValue(name, out binding)) return true;
            foreach (var ns in UsedNamespaces)
            {
                if (ns.TryGetLocalBinding(name, out binding)) return true;
            }
            return false;
        }

        private bool TryGetNamespace(string name, out Scope<T> ns)
        {
            if (Namespaces.TryGetValue(name, out ns)) return true;
            foreach (var usedNs in UsedNamespaces)
            {
                if (usedNs.TryGetNamespace(name, out ns)) return true;
            }
            return ParentScope?.TryGetNamespace(name, out ns) == true;
        }
        
        public Binding<T> GetBinding(string name)
        {
            Binding<T> binding;
            if (!TryGetBinding(name, out binding))
                throw new Exception($"Binding {name} not found!");
            return binding;
        }

        IScope<T> IScope<T>.GetNamespace(string name) => GetNamespace(name);

        public Scope<T> GetNamespace(string name)
        {
            Scope<T> space;
            if (!TryGetNamespace(name, out space))
                throw new Exception($"Namespace {name} not found!");
            return space;
        }

        IScope<T> IScope<T>.GetNamespace(IList<string> namespaces) => GetNamespace(namespaces);

        public Scope<T> GetNamespace(IList<string> namespaces)
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

        public Binding<T> GetNamespacedBinding(string name, IList<string> namespaces)
        {
            if (namespaces.Count == 0)
                return GetBinding(name);
            var space = GetNamespace(namespaces[0]);
            return space.GetNamespacedBinding(name, namespaces.Skip(1).ToArray());
        }

        public void BindItem(string name, T value, bool isMutable = false, OperatorInfo opInfo = null)
        {
            if (Bindings.ContainsKey(name)) throw new Exception($"Local binding {name} already exists!");
            Bindings[name] = new Binding<T> { Name = name, BoundItem = value, IsMutable = isMutable, OperatorInfo = opInfo};
        }

        public void BindNamespacedItem(string name, T value, IList<string> namespaces, bool isMutable = false, OperatorInfo opInfo = null)
        {
            if (namespaces.Count == 0)
            {
                BindItem(name, value, isMutable, opInfo);
                return;
            }
            Scope<T> space;
            if (!Namespaces.TryGetValue(namespaces[0], out space))
            {
                Namespaces[namespaces[0]] = space = new Scope<T>();
            }
            space.BindNamespacedItem(name, value, namespaces.Skip(1).ToArray(), isMutable, opInfo);
        }

        public void SetBoundItem(string name, T value)
        {
            var binding = GetBinding(name);
            if (!binding.IsMutable) throw new Exception($"Binding {name} is not mutable!");
            binding.BoundItem = value;
        }

        public void AddNamespace(List<string> namespaces, Scope<T> ns)
        {
            if (namespaces.Count == 1)
            {
                Namespaces.Add(namespaces[0], ns);
                return;
            }
            Scope<T> space;
            if (!Namespaces.TryGetValue(namespaces[0], out space))
            {
                Namespaces[namespaces[0]] = space = new Scope<T>();
            }
            space.AddNamespace(namespaces.Skip(1).ToList(), ns);
        }
    }
}