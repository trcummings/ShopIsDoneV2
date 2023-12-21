using System;
using Godot;

namespace ShopIsDone.Cameras
{
	public interface IIsometricViewable
	{
        void SetViewedDir(Vector3 viewedDir);
    }
}

