using Godot;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Widgets
{
	public partial class DemeritWidget : Node3D
	{
        [Signal]
        public delegate void DemeritBeganEventHandler();

        [Signal]
        public delegate void DemeritSlappedEventHandler();

        [Signal]
        public delegate void DemeritFinishedEventHandler();

        public enum DemeritType
        {
            YellowSlip,
            PinkSlip
        }

        [Export]
        public Texture2D YellowSlipTexture;

        [Export]
        public Texture2D PinkSlipTexture;

        // Nodes
        private MeshInstance3D _Slip;
        private AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            _Slip = GetNode<MeshInstance3D>("%Slip");
            _AnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
        }

        public void GrantDemerit(Vector3 pos, DemeritType demeritType)
        {
            _ = GrantDemeritAsync(pos, demeritType);
        }

        public async Task GrantDemeritAsync(Vector3 pos, DemeritType demeritType)
        {
            // Position demerit (Higher than our rule breaker)
            GlobalPosition = pos + Vector3.Up;

            // Set demerit type
            SetDemeritType(demeritType);

            // Emit signal
            EmitSignal(nameof(DemeritBegan));

            // Play animation
            _AnimPlayer.Play("default");

            // Show demerit
            Show();

            // Wait for animation to finish
            await ToSignal(_AnimPlayer, "animation_finished");

            // Emit signal
            EmitSignal(nameof(DemeritSlapped));

            // Set to billboard mode
            var material = (StandardMaterial3D)_Slip.GetActiveMaterial(0);
            material.BillboardMode = BaseMaterial3D.BillboardModeEnum.FixedY;

            // TODO: Play dissolve
            await ToSignal(GetTree().CreateTimer(1.0F), "timeout");

            // Unset billboard mode
            material.BillboardMode = BaseMaterial3D.BillboardModeEnum.Disabled;

            // Hide demerit
            Hide();

            EmitSignal(nameof(DemeritFinished));
        }

        private void SetDemeritType(DemeritType demeritType)
        {
            var material = (StandardMaterial3D)_Slip.GetActiveMaterial(0);
            material.AlbedoTexture = GetDemeritTexture(demeritType);
        }

        private Texture2D GetDemeritTexture(DemeritType demeritType)
        {
            if (demeritType == DemeritType.YellowSlip) return YellowSlipTexture;
            else if (demeritType == DemeritType.PinkSlip) return PinkSlipTexture;

            throw new NotImplementedException("No Texture for given demeritType!");
        }
    }
}
