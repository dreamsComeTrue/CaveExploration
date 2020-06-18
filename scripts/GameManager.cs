using Godot;
using System;

public class GameManager : Node
{
    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.F12 && @event.IsPressed())
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
    }
}
