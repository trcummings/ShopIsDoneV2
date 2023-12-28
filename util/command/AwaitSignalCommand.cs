using System;
using Godot;

namespace ShopIsDone.Utils.Commands
{
	public partial class AwaitSignalCommand : Command
	{
		private readonly GodotObject _Obj;
		private readonly string _SignalName;
		private readonly string _MethodName;
		private readonly Variant[] _Args;

		public AwaitSignalCommand(GodotObject obj, string signalName, string method, params Variant[] args)
		{
			_Obj = obj;
			_SignalName = signalName;
			_MethodName = method;
			_Args = args;
        }

        public override void Execute()
        {
			// Connect to the method
            _Obj.Connect(_SignalName, new Callable(this, nameof(Finish)), (uint)ConnectFlags.OneShot);
			// Execute the method
			_Obj.Call(_MethodName, _Args);
        }
    }
}

