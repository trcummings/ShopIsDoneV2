using Godot;
using System;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public static class Consts
    {
        public const string ACTION_KEY = "Action";

        public static class States
        {
            public const string IDLE = "Idle";
            public const string MENU = "Menu";
            public const string FACING_DIRECTION = "FacingDirection";
            public const string MOVE = "Move";
            public const string TARGETING = "Targeting";
        }
    }
}

