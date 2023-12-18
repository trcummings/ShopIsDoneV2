using Godot;
using System;
using System.Collections.Generic;
using Camera;
using static Camera.ScreenshakeHandler;
using Utils.Extensions;

namespace Microgames.ScanItem
{
    public partial class ScanItemMicrogame : Microgame
    {
        [Signal]
        public delegate void CubeRotatedEventHandler();

        [Signal]
        public delegate void ScanTriggeredEventHandler();

        [Signal]
        public delegate void ScanSuccessfulEventHandler();

        [Signal]
        public delegate void ScanFailedEventHandler();

        [Export]
        public float BoxMargin = 0.1f;

        // All sides except for front
        private List<Vector3> Sides = new List<Vector3>()
        {
            Vector3.Left,
            Vector3.Right,
            Vector3.Back,
            Vector3.Down,
            Vector3.Up
        };

        private Node3D _ItemBox;
        private Node3D _BarcodePivot;
        private MeshInstance3D _Barcode;
        private Camera3D _Camera;
        private Node3D _Judge;
        private Node3D _Candidate;
        private ScreenshakeHandler _Screenshake;
        private Sprite3D _Dither;

        // State
        private Vector3 _ChosenSide = Vector3.Forward;
        private Tween _Tween;

        public override void _Ready()
        {
            base._Ready();

            _ItemBox = GetNode<Node3D>("%ScannableItem");
            _BarcodePivot = GetNode<Node3D>("%BarcodePivot");
            _Barcode = GetNode<MeshInstance3D>("%Barcode");
            _Camera = GetNode<Camera3D>("%Camera");
            _Judge = GetNode<Node3D>("%Judge");
            _Candidate = GetNode<Node3D>("%Candidate");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Dither = GetNode<Sprite3D>("%Dither");

            // Connect to scan event
            Connect(nameof(ScanTriggered), new Callable(this, nameof(OnScanTriggered)));

            // Connect screenshake
            _Screenshake.Connect(nameof(ScreenshakeHandler.ShakeOffsetUpdated), new Callable(this, nameof(ShakeUpdate)));

            // Place barcode on random side
            _ChosenSide = Sides.PickRandom();
            PlaceBarcodeRandomly(_ChosenSide);

            // Do not allow player input
            SetProcess(false);

            // Minor fluctuations of camera rotation
            var cTween = CreateTween().SetLoops();
            cTween
                .TweenProperty(_Camera, "rotation_degrees:z", 1.5f, 1f)
                .AsRelative();
            cTween
                .Parallel()
                .TweenProperty(_Camera, "rotation_degrees:x", 0.5f, 0.5f)
                .AsRelative();
            cTween
                .Chain()
                .TweenProperty(_Camera, "rotation_degrees:z", -1.5f, 1f)
                .AsRelative();
            cTween
                .Parallel()
                .TweenProperty(_Camera, "rotation_degrees:x", -0.5f, 0.5f)
                .AsRelative();
        }

        public override void Start()
        {
            // Base call
            base.Start();

            // Allow player input
            SetProcess(true);
        }

        public async override void _Process(double delta)
        {
            // On tap directional button, rotate the item in that direction
            if (Input.IsActionJustPressed("move_up"))
            {
                RotateItem(Vector3.Right);
                EmitSignal(nameof(CubeRotated));
            }
            else if (Input.IsActionJustPressed("move_down"))
            {
                RotateItem(Vector3.Right);
                EmitSignal(nameof(CubeRotated));
            }
            else if (Input.IsActionJustPressed("move_right"))
            {
                RotateItem(Vector3.Up);
                EmitSignal(nameof(CubeRotated));
            }
            else if (Input.IsActionJustPressed("move_left"))
            {
                RotateItem(Vector3.Up);
                EmitSignal(nameof(CubeRotated));
            }

            // On press "Scan", play anim, stop the game, evaluate
            if (Input.IsActionJustPressed("ui_accept"))
            {
                SetProcess(false);
                EmitSignal(nameof(ScanTriggered));

                // Slam the current tween amount to max
                if (_Tween != null && _Tween.IsValid()) _Tween.CustomStep(1000000.0f);

                // Calculate if we're on the correct side
                var isOnCorrectSide = _Judge.GlobalPosition.DistanceTo(_Candidate.GlobalPosition) < 0.1;
                // If we're on the correct side, win!
                if (isOnCorrectSide) Outcome = Outcomes.Win;
                // Otherwise, fail
                else Outcome = Outcomes.Loss;

                // Screenshake
                _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Mild)
                {
                    Axis = ShakeAxis.XOnly
                });

                // Stop the timer
                MicrogameTimer.Stop();

                // Wait out the scan animation
                await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

                // Emit successful or failed scan
                if (Outcome == Outcomes.Win)
                {
                    EmitSignal(nameof(ScanSuccessful));
                }
                else
                {
                    EmitSignal(nameof(ScanFailed));
                }

                // Wait out new animation + sound
                await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

                // Play appropriate sfx
                if (Outcome == Outcomes.Win) PlaySuccessSfx();
                else PlayFailureSfx();

                // Finish microgame
                EmitSignal(nameof(MicrogameFinished), (int)Outcome);
            }
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetProcess(false);
            // This is a failure case only, so play the sound
            PlayFailureSfx();
        }

        private void RotateItem(Vector3 axis)
        {
            // Slam the current tween amount to max
            if (_Tween != null && _Tween.IsValid()) _Tween.CustomStep(1000000.0f);
            // Create a new tween
            _Tween = CreateTween();
            // Rotate
            _Tween
                .TweenProperty(_ItemBox, "transform:basis", _ItemBox.Transform.Basis.Rotated(axis, Mathf.Pi / 2), 0.25f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic);
        }

        private void PlaceBarcodeRandomly(Vector3 side)
        {
            // Get height and width of box + barcode (in half)
            var boxDims = new Vector3(2f, 2f, 2f);
            var barcodeDims = (_Barcode.Mesh as QuadMesh).Size;

            // Randomly rotate barcode
            var randAngle = (float)GD.RandRange(0, Mathf.Tau);
            _Barcode.Rotate(Vector3.Back, randAngle);

            // Get rotated barcode dims
            var rotatedDims = new Vector2(
                (barcodeDims.X * Mathf.Abs(Mathf.Cos(randAngle))) + (barcodeDims.Y * Mathf.Abs(Mathf.Sin(randAngle))),
                (barcodeDims.Y * Mathf.Abs(Mathf.Cos(randAngle))) + (barcodeDims.X * Mathf.Abs(Mathf.Sin(randAngle)))
            );

            // Calculate random position for rotated barcode and position it
            // (extruded on z)
            var x = (boxDims.X - (BoxMargin * 2) - rotatedDims.X) / 2;
            var y = (boxDims.Y - (BoxMargin * 2) - rotatedDims.Y) / 2;
            var pos = new Vector2((float)GD.RandRange(-x, x), (float)GD.RandRange(-y, y));
            _Barcode.Position = new Vector3(pos.X, pos.Y, 1.001f);

            // Slerp the barcode pivot towards the side
            var barcodeTransform = _BarcodePivot.Transform.Orthonormalized();
            var newBasis = new Basis(SideToQuat(side));
            barcodeTransform.Basis = newBasis;
            _BarcodePivot.Transform = barcodeTransform;
        }

        public Quaternion SideToQuat(Vector3 side)
        {
            // Get the "up vector", swapping it out if the vector is Z up or down
            var upVector = Vector3.Up;
            if (side == Vector3.Up) upVector = Vector3.Left;
            if (side == Vector3.Down) upVector = Vector3.Right;
            var sideTransform = new Transform3D()
                .LookingAt(side, upVector)
                .Translated(side)
                .Orthonormalized();

            return new Quaternion(sideTransform.Basis);
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.HOffset = offset.X;
            _Camera.VOffset = offset.Y;
        }

        private void OnScanTriggered()
        {
            // tween dither in and out
            var prevAlpha = _Dither.Modulate.A;
            var tween = CreateTween();
            tween.TweenProperty(_Dither, "modulate:a", 0.8, 0.15f);
            tween.Chain().TweenProperty(_Dither, "modulate:a", prevAlpha, 0.15f);

        }
    }
}