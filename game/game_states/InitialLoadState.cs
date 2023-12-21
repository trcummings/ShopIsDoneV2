using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.GameSettings;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Game.States
{
    public partial class InitialLoadState : State
    {
        //[Export]
        //public NodePath ShaderCachePath;

        //private ShaderCache _ShaderCache;
        private GameSettingsManager _GameSettings;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            //_ShaderCache = GetNode<ShaderCache>(ShaderCachePath);
            _GameSettings = GameSettingsManager.GetGameSettings(this);
        }

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // TODO: Make sure the user save data folder exists

            // Settings
            LoadInitialGameSettings();

            //// Run shader cache
            //_ShaderCache.RunCache();
            //await ToSignal(_ShaderCache, nameof(ShaderCache.FinishedCaching));

            // If the game is in release mode (debug build or no), proceed to the
            // vanity card
            if (GameManager.IsRelease())
            {
                ChangeState(Consts.GameStates.VANITY_CARD);
                return;
            }

            // Otherwise, allow the override mode to apply
            // Pull override from message
            var initialState = (GameManager.InitialGameState)(int)message[Consts.OVERRIDE_GAME_STATE];
            switch (initialState)
            {
                case GameManager.InitialGameState.VanityCard:
                    {
                        ChangeState(Consts.GameStates.VANITY_CARD);
                        return;
                    }

                case GameManager.InitialGameState.MainMenu:
                    {
                        ChangeState(Consts.GameStates.MAIN_MENU);
                        return;
                    }

                case GameManager.InitialGameState.Level:
                    {
                        // Pull the initial level from the message
                        var initialLevel = message.GetValueOrDefault(Consts.INITIAL_LEVEL);
                        // If we don't have an arena data file path, error out,
                        // and go to the main menu instead
                        if (initialLevel.Equals(default(Variant)))
                        {
                            GD.PrintErr("Not given Level! Rerouting to Main Menu");
                            ChangeState(Consts.GameStates.MAIN_MENU);
                        }

                        // Otherwise, go directly to the given level
                        ChangeState(Consts.GameStates.LEVEL, new Dictionary<string, Variant>()
                        {
                            { Consts.INITIAL_LEVEL, initialLevel }
                        });
                        return;
                    }

                case GameManager.InitialGameState.BreakRoom:
                    {
                        // Go to the break room
                        ChangeState(Consts.GameStates.BREAK_ROOM);
                        return;
                    }
            }
        }

        private void LoadInitialGameSettings()
        {
            // Load in settings
            _GameSettings.Load();
            // Video settings
            _GameSettings.SetFullscreen(_GameSettings.GetFullscreen());
            _GameSettings.SetResolution(_GameSettings.GetResolution());
            _GameSettings.SetResolutionScaling(_GameSettings.GetResolutionScaling());
            _GameSettings.SetFxaa(_GameSettings.GetFxaa());
            _GameSettings.SetMsaa(_GameSettings.GetMsaa());
            _GameSettings.SetVsync(_GameSettings.GetVsync());
            // Audio Settings
            _GameSettings.SetMasterVolume(_GameSettings.GetMasterVolume());
            _GameSettings.SetSfxVolume(_GameSettings.GetSfxVolume());
            _GameSettings.SetMusicVolume(_GameSettings.GetMusicVolume());
            // Debug Settings
            _GameSettings.SetDebugDisplayVisible(_GameSettings.GetIsDebugDisplayVisible());
            _GameSettings.SetBlurDuringPause(_GameSettings.GetBlurDuringPause());
        }
    }
}