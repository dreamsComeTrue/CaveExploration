using Godot;
using System;

public class GameUI : Control
{
    private InGameMenu inGameMenu;
    private Signals signals;

    public override void _Ready()
    {
        inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.PulseGameplayTimer), this, nameof(OnPulseGameplayTimer));
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.Escape && @event.Pressed)
        {
            inGameMenu.ToggleVisibility();
        }
    }

    public void OnPulseGameplayTimer(float timeLeft)
    {
		Label countdownLabel = GetNode<Label>("CanvasLayer/CountdownTimer");
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
		
        GetNode<Label>("CanvasLayer/CountdownTimer").Text = timeString;
    }
}