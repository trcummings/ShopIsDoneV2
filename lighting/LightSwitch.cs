using System;
using Godot;
using System.Linq;
using ShopIsDone.Core;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Models;

namespace ShopIsDone.Lighting
{
    public partial class LightSwitch : Node
    {
        [Signal]
        public delegate void LightSwitchFlippedEventHandler();

        // Data
        [Export]
        public bool _IsFlippedOn;

        [Export]
        private Array<string> _Lights = new Array<string>();

        // Paths/Nodes
        [Export]
        private NodePath _ModelPath;
        private IModel _Model;

        [Export]
        private WorldLight _SwitchLight;

        [Export]
        private NodePath LightPath;

        // Other
        private EntityManager _EntityManager;

        public override void _Ready()
        {
            // Base ready
            base._Ready();

            // Ready nodes
            _Model = GetNode<IModel>(_ModelPath);
        }

        public void FlipLights()
        {
            // Play flip switch animation

            //// Turn off all the lights
            //return new SeriesCommand(
            //    new FlipSwitchCommand()
            //    {
            //        LightSwitch = this,
            //        Render = _LightSwitchRender
            //    },
            //    // Flip the lights
            //    new SeriesCommand(
            //        _Lights
            //            .Select(_EntityManager.GetEntity<LevelEntity>)
            //            .Where(l => l != null)
            //            .Select(l => l.GetComponent<LightComponent>())
            //            .Select(l => l.IsLightOn ? l.TurnLightOff() : l.TurnLightOn())
            //            .ToArray()
            //    ),
            //    // Emit signal
            //    new ActionCommand(() =>
            //    {
            //        EmitSignal(nameof(LightSwitchFlipped));
            //    })
            //);
        }

        private void FlipOwnLight()
        {
            // Set the lighting for the switch itself off if the switch is on
            if (_IsFlippedOn)
            {
                _SwitchLight.TurnOff();
                _Model.PerformAnimation("SetOn");
            }
            else
            {
                _SwitchLight.TurnOn();
                _Model.PerformAnimation("SetOff");
            }
        }
    }
}
