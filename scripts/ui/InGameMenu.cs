using Godot;

public class InGameMenu : Control
{
	private Signals signals;

	public override void _Ready()
	{
		Visible = false;

		signals = (Signals)GetNode("/root/Signals");
	}

	public void ToggleVisibility()
	{
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		if (!Visible)
		{
			animationPlayer.Play("slide");
			signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), true);
		}
		else
		{
			animationPlayer.PlayBackwards("slide");
			signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), false);
		}
	}
}
