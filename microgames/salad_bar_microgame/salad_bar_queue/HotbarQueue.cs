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

        private Node2D _SpawnPoint;
        private Node2D _Customers;
        private Node2D _EndPoint;

        public override void _Ready()
        {
            _SpawnPoint = GetNode<Node2D>("%SpawnPoint");
            _Customers = GetNode<Node2D>("%Customers");
            _EndPoint = GetNode<Node2D>("%EndPoint");
        }

        public void SpawnCustomer()
        {
            CreateCustomer(PersonCustomerScene);
        }

        public void SpawnShambler()
        {
            CreateCustomer(StrangeCustomerScene);
        }

        private void CreateCustomer(PackedScene customerScene)
        {
            var customer = customerScene.Instantiate<BaseCustomer>();
            customer.GlobalPosition = _SpawnPoint.GlobalPosition;
            _Customers.AddChild(customer);
            EmitSignal(nameof(SpawnedCustomer), customer);
            customer.Start(_EndPoint.GlobalPosition);
        }
    }
}
