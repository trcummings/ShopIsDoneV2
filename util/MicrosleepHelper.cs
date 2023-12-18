using System;
using Godot;

namespace Utils
{
    public partial class MicrosleepHelper : Node
    {
        [Signal]
        public delegate void MicrosleepFinishedEventHandler();

        [Export]
        public int MicrosleepDurationMs = 20;

        public void Microsleep()
        {
            OS.DelayMsec(MicrosleepDurationMs);
            EmitSignal(nameof(MicrosleepFinished));
        }
    }
}

