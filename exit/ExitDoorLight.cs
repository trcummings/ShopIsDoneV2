using Godot;
using System;

namespace ShopIsDone.Exit
{
    [Tool]
    public partial class ExitDoorLight : Node3D
    {
        [Export]
        public Material LightLockedMaterial;

        [Export]
        public Material LightOpenMaterial;

        private CsgCylinder3D _Light;

        public override void _Ready()
        {
            _Light = GetNode<CsgCylinder3D>("%Light");
        }

        public void UnlockDoor()
        {
            _Light.Material = LightOpenMaterial;
        }

        public void LockDoor()
        {
            _Light.Material = LightLockedMaterial;
        }
    }
}
