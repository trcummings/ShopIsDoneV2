// RightClickPopupSubmenu.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.
#if TOOLS
#nullable disable

using Godot;

namespace AssetPlacer;

[Tool]
public partial class RightClickPopupSubmenu : PopupMenu
{
    [Signal]
    public delegate void SubmenuSelectedEventHandler(long parentId, long id);
    
    public long parentId;
    
    public override void _Ready()
    {
        IdPressed += (id) => EmitSignal(SignalName.SubmenuSelected, parentId, id);
    }
}
#endif