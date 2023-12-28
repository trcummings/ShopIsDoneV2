using Godot;
using System;
using ShopIsDone.ActionPoints;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.UI
{
    public partial class ActionPointMeter : Control
    {
        [Export]
        public PackedScene ActionPointBulbScene;

        private Control _BaseContainer;
        private Control _DiffContainer;

        public override void _Ready()
        {
            // Ready nodes
            _BaseContainer = GetNode<Control>("%BaseContainer");
            _DiffContainer = GetNode<Control>("%DiffContainer");
        }

        public void SetActionPoints(ActionPointHandler apData)
        {
            SetApInContainer(_BaseContainer, apData);
        }

        public void SetApDiff(ActionPointHandler apData)
        {
            ClearApDiff();
            SetApInContainer(_DiffContainer, apData);
        }

        public void ClearApDiff()
        {
            // Clear out diff
            _DiffContainer.RemoveChildrenOfType<ActionPointBulb>((_node, child) => { child.Hide(); });
        }

        private void SetApInContainer(Control container, ActionPointHandler apData)
        {
            // Clear out previous action points
            container.RemoveChildrenOfType<ActionPointBulb>((_node, child) => { child.Hide(); });

            // Loop over max action points and create each bulb node
            for (int i = 0; i < apData.MaxActionPoints; i++)
            {
                // Create the node
                var bulb = ActionPointBulbScene.Instantiate<ActionPointBulb>();

                // Add it to the container
                container.AddChild(bulb);

                // First add the debt
                if (i < apData.ActionPointDebt)
                {
                    bulb.SetBulbState(ActionPointBulb.BulbStates.Debt);
                }
                // Then add the existing points
                else if (i < apData.ActionPointDebt + apData.ActionPoints)
                {
                    bulb.SetBulbState(ActionPointBulb.BulbStates.Lit);
                }
                // Finally the empty ones
                else
                {
                    bulb.SetBulbState(ActionPointBulb.BulbStates.BurnedOut);
                }
            }
        }
    }
}

