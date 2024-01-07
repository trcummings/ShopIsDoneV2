using System;
using DialogueManagerRuntime;
using Godot;

namespace ShopIsDone.Interactables.Interactions
{
    public partial class DialogueInteraction : Interaction
    {
        [Export]
        private string _DialogueStartFrom;

        [Export]
        private Resource _DialogueResource;

        public override void Execute()
        {
            // Oneshot connect to dialogue finished event
            DialogueManager.DialogueEnded += OnDialogueFinished;

            // Run dialogue
            DialogueManager.ShowExampleDialogueBalloon(_DialogueResource, _DialogueStartFrom);
        }

        private void OnDialogueFinished(Resource _)
        {
            // Disconnect from dialogue finished event
            DialogueManager.DialogueEnded -= OnDialogueFinished;

            // Emit finished
            Finish();
        }
    }
}

