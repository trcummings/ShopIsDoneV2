using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using System;

namespace ShopIsDone.Core
{
    public interface IComponent
    {
        LevelEntity Entity { get; set; }

        C GetComponent<C>() where C : IComponent;

        void Init();
    }

    public interface IUpdateOnActionComponent : IComponent
    {
        Command UpdateOnAction();
    }

    public interface IOnCleanupComponent : IComponent
    {
        Command OnCleanup();
    }

    public interface IHoverableComponent : IComponent
    {
        bool IsHoverableOnTile(Tile tile);
    }
}