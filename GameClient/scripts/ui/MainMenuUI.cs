using Godot;
using System;

public class MainMenuUI : Control
{
    private static Color disabledModulateColor = new Color(1.0f, 1.0f, 1.0f, 50.0f / 255.0f);
    AnimationPlayer mainAnimationPlayer;
    AnimationPlayer optionsAnimationPlayer;
    private ScenesFadeTransition scenesFadeTransition;
    private Signals signals;
    private GameManager gameManager;
    private AudioManager audioManager;

    private LineEdit nameLineEdit;
    AnimationPlayer nameLineAnimationPlayer;
    private MenuButton buttonPlay;
    private MenuButton buttonOptions;
    private MenuButton buttonExit;
    private MessageNotifier messageNotifier;

    private MenuButton selectedButton;

    private bool isConnected = false;

    public override void _Ready()
    {
        Visible = false;

        nameLineEdit = GetNode<LineEdit>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameLineEdit");
        nameLineAnimationPlayer = GetNode<AnimationPlayer>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameRect/AnimationPlayer");
        buttonPlay = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonPlay");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonOptions");
        buttonExit = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonExit");
        messageNotifier = GetNode<MessageNotifier>("MessageNotifier");

        mainAnimationPlayer = GetNode<AnimationPlayer>("MainAnimationPlayer");
        optionsAnimationPlayer = GetNode<AnimationPlayer>("OptionsAnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        audioManager = (AudioManager)GetNode("/root/AudioManager");
        ConfigureSignalsCallbacks(true);

        gameManager = (GameManager)GetNode("/root/GameManager");

        ToggleVisibility();

        FocusButton(null);

        GetNode<ColorRect>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameRect/ColorRect").Color = new Color("9d087987");
        nameLineEdit.GrabFocus();
        nameLineAnimationPlayer.Play("pulsate");
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;

        _on_PlayerNameLineEdit_text_changed(nameLineEdit.Text);
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;

        RecoverPlayerName();
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
            signals.Connect(nameof(Signals.MessageNotify), this, nameof(OnMessageNotify));

            if (signals.IsConnected(nameof(Signals.OptionsMenuVisibilityChanged), this, nameof(OnOptionsMenuVisibilityChanged)))
            {
                signals.Disconnect(nameof(Signals.OptionsMenuVisibilityChanged), this, nameof(OnOptionsMenuVisibilityChanged));
            }
        }
        else
        {
            signals.Disconnect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
            signals.Disconnect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));
            signals.Disconnect(nameof(Signals.MessageNotify), this, nameof(OnMessageNotify));
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

            UnfocusNameEdit();
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

    private void RecoverPlayerName()
    {
        if (!string.IsNullOrEmpty(gameManager.PlayerName))
        {
            nameLineEdit.Text = gameManager.PlayerName;
            nameLineEdit.CaretPosition = nameLineEdit.Text.Length;
        }
    }
    public void ToggleVisibility()
    {
        if (!Visible)
        {
            mainAnimationPlayer.Play("slide");
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        GD.Print("MAIN MENU");

        if (@event.Pressed)
        {
            if ((KeyList)@event.Scancode == KeyList.Up)
            {
                if (selectedButton == null)
                {
                    FocusButton(null);
                    GrabNameEditFocus();
                }
                else if (selectedButton == buttonPlay)
                {
                    FocusButton(null);
                    GrabNameEditFocus();
                }
                else if (selectedButton == buttonOptions)
                {
                    if (nameLineEdit.Text != "")
                    {
                        FocusButton(buttonPlay);
                    }
                    else
                    {
                        FocusButton(null);
                        GrabNameEditFocus();
                    }
                }
                else if (selectedButton == buttonExit)
                {
                    FocusButton(buttonOptions);
                }

                PlayMenuFocusOptionSound();
            }
            else if ((KeyList)@event.Scancode == KeyList.Down)
            {
                if (selectedButton == null)
                {
                    FocusButton(null);
                    GrabNameEditFocus();
                }
                else if (selectedButton == buttonPlay)
                {
                    FocusButton(buttonOptions);
                }
                else if (selectedButton == buttonOptions)
                {
                    FocusButton(buttonExit);
                }
                else if (selectedButton == buttonExit)
                {
                    FocusButton(null);
                    GrabNameEditFocus();
                }

                PlayMenuFocusOptionSound();
            }
        }

        KeyList key = (KeyList)@event.Scancode;

        if (key == KeyList.Enter || key == KeyList.KpEnter)
        {
            if (@event.Pressed)
            {
                if (selectedButton != null)
                {
                    if (selectedButton == buttonPlay && buttonPlay.Disabled)
                    {
                        //  NOP
                    }
                    else
                    {
                        if (selectedButton == buttonExit)
                        {
                            audioManager.PlayMenuSelectSound();
                        }

                        selectedButton._on_MenuButton_button_down();
                    }
                }
            }
            else
            {
                if (selectedButton == buttonPlay && !buttonPlay.Disabled)
                {
                    OnPlayPressed();
                }
                else if (selectedButton == buttonOptions)
                {
                    OnOptionsPressed();
                }
                else if (selectedButton == buttonExit)
                {
                    OnExitPressed();
                }
            }
        }
    }

    private void GrabNameEditFocus()
    {
        GetNode<ColorRect>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameRect/ColorRect").Color = new Color("9d087987");
        nameLineEdit.GrabFocus();
        nameLineAnimationPlayer.Play("pulsate");
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;
        audioManager.PlayMenuFocusOptionSound();
    }

    private void UnfocusNameEdit()
    {
        GetNode<ColorRect>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameRect/ColorRect").Color = new Color("1a1831");
        nameLineEdit.ReleaseFocus();
        nameLineAnimationPlayer.Stop(true);
    }

    public void FocusButton(MenuButton button)
    {
        if (button == selectedButton)
        {
            return;
        }

        audioManager.PlayMenuFocusOptionSound();

        UnfocusNameEdit();
        selectedButton?.UnfocusButton();
        selectedButton = button;
        selectedButton?.FocusButton();
    }

    private void OnUnFocusButton()
    {
        selectedButton?.UnfocusButton();
        selectedButton = null;
    }

    private void OnMessageNotify(string message)
    {
        messageNotifier.AddMessage(message);
    }

    public void _on_MenuButtonPlay_pressed()
    {
        buttonPlay._on_MenuButton_button_down();
        OnUnFocusButton();
        OnPlayPressed();
    }

    private void OnPlayPressed()
    {
        if (!buttonPlay.Disabled)
        {
            audioManager.PlayMenuOpenSound();
            gameManager.PlayerName = nameLineEdit.Text;

            scenesFadeTransition.Run("res://scenes/GameplayScene.tscn");
        }
    }

    public void _on_MenuButtonOptions_pressed()
    {
        buttonOptions._on_MenuButton_button_down();
        OnUnFocusButton();
        OnOptionsPressed();
    }

    private void OnOptionsPressed()
    {
        audioManager.PlayMenuRolloutSound();
        optionsAnimationPlayer.Play("slide");
    }

    public void _on_MenuButtonExit_pressed()
    {
        buttonExit._on_MenuButton_button_down();
        OnUnFocusButton();
        OnExitPressed();
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }

    public void _on_PlayerNameLineEdit_gui_input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            InputEventKey keyEvent = (@event as InputEventKey);
            KeyList key = (KeyList)keyEvent.Scancode;

            if (keyEvent.Pressed)
            {
                if (key == KeyList.Down)
                {
                    nameLineEdit.ReleaseFocus();
                    nameLineAnimationPlayer.Stop(true);

                    if (nameLineEdit.Text != "")
                    {
                        FocusButton(buttonPlay);
                    }
                    else
                    {
                        FocusButton(buttonOptions);
                    }

                    PlayMenuFocusOptionSound();
                    GetTree().SetInputAsHandled();
                }
                else if (key == KeyList.Up)
                {
                    nameLineEdit.ReleaseFocus();
                    nameLineAnimationPlayer.Stop(true);

                    FocusButton(buttonExit);
                    PlayMenuFocusOptionSound();
                    GetTree().SetInputAsHandled();
                }
                else if (key == KeyList.Enter || key == KeyList.KpEnter)
                {
                    buttonPlay._on_MenuButton_button_down();
                }
                else if (IsKeyAlphanumeric(key))
                {
                    audioManager.PlayTypeWriterSound();
                }
            }
            else
            {
                if ((key == KeyList.Enter || key == KeyList.KpEnter) && !buttonPlay.Disabled)
                {
                    _on_MenuButtonPlay_pressed();
                }
            }
        }
        else if (@event is InputEventMouseButton)
        {
            InputEventMouseButton mouseButtonEvent = (@event as InputEventMouseButton);

            if (mouseButtonEvent.Pressed)
            {
                FocusButton(null);
                GrabNameEditFocus();
            }
        }
    }

    public void _on_PlayerNameLineEdit_focus_entered()
    {
        OnUnFocusButton();
    }

    private void PlayMenuFocusOptionSound()
    {
        if (selectedButton != null)
        {
            audioManager.PlayMenuFocusOptionSound();
        }
    }

    private bool IsKeyAlphanumeric(KeyList key)
    {
        KeyList[] codes = { KeyList.Space, KeyList.Backspace, KeyList.Left, KeyList.Right, KeyList.Minus,
                            KeyList.Plus, KeyList.Ampersand, KeyList.Bracketleft, KeyList.Bracketright, KeyList.Semicolon,
                            KeyList.Apostrophe, KeyList.Backslash, KeyList.Comma, KeyList.Period, KeyList.Slash,
                            KeyList.Home, KeyList.End, KeyList.Delete };
        return (key >= KeyList.A && key <= KeyList.Z) || (key >= KeyList.Key0 && key <= KeyList.Key9) || Array.IndexOf(codes, key) != -1;
    }

    public void _on_PlayerNameLineEdit_text_changed(string newText)
    {
        buttonPlay.Disabled = nameLineEdit.Text == "";

        if (buttonPlay.Disabled)
        {
            buttonPlay.UnfocusButton(true);
        }

        buttonPlay.Modulate = buttonPlay.Disabled ? disabledModulateColor : Colors.White;
        buttonPlay.MouseFilter = buttonPlay.Disabled ? MouseFilterEnum.Ignore : MouseFilterEnum.Pass;
    }
}
