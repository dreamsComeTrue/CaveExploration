using Godot;
using System;

public class GameUI : Control
{
    private InGameMenu inGameMenu;
    private TextureRect flashLightTexture;
    private NinePatchRect miniMapTexture;
    private Label countdownLabel;
    private InventoryItem inventoryItem1;
    private InventoryItem inventoryItem2;

    private InventoryItem focusedInventoryItem;

    private Signals signals;

    private MessageNotifier messageNotifier;

    private Control overlays;
    private Control uiElements;

    private NinePatchRect keysInfoFrame;

    private AudioManager audioManager;

    public bool enabled = false;

    private float flashLightOffset = 0.0f;

    public override void _Ready()
    {
        inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");
        miniMapTexture = GetNode<NinePatchRect>("CanvasLayer/UIElements/MinimapTexture");
        flashLightTexture = GetNode<TextureRect>("CanvasLayer/UIElements/FlashLightTextureFrame/FlashLightTexture");
        countdownLabel = GetNode<Label>("CanvasLayer/UIElements/Countdown/CountdownTimer");
        inventoryItem1 = GetNode<InventoryItem>("CanvasLayer/UIElements/InventoryItem1");
        inventoryItem2 = GetNode<InventoryItem>("CanvasLayer/UIElements/InventoryItem2");
        keysInfoFrame = GetNode<NinePatchRect>("CanvasLayer/UIElements/KeysInfoRect");
        uiElements = GetNode<Control>("CanvasLayer/UIElements");

        overlays = GetTree().Root.GetNode<Control>("Gameplay/GameUI/CanvasLayer/Overlays");

        messageNotifier = GetNode<MessageNotifier>("CanvasLayer/MessageNotifier");

        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.PulseGameplayTimer), this, nameof(OnPulseGameplayTimer));
        signals.Connect(nameof(Signals.LightBarsChanged), this, nameof(OnLightBarsChanged));
        signals.Connect(nameof(Signals.FlashLightToggled), this, nameof(OnFlashLightToggled));
        signals.Connect(nameof(Signals.InGameMenuVisibilityChanged), this, nameof(OnInGameMenuVisibilityChanged));
        signals.Connect(nameof(Signals.SoundsMuted), this, nameof(OnSoundsMuted));
        signals.Connect(nameof(Signals.MusicMuted), this, nameof(OnMusicMuted));

        audioManager = (AudioManager)GetNode("/root/AudioManager");
    }

    public override void _ExitTree()
    {
        signals.Disconnect(nameof(Signals.PulseGameplayTimer), this, nameof(OnPulseGameplayTimer));
        signals.Disconnect(nameof(Signals.LightBarsChanged), this, nameof(OnLightBarsChanged));
        signals.Disconnect(nameof(Signals.FlashLightToggled), this, nameof(OnFlashLightToggled));
        signals.Disconnect(nameof(Signals.InGameMenuVisibilityChanged), this, nameof(OnInGameMenuVisibilityChanged));
        signals.Disconnect(nameof(Signals.SoundsMuted), this, nameof(OnSoundsMuted));
        signals.Disconnect(nameof(Signals.MusicMuted), this, nameof(OnMusicMuted));
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (@event.Pressed)
        {
            KeyList key = (KeyList)@event.Scancode;

            switch (key)
            {
                case KeyList.Escape:
                    audioManager.PlayMenuRolloutSound();

                    bool shown = inGameMenu.ToggleVisibility();
                    OnInGameMenuVisibilityChanged(shown);
                    break;

                case KeyList.M:
                    miniMapTexture.Visible = !miniMapTexture.Visible;
                    audioManager.PlayToggleSound();
                    break;

                case KeyList.I:
                    keysInfoFrame.Visible = !keysInfoFrame.Visible;
                    audioManager.PlayToggleSound();
                    break;

                case KeyList.Key1:
                    FocusInventoryItem(inventoryItem1);
                    audioManager.PlayToggleSound();
                    break;

                case KeyList.Key2:
                    FocusInventoryItem(inventoryItem2);
                    audioManager.PlayToggleSound();
                    break;
            }
        }
    }

    private void OnInGameMenuVisibilityChanged(bool visible)
    {
        AnimationPlayer animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        if (visible)
        {
            animPlayer.Play("fade");
        }
        else
        {
            animPlayer.PlayBackwards("fade");
        }
    }

    private void FocusInventoryItem(InventoryItem item)
    {
        if (focusedInventoryItem == item)
        {
            focusedInventoryItem = null;
            signals.EmitSignal(nameof(Signals.FocusInventoryItem), new object[] { focusedInventoryItem });
        }
        else
        {
            signals.EmitSignal(nameof(Signals.FocusInventoryItem), item);
            focusedInventoryItem = item;
        }
    }

    public void OnLightBarsChanged(int barsLeft)
    {
        float sizeX = flashLightTexture.RectSize.x - 7;
        float barSize = (sizeX / 24) / sizeX;

        flashLightOffset = barSize * (24.0f - barsLeft);

        ShaderMaterial shaderMaterial = flashLightTexture.Material as ShaderMaterial;
        shaderMaterial.SetShaderParam("discard_offset", flashLightOffset);
    }

    public void OnFlashLightToggled(bool visible)
    {
        AtlasTexture atlas = GetNode<TextureRect>("CanvasLayer/UIElements/FlashLight").Texture as AtlasTexture;
        Vector2 position = visible ? Vector2.Zero : new Vector2(0.0f, 128.0f);
        atlas.Region = new Rect2(position, atlas.Region.Size);

        overlays.Visible = visible;
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

    private void OnSoundsMuted(bool muted)
    {
        if (muted)
        {
            messageNotifier.AddMessage("Sounds disabled...");
        }
        else
        {
            messageNotifier.AddMessage("Sounds enabled...");
        }
    }

    private void OnMusicMuted(bool muted)
    {
        if (muted)
        {
            messageNotifier.AddMessage("Music disabled...");
        }
        else
        {
            messageNotifier.AddMessage("Music enabled...");
        }
    }
}
