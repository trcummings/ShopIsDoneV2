using Godot;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Widgets
{
    public partial class FacingWidget : Node3D
    {
        // Nodes
        private Sprite3D _RightSphere;
        private Sprite3D _LeftSphere;
        private Sprite3D _ForwardSphere;
        private Sprite3D _BackSphere;
        private List<Sprite3D> _Spheres;

        public override void _Ready()
        {
            // Ready nodes
            _RightSphere = GetNode<Sprite3D>("%RightSphere");
            _LeftSphere = GetNode<Sprite3D>("%LeftSphere");
            _ForwardSphere = GetNode<Sprite3D>("%ForwardSphere");
            _BackSphere = GetNode<Sprite3D>("%BackSphere");
            _Spheres = new List<Sprite3D>()
            {
                _RightSphere,
                _LeftSphere,
                _ForwardSphere,
                _BackSphere
            };
        }

        public void WarpToLocation(Vector3 location)
        {
            // Set location to 2 units above given location
            GlobalTransform = GlobalTransform with {
                Origin = location + (2 * Vector3.Up)
            };
        }

        public void SelectFacingDir(Vector3 dir)
        {
            // Select the appropriate sphere
            Sprite3D selectedSphere = null;
            if (dir == Vector3.Left)
            {
                selectedSphere = _LeftSphere;
            }
            else if (dir == Vector3.Right)
            {
                selectedSphere = _RightSphere;
            }
            else if (dir == Vector3.Forward)
            {
                selectedSphere = _ForwardSphere;
            }
            else if (dir == Vector3.Back)
            {
                selectedSphere = _BackSphere;
            }

            // Update all spheres
            foreach (var sphere in _Spheres)
            {
                if (sphere == selectedSphere)
                {
                    // Active frame
                    sphere.Frame = 1;
                }
                else
                {
                    // Inactive frame
                    sphere.Frame = 0;
                }
            }
        }
    }
}