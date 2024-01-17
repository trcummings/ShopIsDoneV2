using Godot;
using System;
using ShopIsDone.Interactables.Interactions;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.BreakRoom.ArenaInteractions
{
    [Tool]
    public partial class TimeClockInteraction : Interaction
    {
        [Export]
        private NodePath _LevelSelectPath;
        private LevelSelect _LevelSelect;

        private Callable _OnLevelChange;
        private Callable _OnCancel;

        public override void _Ready()
        {
            base._Ready();
            // Throws an insanely annoying error trying to get this in the editor
            if (!Engine.IsEditorHint()) _LevelSelect = GetNode<LevelSelect>(_LevelSelectPath);
            _OnLevelChange = new Callable(this, nameof(OnLevelChangeRequested));
            _OnCancel = Callable.From(Finish);
        }

        public override void Execute()
        {
            // Connect to level select events
            _LevelSelect.Connect(nameof(_LevelSelect.LevelChangeRequested), _OnLevelChange);
            _LevelSelect.Connect(nameof(_LevelSelect.Canceled), _OnCancel);

            // Show level select
            _LevelSelect.Activate();
        }

        private void OnLevelChangeRequested(PackedScene level)
        {
            // TODO: Change level here

            Finish();
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
    }
}

