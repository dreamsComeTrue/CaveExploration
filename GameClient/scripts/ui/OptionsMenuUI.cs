using Godot;

public class OptionsMenuUI : Control
{
    private Signals signals;
    private GameSettings gameSettings;
    private AudioManager audioManager;

    private MenuButton buttonMusic;
    private MenuButton buttonSounds;
    private MenuButton buttonCameraLag;
    private MenuButton buttonBack;

    private MenuButton selectedButton;

    private bool isConnected = false;

    public bool isShown = false;

    private bool isHiding = false;

    public override void _Ready()
    {
        buttonMusic = GetNode<MenuButton>("UIFrame/VBoxContainer/MusicButtonHBoxContainer/MusicMenuButton");
        buttonSounds = GetNode<MenuButton>("UIFrame/VBoxContainer/SoundsButtonHBoxContainer/SoundsMenuButton");
        buttonCameraLag = GetNode<MenuButton>("UIFrame/VBoxContainer/CameraLagHBoxContainer/CameraLagMenuButton");
        buttonBack = GetNode<MenuButton>("UIFrame/VBoxContainer/BackMenuButton");

        signals = (Signals)GetNode("/root/Signals");
        gameSettings = (GameSettings)GetNode("/root/GameSettings");
        audioManager = (AudioManager)GetNode("/root/AudioManager");

        //  Disabled by default
        ConfigureSignalsCallbacks(false);
        SetProcessUnhandledKeyInput(false);
        UpdateLabels();
    }

    public override void _ExitTree()
    {
        ConfigureSignalsCallbacks(false);
    }

    public void ConfigureSignalsCallbacks(bool enable)
    {
        if (isConnected == enable)
        {
            return;
        }

        if (enable)
        {
            signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
            signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));
            signals.Connect(nameof(Signals.SoundsMuted), this, nameof(OnSoundsMuted));
            signals.Connect(nameof(Signals.MusicMuted), this, nameof(OnMusicMuted));
        }
        else
        {
            signals.Disconnect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
            signals.Disconnect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));
            signals.Disconnect(nameof(Signals.SoundsMuted), this, nameof(OnSoundsMuted));
            signals.Disconnect(nameof(Signals.MusicMuted), this, nameof(OnMusicMuted));
        }

        isConnected = enable;
    }

    private void UpdateLabels()
    {
        bool musicEnabled = audioManager.IsMusicEnabled();
        Label musicLabel = GetNode<Label>("UIFrame/VBoxContainer/MusicButtonHBoxContainer/MusicLabel");
        musicLabel.Text = musicEnabled ? "ON" : "OFF";
        musicLabel.Set("custom_colors/font_color", musicEnabled ? Colors.Cyan : Colors.Orange);

        bool soundsEnabled = audioManager.IsSoundsEnabled();
        Label soundsLabel = GetNode<Label>("UIFrame/VBoxContainer/SoundsButtonHBoxContainer/SoundsLabel");
        soundsLabel.Text = soundsEnabled ? "ON" : "OFF";
        soundsLabel.Set("custom_colors/font_color", soundsEnabled ? Colors.Cyan : Colors.Orange);

        bool cameraLagEnabled = gameSettings.CameraLagEnabled;
        Label cameraLagLabel = GetNode<Label>("UIFrame/VBoxContainer/CameraLagHBoxContainer/CameraLagLabel");
        cameraLagLabel.Text = cameraLagEnabled ? "ON" : "OFF";
        cameraLagLabel.Set("custom_colors/font_color", cameraLagEnabled ? Colors.Cyan : Colors.Orange);
    }

    public void ShowMenu(bool show)
    {
        if (show)
        {
            UpdateLabels();
            SetProcessUnhandledKeyInput(true);
            ConfigureSignalsCallbacks(true);

            if (selectedButton == null)
            {
                selectedButton = buttonMusic;
            }

            selectedButton?.FocusButton();
            isShown = true;
        }
        else
        {
            SetProcessUnhandledKeyInput(false);
            ConfigureSignalsCallbacks(false);
            OnUnFocusButton();
            isShown = false;
        }

        isHiding = false;
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        GD.Print("OPTIONS MENU");

        KeyList key = (KeyList)@event.Scancode;

        if (@event.Pressed)
        {
            if (key == KeyList.Up)
            {
                if (selectedButton == null)
                {
                    FocusButton(buttonMusic);
                }
                else if (selectedButton == buttonMusic)
                {
                    FocusButton(buttonBack);
                }
                else if (selectedButton == buttonSounds)
                {
                    FocusButton(buttonMusic);
                }
                else if (selectedButton == buttonCameraLag)
                {
                    FocusButton(buttonSounds);
                }
                else if (selectedButton == buttonBack)
                {
                    FocusButton(buttonCameraLag);
                }

                audioManager.PlayMenuFocusOptionSound();
            }
            else if (key == KeyList.Down)
            {
                if (selectedButton == null)
                {
                    FocusButton(buttonMusic);
                }
                else if (selectedButton == buttonMusic)
                {
                    FocusButton(buttonSounds);
                }
                else if (selectedButton == buttonSounds)
                {
                    FocusButton(buttonCameraLag);
                }
                else if (selectedButton == buttonCameraLag)
                {
                    FocusButton(buttonBack);
                }
                else if (selectedButton == buttonBack)
                {
                    FocusButton(buttonMusic);
                }

                audioManager.PlayMenuFocusOptionSound();
            }

            if (key == KeyList.Escape && !isHiding)
            {
                OnBackPressed();
            }
        }
        else
        {
            if (key == KeyList.Enter || key == KeyList.KpEnter)
            {
                if (@event.Pressed)
                {
                    if (selectedButton != null)
                    {
                        if (selectedButton != buttonMusic && selectedButton != buttonSounds)
                        {
                            audioManager.PlayMenuSelectSound();
                        }

                        selectedButton._on_MenuButton_button_down();
                    }
                }
                else
                {
                    if (selectedButton == buttonMusic)
                    {
                        OnMusicPressed();
                    }
                    else if (selectedButton == buttonSounds)
                    {
                        OnSoundsPressed();
                    }
                    else if (selectedButton == buttonCameraLag)
                    {
                        audioManager.PlayMenuSelectSound();
                        OnCameraLagPressed();
                    }
                    else if (selectedButton == buttonBack)
                    {
                        OnBackPressed();
                    }
                }
            }
        }
    }

    public void FocusButton(MenuButton button)
    {
        if (button == selectedButton)
        {
            return;
        }

        audioManager.PlayMenuFocusOptionSound();

        selectedButton?.UnfocusButton();
        selectedButton = button;
        selectedButton?.FocusButton();
    }

    private void OnUnFocusButton()
    {
        selectedButton?.UnfocusButton();
        selectedButton = null;
    }

    private void _on_MenuButtonMusic_pressed()
    {
        OnMusicPressed();
    }

    private void OnMusicPressed()
    {
        audioManager.ToggleMusic();
        UpdateLabels();
    }

    private void _on_MenuButtonSounds_pressed()
    {
        OnSoundsPressed();
    }

    private void OnSoundsPressed()
    {
        audioManager.ToggleSounds();
        UpdateLabels();
    }

    private void _on_MenuButtonCameraLag_pressed()
    {
        OnCameraLagPressed();
    }

    private void OnCameraLagPressed()
    {
        gameSettings.CameraLagEnabled = !gameSettings.CameraLagEnabled;
        
        signals.EmitSignal(nameof(Signals.GameSettingsUpdated));
        UpdateLabels();
    }

    private void _on_MenuButtonBack_pressed()
    {
        OnBackPressed();
    }

    private void OnBackPressed()
    {
        isHiding = true;

        audioManager.PlayMenuRolloutSound();
        signals.EmitSignal(nameof(Signals.OptionsMenuVisibilityChanged), false);
    }

    private void OnSoundsMuted(bool muted)
    {
        UpdateLabels();
    }

    private void OnMusicMuted(bool muted)
    {
        UpdateLabels();
    }
}
