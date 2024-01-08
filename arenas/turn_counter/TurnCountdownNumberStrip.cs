using Godot;
using System;

namespace ShopIsDone.Arenas.Turns
{
    public partial class TurnCountdownNumberStrip : Node2D
    {
        private Sprite2D _TopNumber;
        private Sprite2D _BottomNumber;

        public override void _Ready()
        {
            _TopNumber = GetNode<Sprite2D>("%TopNumber");
            _BottomNumber = GetNode<Sprite2D>("%BottomNumber");
        }

        public void SetTopNumber(char c)
        {
            _TopNumber.Frame = charToFrame(c);
        }

        public void SetBottomNumber(char c)
        {
            _BottomNumber.Frame = charToFrame(c);
        }

        public char GetTopNumber()
        {
            return frameToChar(_TopNumber.Frame);
        }

        public char GetBottomNumber()
        {
            return frameToChar(_BottomNumber.Frame);
        }

        private int charToFrame(char c)
        {
            if (c == '0') return 0;
            if (c == '1') return 1;
            if (c == '2') return 2;
            if (c == '3') return 3;
            if (c == '4') return 4;
            if (c == '5') return 5;
            if (c == '6') return 6;
            if (c == '7') return 7;
            if (c == '8') return 8;
            if (c == '9') return 9;
            if (c == ' ') return 10;
            if (c == 'l') return 11;
            if (c == 'a') return 12;
            if (c == 's') return 13;
            if (c == 't') return 14;
            throw new ArgumentException($"{c} is an invalid input to charToFrame");
        }

        private char frameToChar(int frame)
        {
            if (frame == 0) return '0';
            if (frame == 1) return '1';
            if (frame == 2) return '2';
            if (frame == 3) return '3';
            if (frame == 4) return '4';
            if (frame == 5) return '5';
            if (frame == 6) return '6';
            if (frame == 7) return '7';
            if (frame == 8) return '8';
            if (frame == 9) return '9';
            if (frame == 10) return ' ';
            if (frame == 11) return 'l';
            if (frame == 12) return 'a';
            if (frame == 13) return 's';
            if (frame == 14) return 't';
            throw new ArgumentException($"{frame} is an invalid input to frameToChar");
        }
    }
}