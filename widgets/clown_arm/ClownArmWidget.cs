using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Widgets
{
    public partial class ClownArmWidget : Node3D, IService
    {
        [Signal]
        public delegate void HandDescendEventHandler();

        [Signal]
        public delegate void HandClenchedEventHandler();

        [Export]
        public NodePath GrabPointPath;

        // Nodes
        private Node3D _GrabPoint;
        private AnimationPlayer _WidgetAnimPlayer;

        // State

        public override void _Ready()
        {
            // Ready nodes
            _GrabPoint = GetNode<Node3D>(GrabPointPath);
            _WidgetAnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

            Hide();
        }

        public async Task DescendUponPawn(LevelEntity entity)
        {
            // Play the idle animation
            _WidgetAnimPlayer.Play("Idle");

            // Position the clown arm at the proper tile
            GlobalPosition = entity.GlobalPosition;

            // Rotate the clown arm to be the same direction as the pawn, so it
            // appears behind it
            var target = GlobalPosition - entity.FacingDirection;
            LookAt(target, Vector3.Up);

            // Play an ominous synth sweep when the hand appears
            EmitSignal(nameof(HandDescend));

            // Descend and stretch hand out
            _WidgetAnimPlayer.Play("Descend");

            // Show the clown arm
            // NB: Idle frames to avoid a content flash
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            Show();

            // Wait for the animation to finish
            await ToSignal(_WidgetAnimPlayer, "animation_finished");
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

            // Play the grabbing animation
            _WidgetAnimPlayer.Play("GrabPawn");
            await ToSignal(_WidgetAnimPlayer, "animation_finished");

            // Clench hand
            EmitSignal(nameof(HandClenched));
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

            // Parent the pawn to the grab point
            var prevPawnParent = entity.GetParent();
            prevPawnParent.RemoveChild(entity);
            _GrabPoint.AddChild(entity);

            // Fix its position to the grab point
            entity.Position = Vector3.Zero;

            // Ascend
            _WidgetAnimPlayer.Play("Ascend");
            await ToSignal(_WidgetAnimPlayer, "animation_finished");

            // Remove the child and set its parent back to normal
            _GrabPoint.RemoveChild(entity);
            prevPawnParent.AddChild(entity);

            // Hide
            Hide();
        }
    }
}