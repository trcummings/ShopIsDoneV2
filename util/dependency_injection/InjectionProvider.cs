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

        public static void Inject(Node node)
        {
            GetProvider(node).InjectNode(node);
        }

        public void RegisterService<TService>(TService service)
            where TService : IService
        {
            _Services.Add(typeof(TService), service);
        }

        public void UnregisterService<TService>(TService _)
            where TService : IService
        {
            _Services.Remove(typeof(TService));
        }

        private object Resolve(Type serviceType)
        {
            if (_Services.TryGetValue(serviceType, out var service))
            {
                return service;
            }
            throw new InvalidOperationException($"No service registered for type {serviceType}");
        }

        private void InjectNode(Node node)
        {
            // Get all fields marked with the inject attribute
            var fields = node
                .GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(prop => Attribute.IsDefined(prop, typeof(InjectAttribute)));

            // Resolve each property
            foreach (var field in fields)
            {
                var serviceType = field.FieldType;
                var service = Resolve(serviceType);
                field.SetValue(node, service);
            }
        }
    }
}

