using Godot;
using ShopIsDone.Actors;
using System;

namespace ShopIsDone.Arenas
{
    public partial class EnterArenaArea : Area3D
    {
        [Signal]
        public delegate void EnteredArenaEventHandler(Arena arena);

        [Export]
        private Arena _Arena;

        public override void _Ready()
        {
           BodyEntered += OnBodyEntered;
        }

        public void Disable()
        {
            Monitoring = false;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is Actor)
            {
                CallDeferred(nameof(Disable));
                EmitSignal(nameof(EnteredArena), _Arena);
            }
        }
    }
}