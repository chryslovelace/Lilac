using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Lilac.Attributes;
using Lilac.Values;

// ReSharper disable StaticMemberInGenericType

namespace Lilac.Utilities
{
    public static class MemberContainer<T>
    {
        private static readonly ConstructorInfo BuiltInFunctionConstructor = typeof(BuiltInFunction).GetConstructor(new[] {typeof(MethodInfo), typeof(Type), typeof(object)});
        private static Dictionary<string, Func<T, Value>> Getters { get; } = new Dictionary<string, Func<T, Value>>();
        private static Dictionary<string, Action<T, Value>> Setters { get; } = new Dictionary<string, Action<T, Value>>();

        static MemberContainer()
        {
            foreach (var method in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attributes = method.GetCustomAttributes<BuiltInMethodAttribute>();
                foreach (var attribute in attributes)
                {
                    var getter = MakeMethodGetter(method, attribute.DelegateType);
                    Getters.Add(attribute.Name, getter);
                }
            }
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attributes = property.GetCustomAttributes<BuiltInMemberAttribute>();
                foreach (var attribute in attributes)
                {
                    var getter = MakeGetter(property);
                    Getters.Add(attribute.Name, getter);
                    if (attribute.GetOnly) continue;
                    var setter = MakeSetter(property);
                    Setters.Add(attribute.Name, setter);
                }
            }
        }

        private static Func<T, Value> MakeMethodGetter(MethodInfo method, Type delegateType)
        {
            var target = Expression.Parameter(typeof(T), "target");
            var get = Expression.New(BuiltInFunctionConstructor, Expression.Constant(method),
                Expression.Constant(delegateType), target);
            var lambda = Expression.Lambda<Func<T, Value>>(get, $"Get{method.Name}", new[] { target });
            return lambda.Compile();
        }
        
        private static Func<T, Value> MakeGetter(PropertyInfo property)
        {
            var target = Expression.Parameter(typeof(T), "target");
            var get = Expression.Property(target, property);
            var lambda = Expression.Lambda<Func<T, Value>>(get, $"Get{property.Name}", new [] {target});
            return lambda.Compile();
        }

        private static Action<T, Value> MakeSetter(PropertyInfo property)
        {
            var target = Expression.Parameter(typeof(T), "target");
            var value = Expression.Parameter(typeof(Value), "value");
            var set = Expression.Assign(Expression.Property(target, property), Expression.Convert(value, property.PropertyType));
            var lambda = Expression.Lambda<Action<T, Value>>(set, $"Set{property.Name}", new[] { target, value });
            return lambda.Compile();
        }

        public static Value GetMember(T target, string name)
        {
            var getter = Getters.GetValueOrDefault(name);
            return getter?.Invoke(target);
        }

        public static bool SetMember(T target, string name, Value value)
        {
            var setter = Setters.GetValueOrDefault(name);
            if (setter == null) return false;
            setter(target, value);
            return true;
        }
    }
}