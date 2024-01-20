using Godot;

// This is a singleton for events that can be called from anywhere in the
// game
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
    public delegate void LevelChangeRequestedEventHandler(PackedScene level);

    // Static function to help get the singleton
    public static Events GetEvents(Node node)
    {
        return node.GetNode<Events>("/root/Events");
    }
}
