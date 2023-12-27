using System;

namespace ShopIsDone.Utils.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
		// This is a marker attribute, so leave it empty
	}
}

