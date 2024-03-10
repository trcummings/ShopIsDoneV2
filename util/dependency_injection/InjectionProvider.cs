using System;
using Godot;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ShopIsDone.Utils.DependencyInjection
{
	// This is a singleton that keeps a dictionary of services to provide to
	// classes that use them
	public partial class InjectionProvider : Node
	{
        private Dictionary<Type, object> _Services = new Dictionary<Type, object>();

        // Static function to help get the singleton
        public static InjectionProvider GetProvider(Node node)
        {
            return node.GetNode<InjectionProvider>("/root/InjectionProvider");
        }

        public bool IsServiceRegistered(Type serviceType)
        {
            return _Services.ContainsKey(serviceType);
        }

        public static void Inject(Node node)
        {
            GetProvider(node).InjectObject(node);
        }

        public void RegisterService(IService service)
        {
            var type = service.GetType();
            _Services.Add(type, service);
        }

        public void UnregisterService(IService service)
        {
            var type = service.GetType();
            _Services.Remove(type);
        }

        private object Resolve(Type serviceType)
        {
            if (_Services.TryGetValue(serviceType, out var service))
            {
                return service;
            }
            throw new InvalidOperationException($"No service registered for type {serviceType}");
        }

        public void InjectObject(GodotObject obj)
        {
            var fields = GetFields(obj.GetType()).Where(prop => Attribute.IsDefined(prop, typeof(InjectAttribute)));

            // Resolve each property
            foreach (var field in fields)
            {
                var serviceType = field.FieldType;
                var service = Resolve(serviceType);
                field.SetValue(obj, service);
            }
        }

        private IEnumerable<FieldInfo> GetFields(Type type)
        {
            // Loop over public and protected members
            foreach (var item in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                yield return item;
            }

            // Get first base type
            type = type.BaseType;

            // Find their "private" members
            while (
                type != null &&
                type != typeof(Node) &&
                type != typeof(Resource) &&
                type != typeof(GodotObject) &&
                type != typeof(object) &&
                type != typeof(Variant)
            )
            {
                // Loop over non-public members
                foreach (var item in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    // Make sure it's private!
                    // To prevent doubling up on protected members
                    if (item.IsPrivate)
                    {
                        yield return item;
                    }
                }

                // Get next base type.
                type = type.BaseType;
            }
        }
    }
}

