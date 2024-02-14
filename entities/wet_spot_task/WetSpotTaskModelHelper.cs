using Godot;
using ShopIsDone.Tasks.TaskModels;
using System;

namespace ShopIsDone.Entities.WetSpotTask
{
	public partial class WetSpotTaskModelHelper : TaskModelHelper
    {
        // Nodes
        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
        private Sprite3D _WetFloor;

        [Export]
        private Node3D _WetFloorSign;

        public override void Init(int current, int total, float _)
        {
            SetProg(current, total);
        }

        public override void SetProgress(int amount, int current, int total, float percent)
        {
            SetProg(current, total);
        }

        private void SetProg(int current, int total)
        {
            // If we're still at max, show frame 0
            if (current == total)
            {
                _WetFloor.Show();
                _WetFloor.Frame = 0;
                _AnimPlayer.Play("RESET");
            }
            // If we're at the end, hide both
            else if (current == 0)
            {
                _WetFloor.Hide();
                _AnimPlayer.Play("fall_over");
            }
            // Otherwise, calculate the proper frame based on the percentage
            else
            {
                _WetFloor.Show();

                var percent = 1F - (current / (float)total);
                var frame = Mathf.RoundToInt(percent * _WetFloor.Hframes);
                _WetFloor.Frame = frame;
            }
        }
    }
}
