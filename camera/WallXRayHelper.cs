using Godot;
using ShopIsDone.Levels;
using System;

namespace ShopIsDone.Cameras
{
    public partial class WallXRayHelper : Node
    {
        [Export]
        private PlayerCharacterManager _CharacterManager;

        [Export]
        private Camera3D _Camera;

        public override void _Process(double delta)
        {
            if (_Camera == null || _CharacterManager.Leader == null) return;

            // Update the global shader position of the leader every frame, transformed into clip space
            RenderingServer.GlobalShaderParameterSet(
                "leader_pos",
                _CharacterManager.Leader.GlobalPosition
            );

            // Set global shader camera distance to leader
            var dist = _Camera.GlobalPosition.DistanceTo(_CharacterManager.Leader.GlobalPosition);
            RenderingServer.GlobalShaderParameterSet("camera_to_leader_distance", dist);
        }
    }
}