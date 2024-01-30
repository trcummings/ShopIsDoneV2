// AssetPlacementController.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable
using System.Collections.Generic;
using Godot;

namespace AssetPlacer;

[Tool]
public partial class AssetPlacementController : Node
{
    public bool Active { get => _active;
        set
        {
            var before = _active;
            _active = value;
            if(before != value) OnActiveChanged();
        }
    }

    protected virtual void OnActiveChanged()
    { }

    private bool _active;
    public float snappingGridSize;
    public const float DefaultSnappingGridSize = 1.0f;
    public const float SurfaceModeSnappingGridSide = .2f;
    protected PlacementUi placementUi;
    protected Snapping snapping;
    protected EditorInterface editorInterface;

    public virtual void Init(PlacementUi placementUi, Snapping snapping, EditorInterface editorInterface, Node _)
    {
        this.placementUi = placementUi;
        this.snapping = snapping;
        this.editorInterface = editorInterface;
        snapping.OffsetFromSelectedButtonPressed += () =>
        {
            if (Active) OffsetFromSelected(GetSelectedNode());
        };
        snapping.TranslateOffsetChanged += () => {
            if(Active) OnTranslateOffsetChanged();
        };
    }

    protected virtual void OnTranslateOffsetChanged() { }

    public virtual void OnSelectionChanged() { }
    
    protected Node3D GetSelectedNode()
    {
        var selectedNodes = editorInterface.GetSelection().GetSelectedNodes();
        if (selectedNodes.Count == 1 && selectedNodes[0] is Node3D node)
        {
            return node;
        }
        return null;
    }
    
    /**
     * returns the tooltip that should be shown on the mouse
     */
    public virtual string Process(Node sceneRoot, Camera3D viewportCamera, bool rmbPressed)
    {
        return null;
    }

    public virtual PlacementInfo GetPlacementPosition(Camera3D viewportCam, Vector2 viewportMousePosition, List<Node3D> placingNodes)
    {
        return new(PlacementPositionInfo.invalidInfo);
    }

    public virtual bool ProcessInput(AssetPlacerPlugin.InputEventType inputEventType, Vector2 viewportMousePos)
    {
        return false;
    }


    public virtual Vector3 SnapToPosition(PlacementPositionInfo info, float snapStep) { return Vector3.Zero; }
    public virtual void OffsetFromSelected(Node3D node) { }
    public virtual void RotateNode(Node3D node3D, Vector3 startRotation, float rotation) { }
}

#endif