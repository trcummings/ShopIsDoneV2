using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Utils.Extensions
{
    public static class NodeExtensions
    {
        public static void RemoveChildrenOfType<TChild>(this Node node, Action<Node, TChild> onCleanup = null)
            where TChild : Node
        {
            foreach (var child in node.GetChildren().OfType<TChild>())
            {
                // Run the cleanup function
                if (onCleanup != null) onCleanup?.Invoke(node, child);

                // Remove the child and queue it for deletion
                node.RemoveChild(child);
                child.QueueFree();
            }
        }

        public static void RecurseChildrenOfType<TChild>(this Node node, Action<Node, TChild> OnRecurse = null)
            where TChild : Node
        {
            foreach (Node child in node.GetChildren())
            {
                child.RecurseChildrenOfType(OnRecurse);
                if (child is TChild childOfType) OnRecurse?.Invoke(node, childOfType);
            }
        }

        // Searches for a node in the children of the given node that is in the
        // requested group and returns the first one found
        public static Node FindChildInGroup(this Node node, string group)
        {
            // Base Case
            if (node.IsInGroup(group)) return node;

            // Induction Step
            foreach (Node child in node.GetChildren())
            {
                var result = child.FindChildInGroup(group);
                if (result != null) return result;
            }

            // Failure Case
            return null;
        }

        // Same as above but for all children + the parent (if they are in the group)
        public static List<Node> FindChildrenInGroup(this Node node, string group)
        {
            // Induction step
            if (!node.IsInGroup(group))
            {
                return node
                    .GetChildren()
                    .OfType<Node>()
                    .Aggregate(new List<Node>(), (acc, nextNode) =>
                    {
                        acc.AddRange(nextNode.FindChildrenInGroup(group));
                        return acc;
                    });
            }

            // Base case
            return new List<Node>() { node };
        }
    }
}

