﻿using System;
using ShopIsDone.Cameras;
using Godot;
using ShopIsDone.Arenas.ArenaScripts;

namespace ShopIsDone.Arenas
{
    public partial class ArenaEvents : Resource
    {
        [Signal]
        public delegate void ScreenshakeRequestedEventHandler(ScreenshakeHandler.ShakePayload payload);

        [Signal]
        public delegate void ArenaCommandRequestedEventHandler(ArenaScript command);

        public void RequestScreenshake(ScreenshakeHandler.ShakePayload payload)
        {
            EmitSignal(nameof(ScreenshakeRequested), payload);
        }

        public void AddArenaCommandToQueue(ArenaScript command)
        {
            EmitSignal(nameof(ArenaCommandRequested), command);
        }
    }
}

