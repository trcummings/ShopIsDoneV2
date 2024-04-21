using System;
using System.Linq;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Microgames.PoseMannequin
{

    // Pick 2 groups
    // Create list of potentially correct poses, and incorrect poses
    // Create running list of used poses
    // Pick next correct pose (removing it from the pool)
	public partial class PoseManager : Node
	{
        private Array<string> _PoseGroups = new Array<string>()
        {
            "Flexing",
            "Kissing",
            "Desperation",
            "Vogueing",
        };
        private Array<string> _CorrectPoses = new Array<string>();
        private Array<string> _IncorrectPoses = new Array<string>();

        public string CurrentPose;
        public Array<string> AvailablePoses
        {
            get { return _CorrectPoses.Concat(_IncorrectPoses).ToGodotArray(); }
        }

        public override void _Ready()
        {
            base._Ready();
        }

        public void Init()
        {
            // Choose two pose groups and create their animations
            _PoseGroups.Shuffle();
            var groups = _PoseGroups.Take(2);
            _CorrectPoses = CreatePoses(groups.First());
            _IncorrectPoses = CreatePoses(groups.Last());

            // Pick first pose
            CurrentPose = _CorrectPoses.PickRandom();
        }

        public void NarrowSet()
        {
            var rand1 = _CorrectPoses.PickRandom();
            _CorrectPoses.Remove(rand1);
            var rand2 = _IncorrectPoses.PickRandom();
            _IncorrectPoses.Remove(rand2);
        }

        // Decide which poses are available for this upcoming round, and which
        // the correct pose will be
        public void PickNextPoseSet()
        {
            //_CorrectPoses.Remove(CurrentPose);
            CurrentPose = _CorrectPoses.PickRandom();
        }

        private Array<string> CreatePoses(string group)
        {
            return Enumerable.Range(1, 4).Select(i => $"{group}-Pose-{i}").ToGodotArray();
        }
    }
}

