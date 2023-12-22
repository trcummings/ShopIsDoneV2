using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace ShopIsDone.Utils
{
    public partial class ShaderCache : Node3D
    {
        [Signal]
        public delegate void FinishedCachingEventHandler();

        [Export]
        public Array<string> PathWhitelist = new Array<string>();

        // State
        private List<string> _MaterialFilePaths = new List<string>();
        private int _ProcessCount = 0;

        // Nodes
        private Node3D _Materials;

        public override void _Ready()
        {
            // Ready nodes
            _Materials = GetNode<Node3D>("%Materials");

            // Stop processing initially
            SetProcess(false);
        }

        public void RunCache()
        {
            // Clear existing list of materials and reset process count
            _MaterialFilePaths.Clear();
            _ProcessCount = 0;

            // Remove all spatial nodes underneath the materials node
            foreach (var node in _Materials.GetChildren().OfType<Node3D>())
            {
                _Materials.RemoveChild(node);
                node.QueueFree();
            }

            // Recursively find all materials in the codebase
            FindAllMaterials("res://");

            // Create a mesh for each material, position them, etc
            foreach (var filePath in _MaterialFilePaths) CreateMeshWith(filePath);

            // Start processing
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            // Process for 10 frames then stop processing
            _ProcessCount += 1;
            if (_ProcessCount > 10)
            {
                // Hide the node
                Visible = false;

                // Stop processing
                SetProcess(false);

                // Emit the finished signal
                EmitSignal(nameof(FinishedCaching));
            }
        }

        private void FindAllMaterials(string path)
        {
            // Create a directory object and set it to the current path
            var dir = DirAccess.Open(path);
            dir.ListDirBegin();

            // Get the root of the current path
            var pathRoot = dir.GetCurrentDir() + "/";

            // Keep running until we break
            while (true)
            {
                // Get filename of next item in the directory
                var fileName = dir.GetNext();

                // If the file name is empty, break
                if (fileName == "") break;

                // Create the full path from the root and the file name
                var fullPath = pathRoot + fileName;

                // If our current file is a directory, recurse
                if (dir.CurrentIsDir())
                {
                    FindAllMaterials(fullPath);
                }
                // Otherwise, if we find a material, add it to the list
                else if (fileName.EndsWith(".material"))
                {
                    _MaterialFilePaths.Add(fullPath);
                }
            }

            // Close out the directory stream
            dir.ListDirEnd();
        }

        private void CreateMeshWith(string filePath)
        {
            // Create a new mesh and mesh instance
            var mesh = new PlaneMesh();
            var meshInstance = new MeshInstance3D();

            // Add it to the materials node
            _Materials.AddChild(meshInstance);

            // Load in the material
            var material = GD.Load<Material>(filePath);

            // Set the parameters
            mesh.Size = Vector2.One;
            mesh.Material = material;
            meshInstance.Mesh = mesh;
            meshInstance.RotationDegrees = new Vector3(90, 0, 0);
            meshInstance.Name = filePath;
        }
    }
}