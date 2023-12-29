using Godot;
using System;

namespace ShopIsDone.Cameras
{
    public partial class ScreenshakeHandler : Node
    {
        [Signal]
        public delegate void ShakeOffsetUpdatedEventHandler(Vector2 offset);

        [Export]
        public Noise Noise;

        // Shake type enum
        public enum ShakeType { Random, Sine, Noise };

        public enum ShakeAxis { Both, XOnly, YOnly };

        // State
        private ShakeType _ShakeType = ShakeType.Random;
        private float _Intensity = 0;
        private float _TotalDuration = 0;
        private double _CurrentDuration = 0;
        private float _Easing = 1;
        private ShakeAxis _Axis = ShakeAxis.Both;

        private RandomNumberGenerator _RNG = new RandomNumberGenerator();

        public override void _Process(double delta)
        {
            // Return early if we're out of shake duration
            if (_CurrentDuration <= 0.0)
            {
                _CurrentDuration = 0;
                EmitSignal(nameof(ShakeOffsetUpdated), Vector2.Zero);
                return;
            }

            // Subtract elapsed time from duration
            _CurrentDuration -= delta;

            // Damp the intensity
            var damping = (float)Mathf.Ease(_CurrentDuration / _TotalDuration, _Easing);
            var dampedIntensity = _Intensity * damping;

            // Create offset value
            Vector2 offset = Vector2.Zero;

            // Random shake, chaotic and violent
            if (_ShakeType == ShakeType.Random)
            {
                offset = new Vector2(_RNG.Randf(), _RNG.Randf()) * dampedIntensity;
            }

            // Sine wave based, continuous and smooth, based on OS ticks msec
            if (_ShakeType == ShakeType.Sine)
            {
                var ticks = Time.GetTicksMsec();
                offset = new Vector2(Mathf.Sin(ticks * 0.03f), Mathf.Sin(ticks * 0.07f)) * dampedIntensity * 0.5f;
            }

            // Noise based shake, continuous and smooth, based on OS ticks msec
            if (_ShakeType == ShakeType.Noise)
            {
                var ticks = Time.GetTicksMsec();
                var noiseX = Noise.GetNoise1D(ticks * 0.1f);
                var noiseY = Noise.GetNoise1D(ticks * 0.1f + 100);

                offset = new Vector2(noiseX, noiseY) * dampedIntensity * 2f;
            }

            // Modify offset by shake axis
            switch (_Axis)
            {
                case ShakeAxis.XOnly:
                    {
                        offset.Y = 0;
                        break;
                    }

                case ShakeAxis.YOnly:
                    {
                        offset.X = 0;
                        break;
                    }
            }

            // Emit offset
            EmitSignal(nameof(ShakeOffsetUpdated), offset);
        }

        public void Shake(ShakePayload payload)
        {
            // Override the shake if it's got more time on the clock
            if (payload.Duration > _CurrentDuration)
            {
                _Intensity = payload.Intensity;
                _TotalDuration = payload.Duration;
                _CurrentDuration = payload.Duration;
                _ShakeType = payload.ShakeType;
                _Easing = payload.Easing;
                _Axis = payload.Axis;
            }
        }

        public void HaltShake()
        {
            _Intensity = 0;
            _TotalDuration = 0;
            _CurrentDuration = 0;
        }

        public void Shake(
            ShakePayload.ShakeSizes size = ShakePayload.ShakeSizes.Mild,
            ShakeAxis axis = ShakeAxis.Both
        )
        {
            Shake(new ShakePayload(size) { Axis = axis });
        }

        // NB: This has to be a godot object so we can pass it through signals
        public partial class ShakePayload : GodotObject
        {
            public enum ShakeSizes
            {
                Tiny,
                Mild,
                Medium,
                Huge
            }

            public float Intensity;
            public float Duration;
            public ShakeType ShakeType = ShakeType.Random;
            public float Easing = 1;
            public ShakeAxis Axis = ShakeAxis.Both;

            public ShakePayload(ShakeSizes size = ShakeSizes.Mild)
            {
                switch (size)
                {
                    case ShakeSizes.Tiny:
                        {
                            Intensity = 0.05f;
                            Duration = 0.3f;
                            break;
                        }

                    case ShakeSizes.Mild:
                        {
                            Intensity = 0.125f;
                            Duration = 0.3f;
                            break;
                        }

                    case ShakeSizes.Medium:
                        {
                            Intensity = 1f;
                            Duration = 0.3f;
                            break;
                        }
                    case ShakeSizes.Huge:
                        {
                            Intensity = 2f;
                            Duration = 0.3f;
                            break;
                        }
                }
            }
        }

    }
}