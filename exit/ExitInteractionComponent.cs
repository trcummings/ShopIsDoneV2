using System;
using Godot;
using ShopIsDone.ArenaInteractions;
using ShopIsDone.Arenas;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Core;
using ShopIsDone.Cameras;
using ShopIsDone.Conditions;
using ShopIsDone.Models;
using ShopIsDone.EntityStates;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Exit
{
	public partial class ExitInteractionComponent : InteractionComponent, IUpdateOnActionComponent
    {
        [Signal]
        public delegate void UnitOpenedDoorEventHandler();

        [Signal]
        public delegate void UnitClosedDoorEventHandler();

        [Signal]
        public delegate void UnitRattledDoorEventHandler();

        [Inject]
        private UnitDeathService _DeathService;

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private ConditionsService _ConditionsService;

        [Inject]
        private ArenaOutcomeService _OutcomeService;

        [Export]
        private ModelComponent _Model;

        public bool IsLocked = true;

        public override void Init()
        {
            base.Init();

            // Inject
            InjectionProvider.Inject(this);

            // Lock if conditions not completed
            if (_ConditionsService.AllConditionsComplete())
            {
                IsLocked = false;
                _ = _Model.PerformActionAsync("unlock_silent");
            }
            else
            {
                IsLocked = true;
                _ = _Model.PerformActionAsync("lock_silent");
            }
        }

        // Unlock and lock based on conditions service
        public Command UpdateOnAction()
        {
            return new SeriesCommand(
                new ConditionalCommand(
                    () => _ConditionsService.AllConditionsComplete() && IsLocked,
                    _CameraService.PanToTemporaryCameraTarget(Entity,
                        new SeriesCommand(
                            new ActionCommand(() => IsLocked = false),
                            _Model.RunPerformAction("unlock")
                        )
                    )
                ),
                new ConditionalCommand(
                    () => !_ConditionsService.AllConditionsComplete() && !IsLocked,
                    _CameraService.PanToTemporaryCameraTarget(Entity,
                        new SeriesCommand(
                            new ActionCommand(() => IsLocked = true),
                            _Model.RunPerformAction("lock")
                        )
                    )
                )
            );
        }

        public override Command RunInteraction(
            UnitInteractionHandler handler,
            Dictionary<string, Variant> message = null
        )
        {
            // Get unit from message
            var unit = handler.Entity;
            var stateHandler = handler.GetComponent<EntityStateHandler>();

            return new IfElseCommand(
                () => IsLocked,
                new ActionCommand(() => EmitSignal(nameof(UnitOpenedDoor))),
                // If it's open, let the unit move through
                new SeriesCommand(
                    new ActionCommand(() =>
                    {
                        // Emit the exit noise signal
                        EmitSignal(nameof(UnitOpenedDoor));

                        // Mark the pawn as exited and hide it
                        _OutcomeService.ExitUnit(unit);
                        _DeathService.SafelyRemoveUnit(unit);
                    }),
                    // Set the pawn's state to "escaped"
                    stateHandler.RunChangeState(StateConsts.Employees.EXITED),
                    // Wait a tick then make noise
                    new WaitForCommand(Entity, 0.35f),
                    new ActionCommand(() => EmitSignal(nameof(UnitClosedDoor))),
                    // Fire off the base finished hook
                    base.RunInteraction(handler, message)
                )
            );
        }
    }
}

