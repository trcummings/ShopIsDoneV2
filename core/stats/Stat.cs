using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Core.Stats
{
    public partial class Stat : GodotObject
    {
        public float BaseValue { get; set; }

        private Array<Modifier> _Modifiers = new Array<Modifier>();

        public Stat(float baseValue)
        {
            BaseValue = baseValue;
        }

        public void AddModifier(Modifier modifier)
        {
            _Modifiers.Add(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            _Modifiers.Remove(modifier);
        }

        public float GetValue()
        {
            float finalValue = BaseValue;

            // Handle additive modifiers
            float additive = _Modifiers
                .Where(m => m.Type == Modifier.ModifierType.Additive)
                .Sum(m => m.Value);

            finalValue += additive;

            // Handle multiplicative modifiers
            float multiplicative = 1 + _Modifiers
                .Where(m => m.Type == Modifier.ModifierType.Multiplicative)
                .Sum(m => m.Value);

            finalValue *= multiplicative;

            return finalValue;
        }
    }
}

