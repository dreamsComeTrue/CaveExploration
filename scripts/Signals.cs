using Godot;

public class Signals : Node
{
    [Signal]
    public delegate void InGameMenuVisibilityChanged(bool shown);

    [Signal]
    public delegate void PulseGameplayTimer(float timeLeft);

    [Signal]
    public delegate void MapGenerated();

    [Signal]
    public delegate void PlayerMoved(Vector2 newPosition);
}
