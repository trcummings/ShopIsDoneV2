using Godot;
using ShopIsDone.Demerits;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.UI
{
    public partial class PlayerPawnUI : Control
    {
        [Export]
        public Color DeselectedColor;

        // Nodes
        private TextureRect _Portrait;
        private TextureRect _PortraitBg;
        private Label _PawnNameLabel;
        private TextureRect _PortraitOutline;
        private ActionPointMeter _ActionPointMeter;
        private ApExcessUI _ApExcessUI;

        // Demerits
        private TextureRect _YellowSlip;
        private TextureRect _PinkSlip;

        // State
        public LevelEntity Pawn;

        public override void _Ready()
        {
            // Ready nodes
            _Portrait = GetNode<TextureRect>("%Portrait");
            _PortraitBg = GetNode<TextureRect>("%PortraitBackground");
            _PawnNameLabel = GetNode<Label>("%PawnName");
            _PortraitOutline = GetNode<TextureRect>("%CardBase");
            _ApExcessUI = GetNode<ApExcessUI>("%ApExcessContainer");
            _ActionPointMeter = GetNode<ActionPointMeter>("%ActionPointMeter");
            _YellowSlip = GetNode<TextureRect>("%YellowSlip");
            _PinkSlip = GetNode<TextureRect>("%PinkSlip");
        }

        public void Init(LevelEntity pawn)
        {
            // Set pawn
            Pawn = pawn;

            // Fill out fields with pawn data
            _PawnNameLabel.Text = pawn.EntityName;

            // Set Portrait
            SetPortrait();

            // Initially unselect
            SelectPawnUI(false);

            // Set Action Points
            SetActionPoints();

            // Set Demerits
            SetDemerits();
        }

        public void SelectPawnUI(bool val)
        {
            _PortraitOutline.Modulate = val ? Colors.White : DeselectedColor;
            _Portrait.Modulate = val ? Colors.White : DeselectedColor;
            _PortraitBg.Modulate = val ? Colors.White : DeselectedColor;
        }

        public void SetActionPoints()
        {
            var apHandler = Pawn.GetComponent<ActionPointHandler>();
            // Set action point render using the pawn's real action point data
            _ActionPointMeter.SetActionPoints(apHandler);
            _ApExcessUI.SetApExcess(apHandler.ActionPointExcess);
        }

        public void SetApDiff(ActionPointHandler apData)
        {
            // Clear the previous diff
            ClearApDiff();

            // If the pawn's current ap data is the same, don't do a diff
            var apHandler = Pawn.GetComponent<ActionPointHandler>();
            if (apData.ActionPoints == apHandler.ActionPoints &&
                apData.MaxActionPoints == apHandler.MaxActionPoints &&
                apData.ActionPointDebt == apHandler.ActionPointDebt &&
                apData.ActionPointExcess == apHandler.ActionPointExcess
            ) return;

            // Set the diff
            _ActionPointMeter.SetApDiff(apData);
            _ApExcessUI.SetApDiff(apData.ActionPointExcess);
        }

        public void ClearApDiff()
        {
            _ActionPointMeter.ClearApDiff();
            _ApExcessUI.ClearApDiff();
        }

        private void SetDemerits()
        {
            if (Pawn.GetComponent<DemeritHandler>().HasYellowSlip) _YellowSlip.Show();
            else _YellowSlip.Hide();

            if (Pawn.GetComponent<DemeritHandler>().HasPinkSlip) _PinkSlip.Show();
            else _PinkSlip.Hide();
        }

        private void SetPortrait()
        {
            // Get index by pawn Id
            var idx = 0;
            if (Pawn.Id == "haskell") idx = 1;
            else if (Pawn.Id == "ricky") idx = 2;
            else if (Pawn.Id == "jessica") idx = 3;

            (_Portrait.Texture as AtlasTexture).SetAtlasIdx(idx, 4);
        }
    }
}