using Godot;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Lighting
{
    // Base arena light class for lights + light volumes
    [Tool]
    public partial class WorldLight : Node3D
    {
        [Export]
        public bool UseLightRadiusAsVolume = true;

        [Export]
        public bool IsLit
        {
            get { return _Light?.Visible ?? false; }
            set {
                if (value) TurnOn();
                else TurnOff();
            }
        }

        [Export]
        public float Radius
        {
            get { return GetRadius(); }
            set { SetRadius(value); }
        }

        [Export]
        public float LightEnergy
        {
            get { return GetLightEnergy(); }
            set { SetLightEnergy(value); }
        }

        [Export]
        public float LightAngle
        {
            get { return GetLightAngle(); }
            set { SetLightAngle(value); }
        }

        [Export]
        public Color LightColor
        {
            get { return GetLightColor(); }
            set { SetLightColor(value); }
        }

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
            _LightFlickerSfx = GetNode<AudioStreamPlayer>("%LightFlickerSfxPlayer");

            // Hide light mesh if we're in the editor, otherwise show it
            if (Engine.IsEditorHint()) _LightVolumeMesh.Hide();
            else _LightVolumeMesh.Show();

            // Duplicate meshes
            if (UseLightRadiusAsVolume) DuplicateMeshes();

            // Set radius
            SetRadius(Radius);
        }

        protected virtual float GetRadius()
        {
            if (_Light is OmniLight3D omni) return omni.OmniRange;
            else if (_Light is SpotLight3D spot) return spot.SpotRange;
            return 0;
        }

        protected virtual void SetRadius(float radius)
        {
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

        protected float GetLightEnergy()
        {
            if (_Light == null) return 0;
            return _Light.LightEnergy;
        }

        protected void SetLightEnergy(float value)
        {
            if (_Light == null) return;
            _Light.LightEnergy = value;
        }

        protected Color GetLightColor()
        {
            return _Light?.LightColor ?? Colors.White;
        }

        protected void SetLightColor(Color value)
        {
            if (_Light == null) return;
            _Light.LightColor = value;
        }

        protected void DuplicateMeshes()
        {
            // Duplicate meshes
            _LightShape.Shape = (Shape3D)_LightShape.Shape.Duplicate();
            _LightVolumeMesh.Mesh = (Mesh)_LightVolumeMesh.Mesh.Duplicate();
        }

        private float GetLightAngle()
        {
            if (_Light is SpotLight3D spot) return spot.SpotAngle;
            return 0;
        }

        private void SetLightAngle(float value)
        {
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

            // Show light volume
            _LightVolumeMesh.Show();

            // Turn on collision
            _LightArea.Monitorable = true;
            _LightArea.Monitoring = true;
            _LightShape.Disabled = false;
        }

        public void TurnOff()
        {
            if (_Light == null) return;

            // Hide light
            _Light.Hide();

            // Hide light volume
            _LightVolumeMesh.Hide();

            // Turn off collision
            _LightArea.Monitorable = false;
            _LightArea.Monitoring = false;
            _LightShape.Disabled = true;
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

        public async Task FlickerLightOff()
        {
            // Play SFX
            _LightFlickerSfx.Play();

            // Await both
            await ToSignal(_LightFlickerSfx, "finished");

            // Set light energy back to 1
            LightEnergy = 1;

            // Turn self off
            TurnOff();
        }

        public async Task FlickerLightOn()
        {
            // Turn self off
            TurnOn();

            // Play SFX
            _LightFlickerSfx.Play();

            // Await both
            await ToSignal(_LightFlickerSfx, "finished");

            // Set light energy back to 1
            LightEnergy = 1;
        }
    }
}