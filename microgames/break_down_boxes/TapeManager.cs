using System;
using System.Linq;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Microgames.BreakDownBoxes
{
	public partial class TapeManager : Node
    {
		[Signal]
		public delegate void AllPointsCutEventHandler();

        [Export]
        private PackedScene TapeCutPointScene;

		public bool IsTapeCut = false;

        private string _TapeGroup;
		private Node3D _StartPoint;
		private Node3D _EndPoint;
		private Array<Sprite3D> _TapePieces = new Array<Sprite3D>();
		private Array<TapeCutPoint> _CutPoints = new Array<TapeCutPoint>();
		private AudioStreamPlayer _PointCutPlayer;

        public override void _Ready()
        {
            base._Ready();
			_PointCutPlayer = GetNode<AudioStreamPlayer>("%PointCutPlayer");
        }

        public void Init(string tapeGroup)
		{
			_TapeGroup = tapeGroup;

			// Get all nodes in the tape group
			var allItems = GetTree()
				.GetNodesInGroup(_TapeGroup)
				.OfType<Node3D>();

			// Filter out the tape pieces
			_TapePieces = allItems
				.OfType<Sprite3D>()
				.Where(s => s.IsInGroup(BreakDownBoxesMicrogame.TAPE_GROUP))
				.ToGodotArray();

			// Get start and end point
			_StartPoint = allItems
				.ToList()
				.Find(n => n.Name.ToString().Contains("StartPoint"));
            _EndPoint = allItems
				.ToList()
				.Find(n => n.Name.ToString().Contains("EndPoint"));
        }

		public void GenerateCutPoints(int numPoints, Camera3D camera, Node2D pointParent)
		{
			// Unproject the start and end points
			var start = camera.UnprojectPosition(_StartPoint.GlobalPosition);
			var end = camera.UnprojectPosition(_EndPoint.GlobalPosition);

			// Create the points and append the start and end to it
			var points = GeneratePointsBetween(start, end, numPoints);
			points.Add(start);
			points.Add(end);

			// Loop over the points and create one at each point
			foreach (var point in points)
			{
				var tapeCutPoint = TapeCutPointScene.Instantiate<TapeCutPoint>();
				pointParent.AddChild(tapeCutPoint);
				tapeCutPoint.GlobalPosition = point;
				tapeCutPoint.PointCut += OnPointCut;
				_CutPoints.Add(tapeCutPoint);
			}
        }

		private void OnPointCut()
		{
			// Collect the number of cut points
			var numCut = _CutPoints.Where(p => p.WasCut).Count();
			// Pitch up based on the number of cut lines in this tape
			_PointCutPlayer.PitchScale = 1f + (.3f * (numCut / (float)_CutPoints.Count));
			_PointCutPlayer.Play();

            // Check all points if they're cut, then emit all points cut
            if (_CutPoints.All(p => p.WasCut))
			{
				SetTapeCut();
				EmitSignal(nameof(AllPointsCut));
            }
		}

		private void SetTapeWhole()
		{
            foreach (var tape in _TapePieces) tape.Frame = 0;
        }

        public void SetTapeCut()
        {
            foreach (var tape in _TapePieces) tape.Frame = 1;
			IsTapeCut = true;
        }

		private void SetTapeCenterCut()
		{
            foreach (var tape in _TapePieces) tape.Frame = 2;
        }

        private Array<Vector2> GeneratePointsBetween(Vector2 start, Vector2 end, int numberOfPoints)
        {
            Array<Vector2> points = new Array<Vector2>();

            // The distance to interpolate by for each step
            double stepX = (end.X - start.X) / (numberOfPoints + 1);
            double stepY = (end.Y - start.Y) / (numberOfPoints + 1);

            for (int i = 1; i <= numberOfPoints; i++)
            {
                double newX = start.X + (stepX * i);
                double newY = start.Y + (stepY * i);
                points.Add(new Vector2((float)newX, (float)newY));
            }

            return points;
        }
    }
}

