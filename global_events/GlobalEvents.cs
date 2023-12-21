using Godot;

public partial class GlobalEvents : Node
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

    // Static function to help get the singleton
    public static GlobalEvents GetGlobalEvents(Node node)
    {
        return node.GetNode<GlobalEvents>("/root/GlobalEvents");
    }
}
