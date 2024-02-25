using Godot;
using ShopIsDone.Game;

namespace ShopIsDone.Debug
{
    public partial class SystemDisplay : Control
    {
        [Export]
        private Label _OS;

        [Export]
        private Label _Version;

        [Export]
        private Label _FPS;

        [Export]
        private Label _DrawCalls;

        [Export]
        private Label _VideoMemory;

        public override void _Ready()
        {
            _OS.Text = "OS: " + OS.GetName();
            _Version.Text = "Godot Version: " + Engine.GetVersionInfo()["string"];
        }

        public override void _Process(double delta)
        {
            // Ignore unless we're a debug build
            if (!GameManager.IsDebugMode()) return;

            _FPS.Text = "FPS: " + Performance.GetMonitor(Performance.Monitor.TimeFps);
            _DrawCalls.Text = "Draw Calls: " + Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
            _VideoMemory.Text = "Video Memory Used: " + Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
        }
    }
}