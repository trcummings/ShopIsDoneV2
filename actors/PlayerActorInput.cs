using System;
using Godot;
using ShopIsDone.Cameras;

namespace ShopIsDone.Actors
{
	public partial class PlayerActorInput : Node
	{
        [Export]
        private InputXformer _InputXformer;

        public Vector3 MoveDir { get; set; }

        public void UpdateInput()
        {
            var axis = new Vector3(
                Input.GetAxis("fps_move_left", "fps_move_right"),
                0,
                Input.GetAxis("fps_move_backward", "fps_move_forward")
            );

            MoveDir = _InputXformer.GetXformedInput(axis);
        }
    }
}

