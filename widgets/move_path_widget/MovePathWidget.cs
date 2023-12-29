using Godot;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Widgets
{
    public partial class MovePathWidget : Node3D, IService
    {
        // Nodes
        private Node3D _LineCap;
        private MeshInstance3D _LineCapMesh;
        private Node3D _ClosedLineBase;
        private Node3D _OpenLineBase;
        private Node3D _StraightLineTemplate;
        private Node3D _CurvedLineTemplate;
        private Node3D _PathNodes;

        private Tween _Tween;
        private ShaderMaterial _LineCapMaterial;

        public override void _Ready()
        {
            _PathNodes = GetNode<Node3D>("%PathNodes");
            _StraightLineTemplate = GetNode<Node3D>("%StraightLineTemplate");
            _CurvedLineTemplate = GetNode<Node3D>("%CurvedLineTemplate");
            _ClosedLineBase = GetNode<Node3D>("%ClosedLineBase");
            _OpenLineBase = GetNode<Node3D>("%OpenLineBase");
            _LineCap = GetNode<Node3D>("%LineCap");
            _LineCapMesh = GetNode<MeshInstance3D>("%LineCapMesh");

            _LineCapMaterial = (ShaderMaterial)_LineCapMesh.GetActiveMaterial(0);
        }

        public void SetIsValidPath(bool value)
        {
            // Get the current gradient strength of the line cap
            var gradientStrength = _LineCapMaterial.GetShaderParameter("gradient_strength");

            if (_Tween != null)
            {
                _Tween.Kill();
                _Tween = null;
            }
            _Tween = GetTree()
                .CreateTween()
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Bounce);

            // If it's an invalid path, tween to full gradient strength
            // If it's a valid path, tween to 0 strength
            if (!value)
            {
                _Tween.TweenMethod(new Callable(this, nameof(SetGradientStrength)), gradientStrength, 2.0, 0.5f);
            }
            else
            {
                _Tween.TweenMethod(new Callable(this, nameof(SetGradientStrength)), gradientStrength, 0, 0.5f);
            }
        }

        private void SetGradientStrength(float value)
        {
            _LineCapMaterial.SetShaderParameter("gradient_strength", value);
        }

        public void SetPath(List<Vector3> points)
        {
            // Clear all the path nodes in the list
            foreach (var node in _PathNodes.GetChildren().OfType<Node3D>())
            {
                _PathNodes.RemoveChild(node);
                node.QueueFree();
            }

            // Hide everything
            _LineCap.Hide();
            _ClosedLineBase.Hide();
            _OpenLineBase.Hide();

            // If the list is empty, return early
            if (points.Count == 0) return;

            // If there's only one point in the list, just show the closed path base
            // in its proper position
            if (points.Count == 1)
            {
                _ClosedLineBase.GlobalPosition = points.First();
                _ClosedLineBase.Show();
                return;
            }

            // Generate a series of points between each point on the given points
            // line that excludes the first and last points in the list but uses
            // them to calculate the points
            var infinitePoint = new Vector3(Mathf.Inf, Mathf.Inf, Mathf.Inf);
            points
                .WithPreviousAndNext(infinitePoint, infinitePoint)
                .ToList()
                .ForEach((record) =>
                {
                    // Get each point
                    var prevPoint = record.Previous;
                    var currentPoint = record.Current;
                    var nextPoint = record.Next;

                    // If this is the first element, translate, rotate, and show the
                    // open line base
                    if (prevPoint == infinitePoint)
                    {
                        _OpenLineBase.GlobalPosition = currentPoint;
                        _OpenLineBase.LookAt(nextPoint, Vector3.Up);
                        _OpenLineBase.Show();
                        return;
                    }

                    // If this is the last element, translate, rotate, and show the
                    // point cap
                    if (nextPoint == infinitePoint)
                    {
                        _LineCap.GlobalPosition = currentPoint;
                        _LineCap.LookAt(prevPoint, Vector3.Up);
                        _LineCap.Show();
                        return;
                    }

                    // Otherwise, get the vectors between the previous and next points
                    var prevToCurrent = currentPoint - prevPoint;
                    var currentToNext = nextPoint - currentPoint;

                    // Create a template var
                    Node3D template;

                    // If they're the same, then it's a straight line
                    if (prevToCurrent == currentToNext) template = _StraightLineTemplate;
                    else template = _CurvedLineTemplate;

                    // Create node and translate
                    var node = (Node3D)template.Duplicate();
                    _PathNodes.AddChild(node);
                    node.GlobalPosition = currentPoint;

                    // Rotate the node 
                    if (prevToCurrent == currentToNext) node.LookAt(nextPoint, Vector3.Up);
                    else
                    {
                        var angle = prevToCurrent.SignedAngleTo(currentToNext, Vector3.Up);
                        if (angle < 0) node.LookAt(nextPoint, Vector3.Up);
                        else node.LookAt(prevPoint, Vector3.Up);
                    }

                    // Show the node
                    node.Show();
                });
        }
    }
}