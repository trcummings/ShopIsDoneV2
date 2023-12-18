using Godot;
//using ShopIsDone.Core;
//using Util.DirectoryTraverser;
using Godot.Collections;
//using GodotOnReady.Attributes;
//using System;
//using System.Linq;
//using ShopIsDone.Util.Positioning;

namespace Microgames
{
    public partial class MicrogamePayload : GodotObject
    {
        public Microgame Microgame;
        //public IOutcomeHandler[] Targets;
        //public IOutcomeHandler Source;
        //public Positions Position = Positions.Null;
        public Dictionary<string, Variant> Message = new Dictionary<string, Variant>();
    }

    //public partial class MicrogameSystem : ComponentSystem<MicrogameComponent>, IPersistable<ArenaData>
    //{
    //    [OnReadyGet(OrNull = true)]
    //    private MicrogameManager _MicrogameManager;

    //    public List<(string, PackedScene)> MicrogameScenes = new List<(string, PackedScene)>();
    //    public override Command Load(ComponentSystem<MicrogameComponent> data)
    //    {
    //        return new SeriesCommand(
    //            // Load in a list of all microgames by Id
    //            new ActionCommand(() => {
    //                var microgamePaths = new MicrogameTraverser().Traverse("res://scenes/Microgames/");
    //                MicrogameScenes.Clear();
    //                foreach (var path in microgamePaths)
    //                {
    //                    var scene = GD.Load<PackedScene>(path.FullPath);
    //                    MicrogameScenes.Add((path.FileNameNoExt, scene));
    //                }
    //            }),
    //            new DeferredCommand(() => base.Load(data))
    //        );
    //    }

    //    public Command RunMicrogame(Arena arena, MicrogamePayload payload)
    //    {
    //        return _MicrogameManager.RunMicrogame(payload, (Microgame.Outcomes outcome) =>
    //            new SeriesCommand(
    //                payload.Source.HandleOutcome(arena, outcome, payload.Targets, payload.Source, payload.Position),
    //                new DeferredCommand(() => new SeriesCommand(
    //                    payload.Targets.Select(t => t.HandleOutcome(arena, outcome, payload.Targets, payload.Source, payload.Position)).ToArray())
    //                )
    //            )
    //        );
    //    }

    //    protected override void InitComponent(MicrogameComponent component)
    //    {
    //        component.Init(this);
    //    }

    //    public void Save(ArenaData data)
    //    {
    //        data.MicrogameSystem = new MicrogameSystem()
    //        {
    //            _ComponentData = GetComponentSaveData()
    //        };
    //    }

    //    private class MicrogameTraverser : DirectoryTraverser
    //    {
    //        public override bool SatisfiesCriteria(PathData pathData)
    //        {
    //            return pathData.FileName.EndsWith("Microgame.tscn");
    //        }
    //    }
    //}
}