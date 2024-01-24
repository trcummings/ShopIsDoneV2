using DialogueManagerRuntime;
using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Interactables.Interactions
{
	[Tool]
	public partial class DialogueResponseInteraction : Interaction
    {
        [Export]
        private string _DialogueStartFrom;

        [Export]
        private Resource _DialogueResource;

        [Export]
        private Interaction _OnYesInteraction;

        [Export]
        private Interaction _OnNoInteraction;

        private CanvasLayer _DialogueBalloon;
        private Callable _OnDialogueResponse;

        public override void _Ready()
        {
            base._Ready();
            _OnDialogueResponse = new Callable(this, nameof(OnDialogueResponse));
        }

        public override void Execute()
        {
            // Run dialogue
            _DialogueBalloon = DialogueManager.ShowExampleDialogueBalloon(_DialogueResource, _DialogueStartFrom);
            // Connect to response signal
            _DialogueBalloon.Connect("response_confirmed", _OnDialogueResponse, (uint)ConnectFlags.OneShot);
        }

        private void OnDialogueResponse(GodotObject response)
        {
            Interaction chosenInteraction;

            // Pull tags from response
            var tags = (Array<string>)response.Get("tags");

            // If it's a confirm response, run the on yes interaction
            if (tags.Contains("confirm")) chosenInteraction = _OnYesInteraction;
            // Otherwise, run the on no interaction
            else chosenInteraction = _OnNoInteraction;

            // Run
            chosenInteraction.Connect(
                nameof(_OnYesInteraction.Finished),
                Callable.From(Finish),
                (uint)ConnectFlags.OneShot
            );
            chosenInteraction.Execute();
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
            if (_OnYesInteraction == null || _OnNoInteraction == null)
            {
                return new string[] { "DialogueResponseInteraction requires both On Yes and On No Interactions!" };
            }
            return base._GetConfigurationWarnings();
        }
    }
}