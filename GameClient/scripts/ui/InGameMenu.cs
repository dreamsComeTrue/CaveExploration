using Godot;

public class InGameMenu : Control
{
    AnimationPlayer animationPlayer;
    public AnimationPlayer optionsAnimationPlayer;
    private Signals signals;
    private AudioManager audioManager;

    private MenuButton buttonResume;
    private MenuButton buttonOptions;
    private MenuButton buttonLeave;
    private MenuButton buttonQuit;

    private MenuButton selectedButton;

    private Control overlays;

    private ScenesFadeTransition scenesFadeTransition;

    private bool isConnected = false;

    public override void _Ready()
    {
        Visible = false;

        buttonResume = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonResume");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonOptions");
        buttonLeave = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonLeave");
        buttonQuit = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonQuit");

        overlays = GetTree().Root.GetNode<Control>("Gameplay/GameUI/CanvasLayer/Overlays");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        optionsAnimationPlayer = GetNode<AnimationPlayer>("OptionsAnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        ConfigureSignalsCallbacks(false);
        SetProcessUnhandledKeyInput(false);

        audioManager = (AudioManager)GetNode("/root/AudioManager");
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

            if (signals.IsConnected(nameof(Signals.OptionsMenuVisibilityChanged), this, nameof(OnOptionsMenuVisibilityChanged)))
            {
                signals.Disconnect(nameof(Signals.OptionsMenuVisibilityChanged), this, nameof(OnOptionsMenuVisibilityChanged));
            }
        }
        else
        {
            signals.Disconnect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
            signals.Disconnect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));
            signals.Connect(nameof(Signals.OptionsMenuVisibilityChanged), this, nameof(OnOptionsMenuVisibilityChanged));
        }

        isConnected = enable;
    }

    public void ShowMenu(bool show)
    {
        if (show)
        {
            SetProcessUnhandledKeyInput(true);
            ConfigureSignalsCallbacks(true);

            selectedButton?.UnfocusButton();
            selectedButton = buttonOptions;
            selectedButton?.FocusButton();
        }
        else
        {
            SetProcessUnhandledKeyInput(false);
            ConfigureSignalsCallbacks(false);
            OnUnFocusButton();
        }
    }

    private void OnOptionsMenuVisibilityChanged(bool visible)
    {
        if (!visible)
        {
            optionsAnimationPlayer.PlayBackwards("slide");
        }
    }

    public bool ToggleVisibility()
    {
        bool shown = false;

        if (!Visible)
        {
            animationPlayer.Play("slide");
            signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), true);

            if (selectedButton == null)
            {
                selectedButton = buttonResume;
                selectedButton?.FocusButton();
            }
            else
            {
                FocusButton(selectedButton);
                selectedButton?.FocusButton();
            }

            shown = true;
        }
        else
        {
            animationPlayer.PlayBackwards("slide");
            signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), false);
            selectedButton?.UnfocusButton();

            shown = false;
        }
        
        ConfigureSignalsCallbacks(shown);
        SetProcessUnhandledKeyInput(shown);

        return shown;
    }

    public void SlideFunction()
    {
        overlays.Visible = !this.Visible;
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        GD.Print("IN-GAME MENU");

        if (!Visible)
        {
            return;
        }

        KeyList key = (KeyList)@event.Scancode;

        if (@event.Pressed)
        {
            if (key == KeyList.Up)
            {
                if (selectedButton == null)
                {
                    FocusButton(buttonResume);
                }
                else if (selectedButton == buttonResume)
                {
                    FocusButton(buttonQuit);
                }
                else if (selectedButton == buttonOptions)
                {
                    FocusButton(buttonResume);
                }
                else if (selectedButton == buttonLeave)
                {
                    FocusButton(buttonOptions);
                }
                else if (selectedButton == buttonQuit)
                {
                    FocusButton(buttonLeave);
                }

                audioManager.PlayMenuFocusOptionSound();
            }
            else if (key == KeyList.Down)
            {
                if (selectedButton == null)
                {
                    FocusButton(buttonResume);
                }
                else if (selectedButton == buttonResume)
                {
                    FocusButton(buttonOptions);
                }
                else if (selectedButton == buttonOptions)
                {
                    FocusButton(buttonLeave);
                }
                else if (selectedButton == buttonLeave)
                {
                    FocusButton(buttonQuit);
                }
                else if (selectedButton == buttonQuit)
                {
                    FocusButton(buttonResume);
                }

                audioManager.PlayMenuFocusOptionSound();
            }
        }

        if (key == KeyList.Enter || key == KeyList.KpEnter)
        {
            if (@event.Pressed)
            {
                if (selectedButton != null)
                {
                    if (selectedButton != buttonOptions)
                    {
                        audioManager.PlayMenuSelectSound();
                    }

                    selectedButton._on_MenuButton_button_down();
                }
            }
            else
            {
                if (selectedButton == buttonResume)
                {
                    OnResumePressed();
                }
                else if (selectedButton == buttonOptions)
                {
                    OnOptionsPressed();
                }
                else if (selectedButton == buttonLeave)
                {
                    OnLeavePressed();
                }
                else if (selectedButton == buttonQuit)
                {
                    OnQuitPressed();
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

    private void _on_MenuButtonResume_pressed()
    {
        ToggleVisibility();
    }

    private void OnResumePressed()
    {
        _on_MenuButtonResume_pressed();
    }

    private void _on_MenuButtonOptions_pressed()
    {
        OnOptionsPressed();
    }

    private void OnOptionsPressed()
    {
        audioManager.PlayMenuRolloutSound();
        optionsAnimationPlayer.Play("slide");        
    }

    private void _on_MenuButtonLeave_pressed()
    {
        scenesFadeTransition.Run("res://scenes/ui/MainMenuScene.tscn");
    }

    private void OnLeavePressed()
    {
        _on_MenuButtonLeave_pressed();
    }

    private void _on_MenuButtonQuit_pressed()
    {
        GetTree().Quit();
    }

    private void OnQuitPressed()
    {
        _on_MenuButtonQuit_pressed();
    }
}
