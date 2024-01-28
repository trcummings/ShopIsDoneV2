using Godot;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Lighting.Silhouettes
{
    public partial class SilhouetteSystem : Node, IService
    {
        [Export]
        public int MaxFadeTurns = 3;

        public void Update()
        {

        }
    }
}
