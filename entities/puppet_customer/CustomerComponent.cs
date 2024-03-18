using System;
using ShopIsDone.Core;
using Godot;

namespace ShopIsDone.Entities.PuppetCustomers
{
	// This is an empty "marker" component to show that an entity is a
	// customer, it may have functionality in the future
	public partial class CustomerComponent : NodeComponent
    {
		[Export(PropertyHint.MultilineText)]
		public string Description = "";
	}
}

