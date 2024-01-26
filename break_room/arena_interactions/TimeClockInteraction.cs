using Godot;
using System;
using ShopIsDone.Interactables.Interactions;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Core.Data;

namespace ShopIsDone.BreakRoom.ArenaInteractions
{
    [Tool]
    public partial class TimeClockInteraction : Interaction
    {
        [Export]
        private NodePath _LevelSelectPath;
        private LevelSelect _LevelSelect;

        [Export]
        private Interaction _OnCancelInteraction;

        private Callable _OnLevelChange;
        private Callable _OnCancel;

        public override void _Ready()
        {
            base._Ready();

            // Ignore if in editor
            if (Engine.IsEditorHint()) return;

            _LevelSelect = GetNode<LevelSelect>(_LevelSelectPath);
            _OnLevelChange = new Callable(this, nameof(OnLevelChangeRequested));
            _OnCancel = Callable.From(OnCancel);
        }

        public override void Execute()
        {
            // Connect to level select events
            _LevelSelect.Connect(nameof(_LevelSelect.LevelChangeRequested), _OnLevelChange);
            _LevelSelect.Connect(nameof(_LevelSelect.Canceled), _OnCancel);

            // Show level select
            _LevelSelect.Activate();
        }

        private void OnLevelChangeRequested(string levelId)
        {
            CleanUp();

            // Emit level change event
            var events = Events.GetEvents(this);
            events.RequestLevelChange(levelId);

            // Finish
            Finish();
        }

        private void OnCancel()
        {
            CleanUp();

            // Run on cancel interaction
            _OnCancelInteraction.Connect(
                nameof(_OnCancelInteraction.Finished),
                Callable.From(Finish),
                (uint)ConnectFlags.OneShot
            );
            _OnCancelInteraction.Execute();
        }

        private void CleanUp()
        {
            // Clean up connections and deactivate level select
            _LevelSelect.SafeDisconnect(nameof(_LevelSelect.LevelChangeRequested), _OnLevelChange);
            _LevelSelect.SafeDisconnect(nameof(_LevelSelect.Canceled), _OnCancel);
            _LevelSelect.Deactivate();
        }

        protected override void Finish()
        {
            // Clean up connections and deactivate level select
            _LevelSelect.SafeDisconnect(nameof(_LevelSelect.LevelChangeRequested), _OnLevelChange);
            _LevelSelect.SafeDisconnect(nameof(_LevelSelect.Canceled), _OnCancel);
            _LevelSelect.Deactivate();

            // Base finish
            base.Finish();
        }

        public override string[] _GetConfigurationWarnings()
        {
            if (_OnCancelInteraction == null)
            {
                return new string[] { "Needs On Cancel Interaction!" };
            }
            return base._GetConfigurationWarnings();
        }
    }
}

