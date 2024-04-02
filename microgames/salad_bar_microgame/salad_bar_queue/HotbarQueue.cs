using Godot;
using System;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class HotbarQueue : Node2D
    {
        [Signal]
        public delegate void SpawnedCustomerEventHandler(BaseCustomer customer);

        [Signal]
        public delegate void CustomerLeftEventHandler();

        [Export]
        public PackedScene PersonCustomerScene;

        [Export]
        public PackedScene StrangeCustomerScene;

        private Marker2D _SpawnPoint;
        private Node2D _Customers;
        private Marker2D _EndPoint;

        public override void _Ready()
        {
            _SpawnPoint = GetNode<Marker2D>("%SpawnPoint");
            _Customers = GetNode<Node2D>("%Customers");
            _EndPoint = GetNode<Marker2D>("%EndPoint");
        }

        public void SpawnCustomer()
        {
            CreateCustomer<BaseCustomer>(PersonCustomerScene);
        }

        public void SpawnShambler()
        {
            CreateCustomer<Shambler>(StrangeCustomerScene);
        }

        private void CreateCustomer<T>(PackedScene customerScene)
            where T : BaseCustomer
        {
            var customer = customerScene.Instantiate<T>();
            _Customers.AddChild(customer);
            customer.GlobalPosition = _SpawnPoint.GlobalPosition;

            EmitSignal(nameof(SpawnedCustomer), customer);
            customer.Start(_EndPoint.GlobalPosition);
        }
    }
}
