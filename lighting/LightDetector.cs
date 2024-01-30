using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Lighting
{
    public partial class LightDetector : Node3D
    {
        [Signal]
        public delegate void LightEnteredEventHandler();

        [Signal]
        public delegate void LightExitedEventHandler();

        // Tiles that detect light
        protected List<Area3D> _LightDetectorAreas = new List<Area3D>();

        private bool _WasLit = false;

        public override void _Ready()
        {
            // Ready nodes
            _LightDetectorAreas = GetChildren().OfType<Area3D>().ToList();
        }

        public override void _Process(double delta)
        {
            var currentIsLit = IsLit();

            if (currentIsLit && !_WasLit)
            {
                EmitSignal(nameof(LightEntered));
            }
            else if (!currentIsLit && _WasLit)
            {
                EmitSignal(nameof(LightExited));
            }

            _WasLit = currentIsLit;
        }

        public bool IsLit()
        {
            // Query the light detector for any overlaps
            return _LightDetectorAreas.Any(IsAreaInLight);
        }

        private bool IsAreaInLight(Area3D area)
        {
            // Query the light detector for any lit overlaps
            return area.GetOverlappingAreas()
                .Where(IsLightArea)
                .Select(GetLightArea)
                .Any(light => light.IsLit);
        }

        private bool IsLightArea(Area3D area)
        {
            return area.GetParent() is WorldLight;
        }

        private WorldLight GetLightArea(Area3D area)
        {
            if (area.GetParent() is WorldLight worldLight) return worldLight;
            return null;
        }
    }
}