using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IDebtDamageHandler
    {
        Command HandleDebtDamage(ApDamagePayload payload);
    }

    // Null implementation of this handler
    public partial class DebtDamageHandler : Node, IDebtDamageHandler
    {
        [Signal]
        public delegate void TookDebtDamageEventHandler(int amount);

        [Signal]
        public delegate void TookNoDamageEventHandler();

        public virtual Command HandleDebtDamage(ApDamagePayload payload)
        {
            return new Command();
        }
    }
}

