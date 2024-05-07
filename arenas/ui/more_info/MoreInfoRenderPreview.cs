using Godot;
using ShopIsDone.Utils.Extensions;
using System;

namespace ShopIsDone.Arenas.UI
{
    public partial class MoreInfoRenderPreview : Control
    {
        // Nodes
        private Node3D _RenderPreviewContainer;
        private Node3D _RenderSpot;

        public override void _Ready()
        {
            // Ready nodes
            _RenderPreviewContainer = GetNode<Node3D>("%RenderPreview");
            _RenderSpot = GetNode<Node3D>("%RenderSpot");
        }

        public override void _Process(double delta)
        {
            _RenderPreviewContainer.Rotate(Vector3.Up, (float)delta * 0.5f);
        }

        public void AddRender(Node3D node, Vector3 offset)
        {
            _RenderSpot.AddChild(node);
            node.Position -= offset;
        }

        public void Reset()
        {
            _RenderSpot.RemoveChildrenOfType<Node3D>();
        }
    }
}

