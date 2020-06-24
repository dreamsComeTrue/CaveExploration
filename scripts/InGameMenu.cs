using Godot;
using System;

public class InGameMenu : Control
{
    public override void _Ready()
    {
        Visible = false;
    }

    public void ToggleVisibility()
    {
        AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        if (!Visible)
        {
            animationPlayer.Play("slide");
        }
        else
        {
            animationPlayer.PlayBackwards("slide");
        }
    }
}
