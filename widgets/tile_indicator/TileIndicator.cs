using Godot;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Widgets
{
    public partial class TileIndicator : Node3D
    {
        public enum IndicatorColor
        {
            Red,
            Green,
            Blue,
            Yellow,
            Grey
        }

        // Nodes
        private Node3D _Indicators;
        private MeshInstance3D _GreenIndicator;
        private MeshInstance3D _RedIndicator;
        private MeshInstance3D _BlueIndicator;
        private MeshInstance3D _YellowIndicator;
        private MeshInstance3D _GreyIndicator;

        public override void _Ready()
        {
            // Ready nodes
            _Indicators = GetNode<Node3D>("%Indicators");
            _GreenIndicator = GetNode<MeshInstance3D>("%GreenIndicator");
            _BlueIndicator = GetNode<MeshInstance3D>("%BlueIndicator");
            _RedIndicator = GetNode<MeshInstance3D>("%RedIndicator");
            _YellowIndicator = GetNode<MeshInstance3D>("%YellowIndicator");
            _GreyIndicator = GetNode<MeshInstance3D>("%GreyIndicator");
        }

        public void ClearIndicators()
        {
            foreach (Node child in _Indicators.GetChildren())
            {
                _Indicators.RemoveChild(child);
                child.QueueFree();
            }
        }

        public void CreateIndicators(IEnumerable<Vector3> tiles, IndicatorColor color)
        {
            foreach (var tile in tiles) CreateIndicator(tile, color);
        }


        // Global position
        public void CreateIndicator(Vector3 tile, IndicatorColor color)
        {
            MeshInstance3D indicatorNodeTemplate = null;

            switch (color)
            {
                case IndicatorColor.Red:
                    indicatorNodeTemplate = _RedIndicator;
                    break;

                case IndicatorColor.Blue:
                    indicatorNodeTemplate = _BlueIndicator;
                    break;

                case IndicatorColor.Green:
                    indicatorNodeTemplate = _GreenIndicator;
                    break;

                case IndicatorColor.Yellow:
                    indicatorNodeTemplate = _YellowIndicator;
                    break;

                case IndicatorColor.Grey:
                    indicatorNodeTemplate = _GreyIndicator;
                    break;
            }

            // Create new node
            var node = (MeshInstance3D)indicatorNodeTemplate.Duplicate();

            // Add it to the indicators
            _Indicators.AddChild(node);

            // Position it
            node.GlobalTranslate(tile);

            // Show it
            node.Show();
        }
    }
}