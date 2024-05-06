using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.GameSettings;
using ShopIsDone.Utils;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Game.States
{
    public partial class InitialLoadState : State
    {
        [Export]
        private ShaderCache _ShaderCache;

        private GameSettingsManager _GameSettings;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _GameSettings = GameSettingsManager.GetGameSettings(this);
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // TODO: Make sure the user save data folder exists

            // Load in settings
            _GameSettings.Load();
            // Initialize settings
            _GameSettings.InitValues();

            // Run shader cache
            _ShaderCache.RunCache();
            await ToSignal(_ShaderCache, nameof(ShaderCache.FinishedCaching));

            // If the game is a standalone build and not in debug mode, proceed
            // to the vanity card
            if (GameManager.IsRelease() && !GameManager.IsDebugMode())
            {
                ChangeState(Consts.GameStates.VANITY_CARD);
                return;
            }

            // Otherwise, allow the override mode to apply
            // Pull override from message
            var initialState = (GameManager.InitialGameStates)(int)message[Consts.OVERRIDE_GAME_STATE];
            switch (initialState)
            {
                case GameManager.InitialGameStates.VanityCard:
                    {
                        ChangeState(Consts.GameStates.VANITY_CARD);
                        return;
                    }

                case GameManager.InitialGameStates.MainMenu:
                    {
                        ChangeState(Consts.GameStates.MAIN_MENU);
                        return;
                    }

                case GameManager.InitialGameStates.Level:
                    {
                        // Pull the initial level from the message
                        var initialLevel = (string)message.GetValueOrDefault(Consts.INITIAL_LEVEL);
                        // If we don't have an arena data file path, error out,
                        // and go to the main menu instead
                        if (string.IsNullOrEmpty(initialLevel))
                        {
                            GD.PrintErr("Not given initial level id! Rerouting to Main Menu");
                            ChangeState(Consts.GameStates.MAIN_MENU);
                        }

                        // Otherwise, go directly to the given level
                        ChangeState(Consts.GameStates.LEVEL, new Dictionary<string, Variant>()
                        {
                            { Consts.LEVEL_KEY, initialLevel }
                        });
                        return;
                    }
            }
        }
    }
}