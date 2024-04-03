using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class HealthBar : Control
    {
        [Signal]
        public delegate void DiedEventHandler();

        [Signal]
        public delegate void DamagedEventHandler();

        [Signal]
        public delegate void HealthSetEventHandler(int current, int total);

        // Health
        private int _MaxHealth = 3;
        private int _CurrentHealth = 3;

        // Drain
        private float _CurrentDrain = 0.0f;
        private bool _WasDrainedLastFrame = false;

        // iFrames
        private bool _InIFrames = false;
        private float _IFrameDuration = 1f;
        private float _IFrameTimer = 0.0f;


        public void Init(int initialHealth)
        {
            _CurrentHealth = initialHealth;
            EmitSignal(nameof(HealthSet), _CurrentHealth, _MaxHealth);
        }

        public void DamageHealth(int _)
        {
            // Ignore if dead
            if (_CurrentHealth == 0) return;

            // Tick down health
            _CurrentHealth -= 1;
            _CurrentHealth = Mathf.Max(0, _CurrentHealth);
            EmitSignal(nameof(Damaged));
            EmitSignal(nameof(HealthSet), _CurrentHealth, _MaxHealth);

            // If we died, die
            if (_CurrentHealth == 0)
            {
                // Stop updating
                SetProcess(false);
                // Set the health bar
                SetHealthBar();
                // Emit death signal
                EmitSignal(nameof(Died));
            }
            // Otherwise, give us iFrames
            else
            {
                _InIFrames = true;
                _IFrameTimer = 0;
            }
        }

        public void DrainHealth(float amount)
        {
            // Ignore if in iFrames
            if (_InIFrames || _CurrentHealth == 0) return;

            // Drain and set drain flag
            _CurrentDrain += amount;
            _WasDrainedLastFrame = true;

            // If we've passed the drain threshold, emit damage and clear drain
            if (_CurrentDrain >= 1)
            {
                _CurrentDrain = 0.0f;
                DamageHealth(1);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            // If we're in iFrames, countdown the iframes
            if (_InIFrames)
            {
                _IFrameTimer += (float)delta;
                if (_IFrameTimer >= _IFrameDuration)
                {
                    _InIFrames = false;
                    _IFrameTimer = 0;
                }
            }

            // If we were not drained last frame, or we're in iframes, recover
            // drain
            if (!_WasDrainedLastFrame || _InIFrames)
            {
                _CurrentDrain -= (float)delta / 5;
                _CurrentDrain = Mathf.Max(0, _CurrentDrain);
            }
            // Flip was drained call to false
            else _WasDrainedLastFrame = false;

            // Set the health bar display
            SetHealthBar();
        }

        private void SetHealthBar()
        {
            var icons = GetChildren().OfType<TextureProgressBar>().ToList();
            for (int i = 0; i < icons.Count(); i++)
            {
                var icon = icons[i];
                // If we're dead, just zero them all out
                if (_CurrentHealth == 0)
                {
                    icon.Value = 0;
                    continue;
                }

                // Show drain on the active icon
                if (i + 1 == _CurrentHealth)
                {
                    icon.Value = 100 - (_CurrentDrain * 100);
                }
                // Otherwise, if our index is over the health threshold, it's
                // empty
                else if (i + 1 > _CurrentHealth) icon.Value = 0;
                // If it's not, it's full
                else icon.Value = 100;
            }

            // If in iFrames, pulse transparent
            if (_InIFrames)
            {
                var newColor = Colors.Transparent;
                newColor.A = (Mathf.Sin(Time.GetTicksMsec() / 3) + 1) / 2;
                Modulate = newColor;
            }
            else Modulate = Colors.White;
        }
    }
}
