﻿using System;
using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Utils
{
	public partial class DirectionalInputHelper : Node, IService
    {
        [Export]
        public IsometricCamera _IsometricCamera;

        public Vector3 InputDir = Vector3.Zero;

        public override void _Process(double delta)
        {
            // Cursor movement input
            int horizontal = 0;
            int vertical = 0;
            if (Input.IsActionJustPressed("move_up") || Input.IsActionJustPressed("fps_move_forward"))
            {
                vertical += 1;
            }
            if (Input.IsActionJustPressed("move_down") || Input.IsActionJustPressed("fps_move_backward"))
            {
                vertical -= 1;
            }
            if (Input.IsActionJustPressed("move_left") || Input.IsActionJustPressed("fps_move_left"))
            {
                horizontal -= 1;
            }
            if (Input.IsActionJustPressed("move_right") || Input.IsActionJustPressed("fps_move_right"))
            {
                horizontal += 1;
            }

            InputDir = _IsometricCamera.GetBasisXformedDir(horizontal, vertical).Round();
        }
    }
}

