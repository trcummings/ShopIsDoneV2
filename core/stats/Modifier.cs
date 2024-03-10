using System;
using Godot;

namespace ShopIsDone.Core.Stats
{
    public partial class Modifier : GodotObject
    {
        public enum ModifierType
        {
            Additive,
            Multiplicative
        }

        public float Value { get; private set; }

        public ModifierType Type { get; private set; }

        public Modifier(float value, ModifierType type = ModifierType.Additive)
        {
            Value = value;
            Type = type;
        }
    }
}

