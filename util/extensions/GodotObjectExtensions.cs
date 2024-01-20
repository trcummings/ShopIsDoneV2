using System;
using Godot;

namespace ShopIsDone.Utils.Extensions
{
	public static class GodotObjectExtensions
	{
        public static void SafeConnect(this GodotObject obj, string signalName, Callable callable, uint flags = 0)
        {
            if (!obj.IsConnected(signalName, callable)) obj.Connect(signalName, callable, flags);
        }

        public static void SafeDisconnect(this GodotObject obj, string signalName, Callable callable)
        {
            if (obj.IsConnected(signalName, callable))
            {
                obj.Disconnect(signalName, callable);
            }
        }
    }
}

