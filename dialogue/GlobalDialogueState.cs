using Godot;
using System;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.Dialogue
{
    // This is a global for setting dialogue values at runtime
    public partial class GlobalDialogueState : Node
    {
        private Dictionary<string, Variant> _DialogueState = new Dictionary<string, Variant>();

        // Static function to help get the singleton
        public static GlobalDialogueState GetGlobalDialogueState(Node node)
        {
            return node.GetNode<GlobalDialogueState>("/root/GlobalDialogueState");
        }

        public void SetDialogueState(string key, Variant value)
        {
            if (!_DialogueState.ContainsKey(key))
            {
                _DialogueState.Add(key, value);
            }
            else _DialogueState[key] = value;
        }

        public void RemoveDialogueState(string key)
        {
            if (_DialogueState.ContainsKey(key)) _DialogueState.Remove(key);
        }

        public Variant GetDialogueState(string key)
        {
            if (_DialogueState.ContainsKey(key)) return _DialogueState[key];
            return default;
        }

        public string Capitalize(string word)
        {
            return word.First().ToString().ToUpper() + word.Substring(1);
        }
    }
}