using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Utils
{
    // Data for return
    public partial class PathData : GodotObject
    {
        public string FileName;
        public string PathRoot;

        public string FileNameNoExt
        { get { return FileName.Split(".")[0]; } }

        public string Ext
        { get { return FileName.Split(".")[1]; } }

        public string FullPath
        { get { return System.IO.Path.Combine(PathRoot, FileName); } }

        public PathData FromFullPath(string fullPath)
        {
            var splitPath = new Stack<string>(fullPath.Split("/").ToList());
            var file = splitPath.Pop();
            var rootList = splitPath.ToList();
            rootList.Reverse();
            var root = string.Join("/", rootList.ToArray());
            return new PathData()
            {
                FileName = file,
                PathRoot = root
            };
        }
    }

    // This class uses Godot's Directory and File system to produce a list of files
    // that match some kind of criteria
    public partial class DirectoryTraverser : GodotObject
    {
        public bool SearchSubfolders = true;

        // State
        private List<PathData> _Paths = new List<PathData>();

        public List<PathData> Traverse(string folderName)
        {
            // Clear candidates
            _Paths.Clear();

            // If we have a filepath, open the folder
            if (!string.IsNullOrEmpty(folderName)) FindAllFiles(folderName);
            else GD.PrintErr("No pathway found at ", folderName);

            return _Paths;
        }

        public virtual bool SatisfiesCriteria(PathData pathData)
        {
            return false;
        }

        private void FindAllFiles(string path)
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

                // Create path data
                var pathData = new PathData()
                {
                    FileName = fileName,
                    PathRoot = pathRoot
                };

                // If our current file is a directory, recurse
                if (SearchSubfolders && dir.CurrentIsDir()) FindAllFiles(pathData.FullPath);

                // Otherwise, if we find a material, add it to the list
                else if (SatisfiesCriteria(pathData)) _Paths.Add(pathData);
            }

            // Close out the directory stream
            dir.ListDirEnd();
        }
    }
}

