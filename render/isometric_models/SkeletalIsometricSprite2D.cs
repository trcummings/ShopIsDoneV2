using Godot;
using System;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Models.IsometricModels
{
    [Tool]
    public partial class SkeletalIsometricSprite2D : IsometricSprite2D
    {
        // Nodes
        [Export]
        private Node2D _FrontView;

        [Export]
        private Node2D _BackView;

        [Export]
        private Skeleton2D _Skeleton;

        public override void _Ready()
        {
            base._Ready();

            // Connect to animation started
            _AnimPlayer.Connect("animation_started", new Callable(this, nameof(OnAnimationStarted)));
            AnimationPaused += OnAnimationPaused;

            // Editor functionality only
            if (!Engine.IsEditorHint()) return;
            _AnimPlayer.Play("RESET");
        }

        public override void _Process(double delta)
        {
            // Editor functionality only
            if (!Engine.IsEditorHint()) return;

            // Stop the directional animation players if the animation player is not playing
            this.RecurseChildrenOfType<AnimationPlayer>((_, child) =>
            {
                // Do not do this to our own animation player
                if (child == _AnimPlayer) return;

                if (!_AnimPlayer.IsPlaying())
                {
                    child.Seek(_AnimPlayer.CurrentAnimationPosition);
                }
            });
        }

        public override void SetDirection(string direction)
        {
            // Set directional sprite child direction
            this.RecurseChildrenOfType<DirectionalSprite>((_, child) =>
            {
                child.Direction = direction;
                if (direction.Contains("right")) child.Scale = new Vector2(-1, 1);
                else if (direction.Contains("left")) child.Scale = new Vector2(1, 1);
            });

            // Get front or back (NB: back means towards camera, forward means away)
            if (direction.Contains("back"))
            {
                _FrontView.Show();
                _BackView.Hide();
            }
            else if (direction.Contains("forward"))
            {
                _FrontView.Hide();
                _BackView.Show();
            }
            else throw new ArgumentException($"given direction {direction} is not in standard format!");

            if (direction.Contains("right"))
            {
                _FrontView.Scale = new Vector2(-1, 1);
                _BackView.Scale = new Vector2(-1, 1);
                _Skeleton.Scale = new Vector2(-1, 1);

                // Show / Hide all directional nodes
                this.RecurseChildrenOfType<Node2D>((_, child) =>
                {
                    if (child.Name == "Left") child.Hide();
                    if (child.Name == "Right") child.Show();
                });
            }
            else if (direction.Contains("left"))
            {
                _FrontView.Scale = new Vector2(1, 1);
                _BackView.Scale = new Vector2(1, 1);
                _Skeleton.Scale = new Vector2(1, 1);

                // Show / Hide all directional nodes
                this.RecurseChildrenOfType<Node2D>((_, child) =>
                {
                    if (child.Name == "Left") child.Show();
                    if (child.Name == "Right") child.Hide();
                });
            }
            else throw new ArgumentException($"given direction {direction} is not in standard format!");
        }

        private void OnAnimationStarted(string animName)
        {
            // Directional sprite animations
            this.RecurseChildrenOfType<AnimationPlayer>((_, child) =>
            {
                // Ignore our own animation player
                if (child == _AnimPlayer) return;

                if (child.HasAnimation(animName)) child.Play(animName);
                else if (child.HasAnimation("RESET")) child.Play("RESET");
            });
        }

        private void OnAnimationPaused()
        {
            this.RecurseChildrenOfType<AnimationPlayer>((_, child) =>
            {
                // Ignore our own animation player
                if (child == _AnimPlayer) return;
                // Stop the child from playing
                if (child.IsPlaying()) child.Stop(false);
            });
        }
    }
}

