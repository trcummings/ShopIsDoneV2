using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Models;
using ShopIsDone.Core;

namespace ShopIsDone.Lighting
{
    [Tool]
    public partial class LightSwitchComponent : Node3DComponent
    {
        [Signal]
        public delegate void LightSwitchFlippedEventHandler();

        // Data
        [Export]
        private bool _IsFlippedOn;
        public bool IsFlippedOn
        {
            get { return _IsFlippedOn; }
            set
            {
                _IsFlippedOn = value;
                FlipOwnLight();
            }
        }

        [Export]
        private Array<WorldLight> _Lights = new Array<WorldLight>();

        // Paths/Nodes
        [Export]
        private NodePath _ModelPath;
        private IModel _Model;

        [Export]
        private WorldLight _SwitchLight;

        public override void _Ready()
        {
            // Base ready
            base._Ready();

            // Ready nodes
            _Model = GetNode<IModel>(_ModelPath);
        }

        public override void Init()
        {
            // Initially set
            FlipOwnLight();
        }

        public Command RunFlipLights()
        {
            return new SeriesCommand(
                // Run flip switch animation
                new AsyncCommand(() => _Model.PerformAnimation(_IsFlippedOn ? "FlipOff" : "FlipOn")),
                // Flip the lights and emit
                new ActionCommand(() =>
                {
                    // Set lights flipped on to opposite of what it was
                    _IsFlippedOn = !_IsFlippedOn;
                    foreach (var light in _Lights) light.IsLit = !light.IsLit;
                    // Emit
                    EmitSignal(nameof(LightSwitchFlipped));
                })
            );
        }

        private void FlipOwnLight()
        {
            // Set the lighting for the switch itself off if the switch is on
            if (_IsFlippedOn)
            {
                _SwitchLight?.TurnOff();
                _Model?.PerformAnimation("SetOn");
                // Turn on each light
                foreach (var light in _Lights) light.IsLit = true;
            }
            else
            {
                _SwitchLight?.TurnOn();
                _Model?.PerformAnimation("SetOff");
                // Turn off each light
                foreach (var light in _Lights) light.IsLit = false;
            }
        }
    }
}
