using Godot;
using ShopIsDone.Actions;
using System;
using System.Text.RegularExpressions;

namespace ShopIsDone.Arenas.UI
{
    public partial class ActionDescription : Control
    {
        private RichTextLabel _Description;
        private ArenaAction _Action;

        public override void _Ready()
        {
            base._Ready();
            _Description = GetNode<RichTextLabel>("%Description");
        }

        public void Init(ArenaAction action)
        {
            _Action = action;
            SetActionText();
        }

        public void UpdateDescription()
        {
            SetActionText();
        }

        private void SetActionText()
        {
            // Get description and replacement values
            var description = _Action.Description;
            var replacements = _Action.GetDescriptionVars();

            // Replace placeholders
            string resultText = Regex.Replace(description, @"\{(\w+)\}", match => {
                string key = match.Groups[1].Value;

                return (
                    replacements.TryGetValue(key, out var value)
                        ? value
                        : match.Value
                ).ToString();
            });

            // Set the text in the description
            _Description.Text = resultText;
        }
    }
}

