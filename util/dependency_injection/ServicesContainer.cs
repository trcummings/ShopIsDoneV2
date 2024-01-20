using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ShopIsDone.Tiles;
using Generics = System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Utils.DependencyInjection
{
	public partial class ServicesContainer : Node
    {
		[Export]
		private Array<NodePath> _ExtraServices = new Array<NodePath>();

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public void RegisterServices()
		{
            foreach (var service in GetServices())
            {
                _InjectionProvider.RegisterService(service);
            }
        }

        public void InitServices()
        {
            foreach (var service in GetServices().OfType<IInitializable>())
            {
                service.Init();
            }
        }

		public void UnregisterServices()
		{
            foreach (var service in GetServices())
            {
                _InjectionProvider.UnregisterService(service);
            }
        }

        private List<IService> GetServices()
        {
            // Collect all services from the extra services list
            var services = _ExtraServices.Select(GetNode<IService>).ToList();
            // Append direct descendent services
            services.AddRange(GetChildren().OfType<IService>());
            //
            return services;
        }
    }
}

