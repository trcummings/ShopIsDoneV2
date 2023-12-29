using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Cameras;
using static ShopIsDone.Cameras.ScreenshakeHandler;
using Utils.Extensions;
using System.Threading.Tasks;

namespace ShopIsDone.Microgames.Apologize
{
    public partial class ApologizeMicrogame : Microgame
    {
        [Signal]
        public delegate void PromptSuccededEventHandler();

        [Signal]
        public delegate void PromptFailedEventHandler();

        [Export]
        public GDScript _ActionIconScript;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private Control _PromptContainer;
        // Customer
        private AnimatedSprite2D _CustomerHead;
        private AnimatedSprite2D _CustomerBody;
        // Employee
        private List<Node2D> _EmployeeStates;
        // Sorry
        private AnimatedSprite2D _BackgroundSplash;
        private List<AnimatedSprite2D> _Letters;
        // Effects
        private AnimatedSprite2D _RedX;
        private AnimatedSprite2D _GreenCheck;

        // State
        private List<string> _AllPrompts = new List<string>()
        {
            "move_left",
            "move_right",
            "move_up",
            "move_down",
            "ui_accept",
        };

        private List<string> _Prompts = new List<string>();
        

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _PromptContainer = GetNode<Control>("%PromptContainer");
            // Customer
            _CustomerHead = GetNode<AnimatedSprite2D>("%CustomerHead");
            _CustomerBody = GetNode<AnimatedSprite2D>("%CustomerBody");
            // Sorry
            _BackgroundSplash = GetNode<AnimatedSprite2D>("%BackgroundSplash");
            _Letters = GetNode<Node2D>("%Letters").GetChildren().OfType<AnimatedSprite2D>().ToList();
            _Letters.Reverse();
            // Employee
            _EmployeeStates = GetNode<Node2D>("%Employee").GetChildren().OfType<Node2D>().ToList();
            // Effects
            _RedX = GetNode<AnimatedSprite2D>("%RedX");
            _GreenCheck = GetNode<AnimatedSprite2D>("%GreenCheck");

            // Connect screenshake
            _Screenshake.ShakeOffsetUpdated += ShakeUpdate;
            // Connect to prompt events
            PromptSucceded += OnPromptSucceded;
            PromptFailed += async () => await FailGame();

            // Generate list of prompts
            var candidateList = _AllPrompts.Where(p => p != "ui_accept").ToList();
            _Prompts.Add(candidateList.PickRandom());
            _Prompts.Add(candidateList.PickRandom());
            _Prompts.Add(candidateList.PickRandom());
            _Prompts.Add(candidateList.PickRandom());
            _Prompts.Add("ui_accept");

            // Create the necessary buttons prompt buttons
            foreach (var prompt in _Prompts)
            {
                var button = new TextureRect();
                button.SetScript(_ActionIconScript);
                button.Set("action_name", prompt);
                _PromptContainer.AddChild(button);
            }

            // Do not allow player input
            SetProcess(false);
        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            // If no prompts,ignore this
            if (_Prompts.Count == 0) return;

            // Grab the latest prompt
            var latest = _Prompts.First();

            // If we pressed the correct input, emit correct prompt event
            if (Input.IsActionJustPressed(latest))
            {
                EmitSignal(nameof(PromptSucceded));
            }
            // Check if any actions were just pressed other than the current one
            // we want, and if so, emit failed prompt event
            else if (_AllPrompts.Where(p => p != latest).Any(p => Input.IsActionJustPressed(p)))
            {
                EmitSignal(nameof(PromptFailed));
            }
        }

        private void SpiralOffSprite(AnimatedSprite2D sprite)
        {
            // Pick random direction
            var dir = new List<float>() { 1, -1 }.PickRandom();
            // Show sprite
            sprite.Show();
            // Tween off
            var tween = CreateTween().BindNode(this);
            tween
                .Parallel()
                .TweenProperty(sprite, "position", new Vector2(dir * 75f + 75f, -100f), 0.25f)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(new Vector2(75f, 75f));
            tween
                .Parallel()
                .TweenProperty(sprite, "rotation_degrees", dir * 360f, 0.25f)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(0f);
            tween
                .Parallel()
                .TweenProperty(sprite, "scale", new Vector2(0f, 0f), 0.25f)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(new Vector2(1f, 1f));
            tween
                .Parallel()
                .TweenProperty(sprite, "modulate:a", 0f, 0.25f)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(1f);
        }

        private void SorryAnimation()
        {
            // Run background splash
            _BackgroundSplash.Play("default");
            _BackgroundSplash.AnimationFinished += OnSplashFinished;

            // Tween in letters in sequence
            // NB: We "Bind Node" here so if the animation doesn't cut short
            // godot doesn't error
            var lettersTween = CreateTween().BindNode(this).Parallel();
            for (int i = 0; i < _Letters.Count; i++)
            {
                var letter = _Letters[i];
                // Save final pos
                var finalPos = letter.Position;
                // Shift down
                letter.Translate(new Vector2(0, 200));
                // Tween
                lettersTween
                    .Parallel()
                    .TweenProperty(letter, "position", finalPos, 0.2f)
                    .SetTrans(Tween.TransitionType.Bounce)
                    .SetEase(Tween.EaseType.Out)
                    .SetDelay(i * 0.05f);
                // Show letter
                letter.Show();
            }

            return;
        }

        private void OnSplashFinished()
        {
            // Disconnect
            _BackgroundSplash.AnimationFinished -= OnSplashFinished;
            // Loop splash
            _BackgroundSplash.Play("loop");
        }

        private void OnPromptSucceded()
        {
            // Mild Screenshake
            _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Tiny)
            {
                Axis = ShakeAxis.XOnly
            });

            // Show green check bouncing away
            SpiralOffSprite(_GreenCheck);

            // Remove first prompt
            _Prompts.RemoveAt(0);
            // Remove the prompt button
            _PromptContainer.RemoveChild(_PromptContainer.GetChild(0));

            // If there's only one employee state left, just vibrate it
            if (_EmployeeStates.Count == 1)
            {
                var tween = CreateTween()
                    .BindNode(this)
                    .SetLoops(4)
                    .SetTrans(Tween.TransitionType.Elastic)
                    .SetEase(Tween.EaseType.OutIn);
                tween
                    .TweenProperty(_EmployeeStates[0], "position", Vector2.Right * 10, 0.05f);
                tween
                    .TweenProperty(_EmployeeStates[0], "position", Vector2.Left * 10, 0.05f);
            }
            // Otherwise, hide the current employee state, show the next one,
            // and remove the current one from the list
            else
            {
                _EmployeeStates[0].Hide();
                _EmployeeStates[1].Show();
                _EmployeeStates[1].GetNode<AnimatedSprite2D>("ApologizingEmployeeHead").Play("apologizing");
                _EmployeeStates.RemoveAt(0);
            }

            // If no more prompts, win!
            if (_Prompts.Count == 0)
            {
                Task _ = OnAllPromptsSucceded();
            }
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetProcess(false);
            // This is a failure case only, so play the sound
            PlayFailureSfx();
        }

        private async Task OnAllPromptsSucceded()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Mild Screenshake
            _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Mild)
            {
                Axis = ShakeAxis.XOnly
            });

            // Stop all player input
            SetProcess(false);

            // Make customer smug
            _CustomerHead.Play("satisfied");
            _CustomerBody.Play("satisfied");

            // Show SORRY animation
            SorryAnimation();

            // Wait a tick
            await ToSignal(GetTree().CreateTimer(1, false), "timeout");

            // Set outcome to win
            Outcome = Outcomes.Win;

            // Play success sound
            PlaySuccessSfx();

            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);

            return;
        }

        private async Task FailGame()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Heavy Screenshake
            _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Medium));

            // Show red x
            _RedX.Show();

            // Stop all player input
            SetProcess(false);

            // Make customer shocked
            _CustomerHead.Play("shocked");
            _CustomerBody.Play("shocked");

            // Show middle finger on employee and angry face
            var employee = _EmployeeStates[0];
            employee.GetNode<AnimatedSprite2D>("NormalArm").Hide();
            employee.GetNode<AnimatedSprite2D>("MiddleFingerArm").Show();
            employee.GetNode<AnimatedSprite2D>("ApologizingEmployeeHead").Play("angry");

            // Wait a tick
            await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);

            return;
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 100;
        }
    }
}
