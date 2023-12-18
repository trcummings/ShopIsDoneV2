using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Camera;
using static Camera.ScreenshakeHandler;
using Utils.Extensions;

namespace Microgames.DestroyRottenProduce
{
    public partial class DestroyRottenProduceMicrogame : Microgame
    {
        [Signal]
        public delegate void HitProduceEventHandler();

        [Export]
        public int FistSpeed = 10;

        [Export(PropertyHint.Range, "1, 3, 1")]
        public int Difficulty = 3;

        [Export]
        public PackedScene FreshCabbage;

        [Export]
        public PackedScene RottenCabbage;

        [Export]
        public PackedScene FreshBroccoli;

        [Export]
        public PackedScene RottenBroccoli;

        [Export]
        public PackedScene FreshPotato;

        [Export]
        public PackedScene RottenPotato;

        private ReticuleFist _ReticuleFist;
        private Node2D _BottomCorner;
        private ScreenshakeHandler _Screenshake;
        private Camera2D _Camera;
        private Node2D _Produce;

        private ShaderMaterial _BackdropEffect;

        public override void _Ready() 
        {
            _ReticuleFist = GetNode<ReticuleFist>("%ReticuleFist");
            _BottomCorner = GetNode<Node2D>("%BottomCorner");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Camera = GetNode<Camera2D>("%Camera");
            _Produce = GetNode<Node2D>("%Produce");

            // Ready effect
            _BackdropEffect = GetNode<TextureRect>("%Backdrop").Material as ShaderMaterial;
            _BackdropEffect.SetShaderParameter("effect_scale", 1.1);
            _BackdropEffect.SetShaderParameter("effect", -0.08);

            // Stop processing until the microgame starts
            SetProcess(false);
            SetPhysicsProcess(false);

            // Connect to fist signal
            _ReticuleFist.Connect(nameof(ReticuleFist.HitProduce), new Callable(this, nameof(OnFistHitProduce)));

            // Connect screenshake
            _Screenshake.Connect(nameof(ScreenshakeHandler.ShakeOffsetUpdated), new Callable(this, nameof(ShakeUpdate)));

            // Fill produce list
            var difficulty = 3;

            var produce = new List<PackedScene>();
            if (difficulty >= 1) produce.Add(RottenCabbage);
            else produce.Add(FreshCabbage);
            produce.Add(FreshCabbage);
            produce.Add(FreshCabbage);
            produce.Add(FreshCabbage);

            if (difficulty >= 2) produce.Add(RottenBroccoli);
            else produce.Add(FreshBroccoli);
            produce.Add(FreshBroccoli);
            produce.Add(FreshBroccoli);
            produce.Add(FreshBroccoli);

            if (difficulty >= 3) produce.Add(RottenPotato);
            else produce.Add(FreshPotato);
            produce.Add(FreshPotato);
            produce.Add(FreshPotato);
            produce.Add(FreshPotato);

            // Shuffle the list
            produce = produce.Shuffle().ToList();

            var max = _BottomCorner.Position;
            var rowHeight = max.Y / 3;
            var halfRowHeight = rowHeight / 2;
            var columnWidth = max.X / 4;
            var halfColumnWidth = columnWidth / 2;

            // Generate produce nodes
            // Rows
            for (int i = 0; i < 3; i++)
            {
                // Columns
                for (int j = 0; j < 4; j++)
                {
                    var idx = (i * 4) + j;
                    var item = produce[idx];
                    var pos = new Vector2(
                        (j * columnWidth) + halfColumnWidth,
                        (i * rowHeight) + halfRowHeight
                    );
                    var deg = GD.Randi() % 360;
                    var node = item.Instantiate<BaseProduceItem>();
                    _Produce.AddChild(node);
                    node.Position = pos;
                    node.RotationDegrees = deg;
                }
            }
        }

        public override void Start()
        {
            base.Start();
            TweenBackdrop();
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public override void _Process(double delta)
        {
            // Fist input
            if (!_ReticuleFist.IsLaunching && Input.IsActionJustPressed("ui_accept"))
            {
                _ReticuleFist.LaunchFist();
            }

            // Check for victory
            var allRottenHit = _Produce
                .GetChildren()
                .OfType<BaseProduceItem>()
                .Where(p => p.IsRotten)
                .All(p => p.WasHit);
            if (allRottenHit) WinGame();
        }

        public override void _PhysicsProcess(double delta)
        {
            // Ignore if launching
            if (_ReticuleFist.IsLaunching) return;

            // Move reticule
            var moveDir = GetDirInput();
            // Flip move dir in y direction
            moveDir.Y = -moveDir.Y;
            // Translate
            _ReticuleFist.Translate(moveDir * FistSpeed);
            // Clamp
            var pos = _ReticuleFist.Position;
            var max = _BottomCorner.Position;
            _ReticuleFist.Position = new Vector2(
                Mathf.Clamp(pos.X, 0, max.X),
                Mathf.Clamp(pos.Y, 0, max.Y)
            );

            // Update background effect
            var halfMax = max / 2;
            var xPercent = (pos.X - halfMax.X) / halfMax.X;
            var yPercent = (pos.Y - halfMax.Y) / halfMax.Y;
            _BackdropEffect.SetShaderParameter("x_offset", xPercent * 0.2);
            _BackdropEffect.SetShaderParameter("y_offset", yPercent * 0.2);
        }

        private void OnFistHitProduce(BaseProduceItem produceItem)
        {
            // Play hit noise
            EmitSignal(nameof(HitProduce));

            // Have produce item take hit
            produceItem.TakeHit();

            // Play screenshake
            // If it's not rotten produce, fail the game
            if (!produceItem.IsRotten)
            {
                // Mild Screenshake
                _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Mild)
                {
                    Axis = ShakeAxis.XOnly
                });

                // Fail the game
                FailGame();
            }
            // Otherwise, tiny shake, and handle rotten produce
            else
            {
                _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Tiny)
                {
                    Axis = ShakeAxis.XOnly
                });
            }
        }

        private async void WinGame()
        {
            StopGame();

            // Wait a tick
            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            // TODO: Transition to post-victory cutscene

            // Set outcome to win
            Outcome = Outcomes.Win;

            // Play success sound
            PlaySuccessSfx();

            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private void FailGame()
        {
            StopGame();

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            // Finish early
            FinishEarly();
        }

        protected override void OnTimerFinished()
        {
            StopGame();

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();
        }

        private void StopGame()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            // Pause all ongoing animals from moving
            foreach (var item in _Produce.GetChildren().OfType<BaseProduceItem>())
            {
                item.Stop();
            }
        }

        private void TweenBackdrop()
        {
            var tween = GetTree().CreateTween();
            // Tween fisheye effect scaling param
            tween
                .TweenProperty(
                    _BackdropEffect,
                    "shader_param/effect_scale",
                    2,
                    GetSecondsPerBeat() * NumBeats
                )
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
            // Tween fisheye effect
            tween
                .Parallel()
                .TweenProperty(
                    _BackdropEffect,
                    "shader_param/effect",
                    2,
                    GetSecondsPerBeat() * NumBeats
                )
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 1000;
        }
    }
}