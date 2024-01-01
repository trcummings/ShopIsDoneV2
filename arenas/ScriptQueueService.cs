﻿using Godot;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.ArenaScripts
{
    public partial class ScriptQueueSystem : Node, IService
    {
        private Queue<ArenaScript> _PostActionCommands = new Queue<ArenaScript>();
        private bool _IsRunning = false;

        public void AddScriptToQueue(ArenaScript script)
        {
            _PostActionCommands.Enqueue(script);
        }

        public Command RunQueue()
        {
            return new ConditionalCommand(
                () => !_IsRunning,
                new SeriesCommand(
                    RunQueueHelper(_PostActionCommands),
                    // Afterwards, set running to false
                    new DeferredCommand(() => new ActionCommand(() => _IsRunning = false))
                )
            );
        }

        private Command RunQueueHelper(Queue<ArenaScript> scripts)
        {
            // If we've got any scripts, run them
            return new ConditionalCommand(
                () => scripts.Count > 0,
                // Get next action
                new DeferredCommand(() => {
                    // Pull next script from queue
                    var next = scripts.Dequeue();
                    // Init script
                    next.Init();
                    // Run
                    return new SeriesCommand(
                        // Run the action
                        next.ExecuteScript(),
                        // Then collect more queue items
                        new DeferredCommand(() => RunQueueHelper(scripts))
                    );
                })
            );
        }
    }
}