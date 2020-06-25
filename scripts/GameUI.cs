using Godot;
using System;

public class GameUI : Control
{
    private InGameMenu inGameMenu;

    public override void _Ready()
    {
        inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.Escape && @event.Pressed)
        {
            inGameMenu.ToggleVisibility();
        }
    }

}
