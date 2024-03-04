using Godot;
using System;
using ShopIsDone.Tiles;
using ShopIsDone.Widgets;

namespace ShopIsDone.Lighting.Silhouettes
{
    public partial class GridEffect : Control
    {
        [Export]
        private TileCursor _TileCursor;

        [Export]
        private Camera3D _Camera;

        [Export]
        public SubViewport _GridSubViewport;

        //[Export]
        //public NodePath ObjectViewportPath;

        //[Export]
        //public NodePath PawnViewportPath;

        [Export]
        public SubViewport _LightSubViewport;

        [Export]
        public SubViewport _WidgetsViewport;

        //// Nodes
        //private TileCursor _TileCursor;
        //private Camera3D _Camera;

        // State
        private Vector3 _GridPointDestination = Vector3.Zero;
        private Vector3 _CurrentPoint = Vector3.Zero;

        private float _FadeDestination = 0f;
        private float _CurrentFade = 0f;

        public override void _Ready()
        {
            Modulate = Colors.White;

            // Connect to the cursor's change event
            _TileCursor.CursorEnteredTile += OnCursorMoved;
            _TileCursor.VisibilityChanged += OnCursorVisibilityChanged;

            // Initialize the shader material's viewport textures
            var material = (ShaderMaterial)Material;
            material.SetShaderParameter("grid_texture", _GridSubViewport.GetTexture());
            material.SetShaderParameter("widgets_texture", _WidgetsViewport.GetTexture());
            //material.SetShaderParameter("objects_texture", GetNode<Viewport>(ObjectViewportPath).GetTexture());
            //material.SetShaderParameter("pawns_texture", GetNode<Viewport>(PawnViewportPath).GetTexture());
            material.SetShaderParameter("light_volumes_texture", _LightSubViewport.GetTexture());
            //material.SetShaderParameter("top_level_widgets_texture", GetNode<Viewport>(TopLevelWidgetsViewportPath).GetTexture());
        }

        public override void _Process(double delta)
        {
            // Pull out material
            var material = (ShaderMaterial)Material;

            // Get camera's viewport rect size
            var cameraSize = _Camera.GetViewport().GetVisibleRect().Size;
            // Update grid center lerp and params
            _CurrentPoint = _CurrentPoint.Lerp(_GridPointDestination, (float)delta * 5);
            // Get the clip space position of the point
            var gridPoint = _Camera.UnprojectPosition(_CurrentPoint);
            // Flip the y point's offset (so we're oriented to screen x/y)
            gridPoint.Y = cameraSize.Y - gridPoint.Y;
            // Adjust the grid point to account for UI scaling
            gridPoint *= GetViewportRect().Size / cameraSize;
            // Set the shader param
            material.SetShaderParameter("grid_point", gridPoint);

            // Update grid fade lerp and params
            _CurrentFade = Mathf.Lerp(_CurrentFade, _FadeDestination, (float)delta * 5);
            material.SetShaderParameter("grid_fade", _CurrentFade);
        }

        private void OnCursorMoved(Tile tile)
        {
            _GridPointDestination = tile.GlobalPosition;
        }

        private void OnCursorVisibilityChanged()
        {
            if (_TileCursor.Visible) _FadeDestination = 1f;
            else _FadeDestination = 0f;
        }
    }
}
