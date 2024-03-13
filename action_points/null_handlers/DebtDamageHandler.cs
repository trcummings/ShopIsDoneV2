using System;
using Godot;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.ActionPoints
{
    public interface IDebtDamageHandler
    {
        void Init(ActionPointHandler handler);

        Command HandleDebtDamage(ApDamagePayload payload);
    }

    // Null implementation of this handler
    public partial class DebtDamageHandler : Node, IDebtDamageHandler
    {
        [Signal]
        public delegate void TookDebtDamageEventHandler(int amount);

        [Signal]
        public delegate void TookNoDamageEventHandler();

        public void Init(ActionPointHandler handler)
        {
            // Inject self
            InjectionProvider.Inject(this);

            // Propagate debt damage signal
            Connect(nameof(TookDebtDamage), Callable.From((int amt) =>
            {
                handler.EmitSignal(nameof(handler.TookDebtDamage), amt);
            }));
        }

        public virtual Command HandleDebtDamage(ApDamagePayload payload)
        {
            return new Command();
        }
    }
}

