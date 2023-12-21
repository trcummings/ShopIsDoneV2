using System;
using Godot;

namespace ShopIsDone.Utils.Extensions
{
    public static class TabContainerExtensions
    {
        public static int GetTabIdxByTitle(this TabContainer tabContainer, string title)
        {
            var idx = -1;

            for (int i = 0; i < tabContainer.GetTabCount(); i++)
            {
                idx = i;
                // If it matches, exit the loop early, we have our id
                if (tabContainer.GetTabTitle(i) == title) break;
            }

            return idx;
        }
    }
}