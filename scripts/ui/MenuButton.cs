using Godot;
using System;

public class MenuButton : NinePatchRect
{
	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void _on_MenuButton_mouse_entered()
	{
		animationPlayer.Play("pulsate");
	}

	private void _on_MenuButton_mouse_exited()
	{
		animationPlayer.Stop(true);
		animationPlayer.Seek(0, true);
	}
}
