using System;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
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

        public override string[] _GetConfigurationWarnings()
        {
            if (_DialogueResource == null)
            {
                return new string[] { "Needs Dialogue resource!" };
            }
            else if (string.IsNullOrEmpty(_DialogueStartFrom))
            {
                return new string[] { "Needs title for Dialogue resource to start from!" };
            }
            if (!((Dictionary)_DialogueResource.Get("titles")).ContainsKey(_DialogueStartFrom))
            {
                return new string[] { $"No dialogue title {_DialogueStartFrom} given in Dialogue resource {_DialogueResource.ResourcePath}!" };
            }
            return base._GetConfigurationWarnings();
        }
    }
}

