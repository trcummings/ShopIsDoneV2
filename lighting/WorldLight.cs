using Godot;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Lighting
{
    // Base arena light class for lights + light volumes
    [Tool]
    public partial class WorldLight : Node3D
    {
        [Signal]
        public delegate void FlickeredOnEventHandler();

        [Signal]
        public delegate void FlickeredOffEventHandler();

        [Export]
        public bool UseLightRadiusAsVolume = true;

        [Export]
        public bool IsLit
        {
            get { return _IsLit; }
            set {
                _IsLit = value;
                if (value) TurnOn();
                else TurnOff();
            }
        }
        private bool _IsLit = false;

        [Export]
        public float Radius
        {
            get { return _Radius; }
            set { SetRadius(value); }
        }
        private float _Radius = 2;

        [Export]
        public float LightEnergy
        {
            get { return _LightEnergy; }
            set { SetLightEnergy(value); }
        }
        private float _LightEnergy = 1;

        [Export]
        public float LightAngle
        {
            get { return _LightAngle; }
            set { SetLightAngle(value); }
        }
        private float _LightAngle = 1;

        [Export]
        public Color LightColor
        {
            get { return _LightColor; }
            set { SetLightColor(value); }
        }
        private Color _LightColor = Colors.White;

        // Debug only for editor
        [Export]
        public bool DebugViewVolume
        {
            get { return _DebugViewVolume; }
            set
            {
                if (!Engine.IsEditorHint() || _LightVolumeMesh == null) return;
                _DebugViewVolume = value;
                if (value) _LightVolumeMesh.Show();
                else _LightVolumeMesh.Hide();
            }
        }
        private bool _DebugViewVolume = false;

        private Area3D _LightArea;
        private CollisionShape3D _LightShape;
        private MeshInstance3D _LightVolumeMesh;
        private AudioStreamPlayer _LightFlickerSfx;

        // We don't know whether the light is a spot light or omni light, so we
        // need to decide under the hood like this
        private OmniLight3D _OmniLight;
        private SpotLight3D _SpotLight;
        private Light3D _Light
        {
            get
            {
                if (_OmniLight != null) return _OmniLight;
                else if (_SpotLight != null) return _SpotLight;
                return null;
            }
        }

        public override void _Ready()
        {
            base._Ready();
            _SpotLight = GetNodeOrNull<SpotLight3D>("%Light");
            _OmniLight = GetNodeOrNull<OmniLight3D>("%Light");
            _LightArea = GetNode<Area3D>("%LightArea");
            _LightShape = GetNode<CollisionShape3D>("%LightShape");
            _LightVolumeMesh = GetNode<MeshInstance3D>("%LightVolumeMesh");
            _LightFlickerSfx = GetNodeOrNull<AudioStreamPlayer>("%LightFlickerSfxPlayer");

            // Hide light mesh if we're in the editor, otherwise show it
            if (Engine.IsEditorHint() && !_DebugViewVolume) _LightVolumeMesh.Hide();
            else _LightVolumeMesh.Show();

            // Set light color
            if (_Light != null) _Light.LightColor = _LightColor;

            // Duplicate meshes
            if (UseLightRadiusAsVolume) DuplicateMeshes();

            // Set radius
            SetRadius(Radius);
        }

        protected virtual void SetRadius(float radius)
        {
            _Radius = radius;

            if (_Light is OmniLight3D omni) omni.OmniRange = radius;
            else if (_Light is SpotLight3D spot) spot.SpotRange = radius;

            if (UseLightRadiusAsVolume) UpdateLightVolume();
        }

        private void UpdateLightVolume()
        {
            if (_LightShape == null || _LightVolumeMesh == null) return;

            // Handle collision shape update
            if (_LightShape.Shape is SphereShape3D sphere)
            {
                // Set sphere radius
                sphere.Radius = Radius;
            }
            else if (_LightShape.Shape is BoxShape3D box)
            {
                // Set collision shape radius to equal Radius
                box.Size = new Vector3(Radius / 2f * Mathf.Sqrt2, 1, Radius / 2f * Mathf.Sqrt2);
            }

            // Handle mesh size update
            if (_LightVolumeMesh.Mesh is SphereMesh sphereMesh)
            {
                sphereMesh.Radius = Radius;
                sphereMesh.Height = 2 * Radius;
            }
            else if (_LightVolumeMesh.Mesh is CylinderMesh cylinder)
            {
                if (_Light is SpotLight3D)
                {
                    var angleRad = Mathf.DegToRad(LightAngle);
                    var w = Mathf.Sin(angleRad) * Radius;
                    var h = Mathf.Cos(angleRad) * Radius;
                    cylinder.Height = h;
                    cylinder.TopRadius = 0;
                    cylinder.BottomRadius = w;
                    _LightVolumeMesh.Position = _LightVolumeMesh.Position with
                    {
                        Y = _Light.Position.Y - (h / 2)
                    };
                }
                else
                {
                    cylinder.Height = Radius;
                    cylinder.TopRadius = Radius;
                    cylinder.BottomRadius = Radius;
                }
            }
        }

        protected void SetLightEnergy(float value)
        {
            _LightEnergy = value;
            if (_Light == null) return;
            _Light.LightEnergy = value;
        }

        protected void SetLightColor(Color value)
        {
            _LightColor = value;
            if (_Light == null) return;
            _Light.LightColor = value;
        }

        protected void DuplicateMeshes()
        {
            // Duplicate meshes
            _LightShape.Shape = (Shape3D)_LightShape.Shape.Duplicate();
            _LightVolumeMesh.Mesh = (Mesh)_LightVolumeMesh.Mesh.Duplicate();
        }

        private void SetLightAngle(float value)
        {
            _LightAngle = value;
            if (_Light is SpotLight3D spot)
            {
                spot.SpotAngle = value;
            }

            if (UseLightRadiusAsVolume) UpdateLightVolume();
        }

        public void TurnOn()
        {
            if (_Light == null) return;

            // Show light
            _Light.Show();

            // Show light volume (if we're not in the editor)
            if (!Engine.IsEditorHint()) _LightVolumeMesh.Show();

            // Turn on collision
            _LightArea.SetDeferred(nameof(_LightArea.Monitorable), true);
            _LightArea.SetDeferred(nameof(_LightArea.Monitoring), true);
            _LightShape.SetDeferred(nameof(_LightShape.Disabled), false);
        }

        public void TurnOff()
        {
            if (_Light == null) return;

            // Hide light
            _Light.Hide();

            // Hide light volume
            _LightVolumeMesh.Hide();

            // Turn off collision
            _LightArea.SetDeferred(nameof(_LightArea.Monitorable), false);
            _LightArea.SetDeferred(nameof(_LightArea.Monitoring), false);
            _LightShape.SetDeferred(nameof(_LightShape.Disabled), true);
        }

        public override void _Process(double delta)
        {
            // If playing, then modulate the light's energy by the noise
            if (_LightFlickerSfx?.Playing ?? false)
            {
                // Set the light energy to a random value
                LightEnergy = 1f - GD.Randf();
            }
        }

        public void FlickerLightOff()
        {
            _ = FlickerLightOffAsync();
        }

        public void FlickerLightOn()
        {
            _ = FlickerLightOnAsync();
        }

        public async Task FlickerLightOffAsync()
        {
            if (_LightFlickerSfx != null)
            {
                // Play SFX
                _LightFlickerSfx.Play();

                // Await sfx
                await ToSignal(_LightFlickerSfx, "finished");
            }

            // Set light energy back to 1
            LightEnergy = 1;

            // Turn self off
            _IsLit = false;
            TurnOff();

            EmitSignal(nameof(FlickeredOff));
        }

        public async Task FlickerLightOnAsync()
        {
            // Turn self off
            _IsLit = true;
            TurnOn();

            if (_LightFlickerSfx != null)
            {
                // Play SFX
                _LightFlickerSfx.Play();

                // Await sfx
                await ToSignal(_LightFlickerSfx, "finished");
            }

            // Set light energy back to 1
            LightEnergy = 1;

            EmitSignal(nameof(FlickeredOn));
        }
    }
}