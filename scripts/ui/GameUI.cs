using Godot;
using System;

public class GameUI : Control
{
    private InGameMenu inGameMenu;
    private TextureRect flashLightTexture;
    private NinePatchRect miniMapTexture;
    private Label countdownLabel;
    private Signals signals;

    private float flashLightOffset = 0.0f;

    public override void _Ready()
    {
        inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");
        miniMapTexture = GetNode<NinePatchRect>("CanvasLayer/MinimapTexture");
        flashLightTexture = GetNode<TextureRect>("CanvasLayer/FlashLightTextureFrame/FlashLightTexture");
        countdownLabel = GetNode<Label>("CanvasLayer/Countdown/CountdownTimer");
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.PulseGameplayTimer), this, nameof(OnPulseGameplayTimer));
        signals.Connect(nameof(Signals.LightBarsChanged), this, nameof(OnUpdateFlashLightBars));
        signals.Connect(nameof(Signals.FlashLightToggled), this, nameof(OnFlashLightToggled));
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

    public void OnUpdateFlashLightBars(int barsLeft)
    {
        float sizeX = flashLightTexture.RectSize.x - 7;
        float barSize = (sizeX / 24) / sizeX;

        flashLightOffset = barSize * (24.0f - barsLeft);

        ShaderMaterial shaderMaterial = flashLightTexture.Material as ShaderMaterial;
        shaderMaterial.SetShaderParam("discard_offset", flashLightOffset);
    }
	
	public void OnFlashLightToggled(bool visible)
	{
		AtlasTexture atlas = GetNode<TextureRect>("CanvasLayer/FlashLight").Texture as AtlasTexture;
		Vector2 position = visible ? Vector2.Zero : new Vector2(0.0f, 128.0f);
		atlas.Region = new Rect2(position, atlas.Region.Size);
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
