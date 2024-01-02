using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IDebtDamageHandler
    {
        Command HandleDebtDamage(ActionPointHandler apHandler, LevelEntity source, int totalDebtDamage, int debtAfterDamage);
    }

    // Null implementation of this handler
    public partial class DebtDamageHandler : Node, IDebtDamageHandler
    {
        [Signal]
        public delegate void TookDebtDamageEventHandler(int amount);

        [Signal]
        public delegate void TookNoDamageEventHandler();

        public virtual Command HandleDebtDamage(ActionPointHandler apHandler, LevelEntity source, int totalDebtDamage, int debtAfterDamage)
        {
            return new Command();
        }
    }
}

