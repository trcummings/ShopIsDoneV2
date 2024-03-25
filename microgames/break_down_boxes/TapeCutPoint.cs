using Godot;
using System;

namespace ShopIsDone.Microgames.BreakDownBoxes
{
    public partial class TapeCutPoint : Area2D
    {
        [Signal]
        public delegate void PointCutEventHandler();

        public bool WasCut = false;
        private AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            base._Ready();
            _AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            AreaEntered += OnPointCut;
        }

        private void OnPointCut(Node2D _)
        {
            WasCut = true;
            _AnimPlayer.Play("default");
            EmitSignal(nameof(PointCut));
            SetDeferred("monitoring", false);
            AreaEntered -= OnPointCut;
        }
    }
}

