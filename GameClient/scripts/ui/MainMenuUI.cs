using Godot;
using System;

public class MainMenuUI : Control
{
    private static Color disabledModulateColor = new Color(1.0f, 1.0f, 1.0f, 50.0f / 255.0f);
    AnimationPlayer animationPlayer;
    private ScenesFadeTransition scenesFadeTransition;
    private Signals signals;
    private AudioManager audioManager;

    private LineEdit nameLineEdit;
    AnimationPlayer nameLineAnimationPlayer;
    private MenuButton buttonPlay;
    private MenuButton buttonOptions;
    private MenuButton buttonExit;

    private MenuButton selectedButton;

    public override void _Ready()
    {
        Visible = false;

        nameLineEdit = GetNode<LineEdit>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameLineEdit");
        nameLineAnimationPlayer = GetNode<AnimationPlayer>("UIFrame/VBoxContainer/PlayerNameContainer/PlayerNameRect/AnimationPlayer");
        buttonPlay = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonPlay");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonOptions");
        buttonExit = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonExit");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        audioManager = (AudioManager)GetNode("/root/AudioManager");
        signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
        signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));

        ToggleVisibility();

        FocusButton(null);

        nameLineEdit.GrabFocus();
        nameLineAnimationPlayer.Play("pulsate");
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;

        _on_PlayerNameLineEdit_text_changed(nameLineEdit.Text);
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;

        audioManager.PlayMainMenuMusic();
    }

    public void ToggleVisibility()
    {
        if (!Visible)
        {
            animationPlayer.Play("slide");
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
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
                        if (selectedButton != buttonPlay)
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
                    _on_MenuButtonPlay_pressed();
                }
                else if (selectedButton == buttonOptions)
                {
                    _on_MenuButtonOptions_pressed();
                }
                else if (selectedButton == buttonExit)
                {
                    _on_MenuButtonExit_pressed();
                }
            }
        }
    }

    private void GrabNameEditFocus()
    {
        nameLineEdit.GrabFocus();
        nameLineAnimationPlayer.Play("pulsate");
        nameLineEdit.CaretPosition = nameLineEdit.Text.Length;
        audioManager.PlayMenuFocusOptionSound();
    }

    private void UnfocusNameEdit()
    {
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

    public void _on_MenuButtonPlay_pressed()
    {
        if (!buttonPlay.Disabled)
        {
            audioManager.PlayMenuOpenSound();
            GameManager gameManager = (GameManager)GetNode("/root/GameManager");
            gameManager.PlayerName = nameLineEdit.Text;

            scenesFadeTransition.Run("res://scenes/GameplayScene.tscn");
        }
    }

    public void _on_MenuButtonOptions_pressed()
    {
        scenesFadeTransition.Run("res://scenes/GameplayScene.tscn");
    }

    public void _on_MenuButtonExit_pressed()
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
                            KeyList.Apostrophe, KeyList.Backslash, KeyList.Comma, KeyList.Period, KeyList.Slash };
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
