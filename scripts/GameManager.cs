using Godot;
using System;

public class GameManager : Node
{
    private Signals signals;

    public const float timeThresholdWarning = 10.0f;

    public float timeLeft = 3 * 60.0f;
    private float timeAccumulator = 0.0f;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
    }

    public override void _Process(float delta)
    {
        timeAccumulator += delta;

        if (timeLeft <= timeThresholdWarning)
        {
            if (timeAccumulator >= 0.05f)
            {
                timeAccumulator = 0.0f;
                timeLeft -= 0.05f;
                signals.EmitSignal(nameof(Signals.PulseGameplayTimer), timeLeft);
            }
        }
        else if (timeAccumulator >= 1.0f)
        {
            timeAccumulator = 0.0f;
            timeLeft -= 1.0f;
            signals.EmitSignal(nameof(Signals.PulseGameplayTimer), timeLeft);
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.F12 && @event.IsPressed())
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
    }
}
