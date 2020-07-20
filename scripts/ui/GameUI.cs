using Godot;
using System;

public class GameUI : Control
{
	private InGameMenu inGameMenu;
	private NinePatchRect miniMapTexture;
	private Label countdownLabel;
	private Signals signals;

	public override void _Ready()
	{
		inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");
		miniMapTexture = GetNode<NinePatchRect>("CanvasLayer/MinimapTexture");
		countdownLabel = GetNode<Label>("CanvasLayer/Countdown/CountdownTimer");
		signals = (Signals)GetNode("/root/Signals");
		signals.Connect(nameof(Signals.PulseGameplayTimer), this, nameof(OnPulseGameplayTimer));
	}

	public override void _UnhandledKeyInput(InputEventKey @event)
	{
		if (@event.Pressed && (KeyList)@event.Scancode == KeyList.Escape)
		{
			inGameMenu.ToggleVisibility();
		}

		if (!@event.Pressed && (KeyList)@event.Scancode == KeyList.M)
		{
			miniMapTexture.Visible = !miniMapTexture.Visible;
		}
	}

	public void OnPulseGameplayTimer(float timeLeft)
	{
		string timeString;

		if (timeLeft < GameManager.timeThresholdWarning)
		{
			int millis = (int)Math.Ceiling((timeLeft % 1) * 100.0f);
			if (millis > 99)
			{
				millis = 0;
			}

			countdownLabel.Set("custom_colors/font_color", Colors.Orange);
			timeString = String.Format("{0:00}:{1:00}:{2:00}", Math.Floor(timeLeft / 60.0f), timeLeft % 60.0f, millis);
		}
		else
		{
			countdownLabel.Set("custom_colors/font_color", Colors.Cyan);
			timeString = String.Format("{0:00}:{1:00}", Math.Floor(timeLeft / 60.0f), timeLeft % 60.0f);
		}

		countdownLabel.Text = timeString;
	}
}
