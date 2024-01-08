using System;
using Godot;

namespace ShopIsDone.Utils.Extensions
{
	public static class TextureExtensions
	{
        public static void SetAtlasIdx(this AtlasTexture atlasTexture, int idx, int cols)
        {
            var safeCols = Mathf.Max(cols, 1);
            // Calculate the x and y indices within the atlas texture
            var regionSize = atlasTexture.Region.Size;
            var xIdx = idx % safeCols;
            var yIdx = idx / safeCols;

            // Set the x and y region value for image offset
            atlasTexture.Region = new Rect2(
                xIdx * regionSize.X,
                yIdx * regionSize.Y,
                atlasTexture.Region.Size
            );
        }
    }
}

