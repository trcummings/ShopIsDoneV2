using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IDebtDamageHandler
    {
        Command HandleDebtDamage(ActionPointHandler apHandler, bool receivedDirectDamage, int totalDebtDamage, int debtAfterDamage);
    }

    public partial class DebtDamageHandler : Node, IDebtDamageHandler
    {
        [Signal]
        public delegate void TookDebtDamageEventHandler(int amount);

        public virtual Command HandleDebtDamage(ActionPointHandler apHandler, bool receivedDirectDamage, int totalDebtDamage, int debtAfterDamage)
        {
            return new Command();
        }
    }
}

