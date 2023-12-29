using System;
using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Actions.Effort;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using PlayerTurnConsts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    // Base class for the choosing action sub state machine
	public partial class ActionState : State
    {
        [Signal]
        public delegate void GoBackRequestedEventHandler();

        [Signal]
        public delegate void MainMenuRequestedEventHandler();

        [Signal]
        public delegate void RunActionRequestedEventHandler(Dictionary<string, Variant> message);

        [Signal]
        public delegate void PawnAPDiffRequestedEventHandler(ActionPointHandler apData);

        [Signal]
        public delegate void PawnAPDiffCanceledEventHandler();

        [Inject]
        protected EffortMeterService _EffortService;

        [Inject]
        private TileManager _TileManager;

        protected ArenaAction _Action;
        protected LevelEntity _SelectedUnit;
        protected Tile _CurrentTile;
        private ActionPointHandler _ActionPointHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Inject services
            InjectionProvider.Inject(this);

            // Set variables
            _SelectedUnit = (LevelEntity)message[PlayerTurnConsts.SELECTED_UNIT_KEY];
            _CurrentTile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);
            _ActionPointHandler = _SelectedUnit.GetComponent<ActionPointHandler>();
            // Connect to effort service
            _EffortService.UpdatedTotalCost += OnTotalCostChange;

            if (message.ContainsKey(Consts.ACTION_KEY))
            {
                // Pull action from message
                _Action = (ArenaAction)message.GetValueOrDefault(Consts.ACTION_KEY);
                // If action can use effort, initialize the effort service
                if (_Action.UsesEffort) _EffortService.Init(_Action, _ActionPointHandler.ActionPoints);
                // Otherwise, launch an AP diff
                else RequestApDiff(new ActionPointHandler() { ActionPoints = _Action.ActionCost });
            }

            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            // Disconnect and clean up
            _EffortService.UpdatedTotalCost -= OnTotalCostChange;
            _EffortService.CleanUp();

            // Clear the ap diff
            CancelApDiff();

            base.OnExit(nextState);
        }

        private void OnTotalCostChange(int newTotal, int _)
        {
            RequestApDiff(new ActionPointHandler()
            {
                ActionPoints = newTotal
            });
        }

        protected void RequestApDiff(ActionPointHandler subtractableAp)
        {
            EmitSignal(nameof(PawnAPDiffRequested), new ActionPointHandler()
            {
                ActionPoints = Mathf.Max(_ActionPointHandler.ActionPoints - subtractableAp.ActionPoints, 0),
                ActionPointDebt = Mathf.Max(_ActionPointHandler.ActionPointDebt - subtractableAp.ActionPointDebt, 0),
                ActionPointExcess = Mathf.Max(_ActionPointHandler.ActionPointExcess - subtractableAp.ActionPointExcess, 0)
            });
        }

        protected void CancelApDiff()
        {
            EmitSignal(nameof(PawnAPDiffCanceled));
        }
    }
}

