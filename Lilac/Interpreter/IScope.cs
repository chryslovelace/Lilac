using System.Collections.Generic;

namespace Lilac.Interpreter
{
    public interface IScope<T>
    {
        bool BindingExists(string name);
        T GetBoundItem(string name);
        Binding<T> GetBinding(string name);
        IScope<T> GetNamespace(string name);
        IScope<T> GetNamespace(IList<string> namespaces);
        void UseNamespace(IList<string> namespaces);
        Binding<T> GetNamespacedBinding(string name, IList<string> namespaces);
        void BindItem(string name, T value, bool isMutable = false);
        void BindNamespacedItem(string name, T value, IList<string> namespaces, bool isMutable = false);
        void SetBoundItem(string name, T value);
        IScope<T> NewChild();
        IScope<T> NewNamespace(List<string> names);
    }
}