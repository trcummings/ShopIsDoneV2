using Godot;
using System;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public static class Consts
    {
        public const string ACTION_KEY = "Action";
        public const string INTERACTION_KEY = "Interaction";

        public static class States
        {
            public const string IDLE = "Idle";
            public const string MENU = "Menu";
            public const string FACING_DIRECTION = "FacingDirection";
            public const string MOVE = "Move";
            public const string TARGETING = "Targeting";
            public const string INTERACT = "Interact";
        }
    }
}

