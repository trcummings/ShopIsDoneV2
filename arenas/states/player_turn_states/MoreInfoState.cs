using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Arenas.UI;
using Godot.Collections;
using ShopIsDone.Game;
using ShopIsDone.Pausing;
using ShopIsDone.GameSettings;
using ShopIsDone.Tiles;

namespace ShopIsDone.Arenas.PlayerTurn
{
    public partial class MoreInfoState : State
    {
        [Signal]
        public delegate void CanceledEventHandler();

        [Export]
        public MoreInfoUIContainer _MoreInfoContainer;

        [Export]
        private BlurBackground _BlurBackground;

        private GameSettingsManager _GameSettings;
        private Tile _LastSelectedTile;

        public override void _Ready()
        {
            // Game settings
            _GameSettings = GameSettingsManager.GetGameSettings(this);
            _GameSettings.BlurDuringPauseChanged += OnBlurDuringPauseChanged;

            // Show or hide the blur background (only hide in debug mode)
            _BlurBackground.Visible = GameManager.IsDebugMode()
                ? _GameSettings.GetBlurDuringPause()
                : true;
        }

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            var payload = (MoreInfoPayload)message?[Consts.MORE_INFO_PAYLOAD_KEY]
                ?? new MoreInfoPayload();
            _LastSelectedTile = (Tile)message?[Consts.LAST_SELECTED_TILE_KEY];
            _MoreInfoContainer.Init(payload);
            _MoreInfoContainer.Show();
            _BlurBackground.FadeIn();

            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // On cancel, go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                EmitSignal(nameof(Canceled));

                ChangeState(Consts.States.CHOOSING_UNIT, new Dictionary<string, Variant>()
                {
                    { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
                });

                return;
            }
        }

        public override void OnExit(string nextState)
        {
            _MoreInfoContainer.Hide();
            _MoreInfoContainer.Reset();
            _BlurBackground.FadeOut();

            base.OnExit(nextState);
        }

        private void OnBlurDuringPauseChanged(bool newValue)
        {
            // Only hide in debug mode
            _BlurBackground.Visible = GameManager.IsDebugMode()
                ? newValue
                : true;
        }
    }
}