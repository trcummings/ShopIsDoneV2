using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Microgames.SaladBar
{
    // This is a monitor that will trigger an event when all customers have left
    // in a given wave (delineated by the reset)
    public partial class CustomerMonitor : Node
    {
        [Signal]
        public delegate void AllCustomersLeftEventHandler();

        private Array<BaseCustomer> _Customers = new Array<BaseCustomer>();

        public void AddCustomer(BaseCustomer customer)
        {
            // Add customer and connect
            _Customers.Add(customer);
            customer.Left += () => OnCustomerLeft(customer);
        }

        private void OnCustomerLeft(BaseCustomer customer)
        {
            _Customers.Remove(customer);
            // If no customers remaining, emit signal
            if (_Customers.Count == 0) EmitSignal(nameof(AllCustomersLeft));
        }
    }
}
