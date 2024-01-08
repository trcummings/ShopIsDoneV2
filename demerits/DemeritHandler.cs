using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Demerits
{
    // Component to handle the distribution and escalation of demerits
    public partial class DemeritHandler : NodeComponent
    {
        [Export]
        public bool HasYellowSlip = false;

        [Export]
        public bool HasPinkSlip = false;

        public Command SetDemeritStatus(bool yellowSlip, bool pinkSlip)
        {
            return new SetDemeritStatusCommand(this, yellowSlip, pinkSlip);
        }

        public Command ResetDemerits()
        {
            return new SetDemeritStatusCommand(this, false, false);
        }

        public Command EscalateDemeritStatus()
        {
            return new EscalateDemeritStatusCommand(this);
        }

        private partial class SetDemeritStatusCommand : Command
        {
            public SetDemeritStatusCommand(DemeritHandler demeritHandler, bool yellowSlip, bool pinkSlip)
            {
                DemeritHandler = demeritHandler;
                YellowSlip = yellowSlip;
                PinkSlip = pinkSlip;
            }

            // Parameters
            private DemeritHandler DemeritHandler;
            private bool YellowSlip;
            private bool PinkSlip;

            // Undo vars
            private bool _PrevYellowSlip;
            private bool _PrevPinkSlip;

            public override void Execute()
            {
                // Set undo vars
                _PrevYellowSlip = DemeritHandler.HasYellowSlip;
                _PrevPinkSlip = DemeritHandler.HasPinkSlip;

                // Set vars
                DemeritHandler.HasYellowSlip = YellowSlip;
                DemeritHandler.HasPinkSlip = PinkSlip;

                Finish();
            }
        }

        private partial class EscalateDemeritStatusCommand : Command
        {
            public DemeritHandler DemeritHandler;

            public EscalateDemeritStatusCommand(DemeritHandler demeritHandler)
            {
                DemeritHandler = demeritHandler;
            }

            // Undo vars
            private bool _PrevYellowSlip;
            private bool _PrevPinkSlip;

            public override void Execute()
            {
                // Set undo vars
                _PrevYellowSlip = DemeritHandler.HasYellowSlip;
                _PrevPinkSlip = DemeritHandler.HasPinkSlip;

                if (!DemeritHandler.HasYellowSlip)
                {
                    DemeritHandler.HasYellowSlip = true;
                }
                else if (DemeritHandler.HasYellowSlip && !DemeritHandler.HasPinkSlip)
                {
                    DemeritHandler.HasPinkSlip = true;
                }

                Finish();
            }
        }
    }
}

