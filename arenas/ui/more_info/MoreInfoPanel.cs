using Godot;
using System;
using ShopIsDone.Models;

namespace ShopIsDone.Arenas.UI
{
    public partial class MoreInfoPayload : GodotObject
    {
        public string Title = "N/A";
        public string Description = "N/A";
        public IModel Model = new Model();
        public Vector3 Point = Vector3.Zero;
    }

    public partial class MoreInfoPanel : Control
    {
        // Nodes
        private Label _Title;
        private Label _Description;
        private MoreInfoRenderPreview _RenderPreview;

        public override void _Ready()
        {
            // Ready nodes
            _Title = GetNode<Label>("%Title");
            _Description = GetNode<Label>("%Description");
            _RenderPreview = GetNode<MoreInfoRenderPreview>("%RenderPreview");
        }

        public void Init(MoreInfoPayload payload)
        {
            _Title.Text = payload.Title;
            _Description.Text = payload.Description;

            // Add interactable render to preview
            _RenderPreview.AddRender(payload.Model as Node3D, payload.Point);
        }

        public void Reset()
        {
            // Reset title and description
            _Title.Text = "";
            _Description.Text = "";

            // Remove render from preview
            _RenderPreview.Reset();
        }
    }
}