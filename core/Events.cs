using Godot;
using ShopIsDone.Core.Data;

// This is a singleton for events that can be called from anywhere in the
// game
[Tool]
[GlobalClass]
public partial class Events : Node
{
    // Fade to black
    [Signal]
	public delegate void FadeInRequestedEventHandler();

    [Signal]
    public delegate void FadeInFinishedEventHandler();

    [Signal]
    public delegate void FadeOutRequestedEventHandler();

    [Signal]
    public delegate void FadeOutFinishedEventHandler();

    // Environment change
    [Signal]
    public delegate void ChangeEnvironmentRequestedEventHandler(Environment environment);

    // Quit game
    [Signal]
    public delegate void QuitGameRequestedEventHandler();

    // Change level
    [Signal]
    public delegate void LevelChangeRequestedEventHandler(string levelId);

    // Static function to help get the singleton
    public static Events GetEvents(Node node)
    {
        return node.GetNode<Events>("/root/Events");
    }

    // API methods
    public void RequestLevelChange(string levelId)
    {
        EmitSignal(nameof(LevelChangeRequested), levelId);
    }
}
