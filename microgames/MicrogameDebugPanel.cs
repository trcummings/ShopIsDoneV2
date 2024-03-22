using Godot;
using ShopIsDone.Utils;
using ShopIsDone.Utils.Extensions;
using System;

namespace ShopIsDone.Microgames
{
    public partial class MicrogameDebugPanel : Control
    {
        private Button _RunMicrogame;
        private OptionButton _Microgames;
        private MicrogameManager _Manager;

        [Export(PropertyHint.Dir)]
        private string _MicrogamesFolder;

        private bool _IsRunning = false;

        public override void _Ready()
        {
            _RunMicrogame = GetNode<Button>("%RunMicrogame");
            _Microgames = GetNode<OptionButton>("%Microgames");
            _RunMicrogame.Pressed += OnRunMicrogame;
        }

        public void Init(MicrogameManager manager)
        {
            _Manager = manager;

            // Initialize all microgames in list
            var microgames = new MicrogameTraverser().Traverse(_MicrogamesFolder);
            for (int i = 0; i < microgames.Count; i++)
            {
                var pathData = microgames[i];
                _Microgames.AddItem(pathData.FileNameNoExt, i);
                _Microgames.SetItemMetadata(i, pathData.FullPath);
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Selectively disable button
            _RunMicrogame.Disabled =
                _IsRunning ||
                _Microgames.Selected == -1 ||
                GetTree().Paused;
        }

        private async void OnRunMicrogame()
        {
            // Run the selected microgame
            var path = (string)_Microgames.GetSelectedMetadata();
            var microgameScene = GD.Load<PackedScene>(path);
            var microgame = microgameScene.Instantiate<Microgame>();

            // Temporarily pause the game
            _Manager.GetParent().ProcessMode = ProcessModeEnum.WhenPaused;
            GetTree().Paused = true;
            _IsRunning = true;

            await ToSignal(GetTree(), "process_frame");

            // Run microgame
            _Manager.RunMicrogame(microgame, new Godot.Collections.Dictionary<string, Variant>());
            await ToSignal(_Manager, nameof(_Manager.MicrogameFinished));

            // Unpause
            _Manager.GetParent().ProcessMode = ProcessModeEnum.Inherit;

            await ToSignal(GetTree(), "process_frame");

            GetTree().Paused = false;
            _IsRunning = false;
        }

        private partial class MicrogameTraverser : DirectoryTraverser
        {
            public override bool SatisfiesCriteria(PathData pathData)
            {
                return pathData.FileName.Contains("microgame.tscn");
            }
        }
    }
}

