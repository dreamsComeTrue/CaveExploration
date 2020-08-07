using Godot;
using System;

public class MainMenuUI : Control
{
    private static Color disabledModulateColor = new Color(1.0f, 1.0f, 1.0f, 50.0f / 255.0f);
    AnimationPlayer animationPlayer;
    private ScenesFadeTransition scenesFadeTransition;
    private Signals signals;

    private LineEdit nameLineEdit;
    private MenuButton buttonPlay;
    private MenuButton buttonOptions;
    private MenuButton buttonExit;

    private MenuButton selectedButton;

    public override void _Ready()
    {
        Visible = false;

        nameLineEdit = GetNode<LineEdit>("UIFrame/VBoxContainer/PlayerNameRect/PlayerNameLineEdit");
        buttonPlay = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonPlay");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonOptions");
        buttonExit = GetNode<MenuButton>("UIFrame/VBoxContainerBottom/MenuButtonExit");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
        signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));

        ToggleVisibility();

        nameLineEdit.GrabFocus();
        FocusButton(null);
        _on_PlayerNameLineEdit_text_changed(nameLineEdit.Text);
    }

    public void ToggleVisibility()
    {
        if (!Visible)
        {
            animationPlayer.Play("slide");
            signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), true);
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
                    nameLineEdit.GrabFocus();
                    FocusButton(null);
                }
                else if (selectedButton == buttonPlay)
                {
                    nameLineEdit.GrabFocus();
                    FocusButton(null);
                }
                else if (selectedButton == buttonOptions)
                {
                    if (nameLineEdit.Text != "")
                    {
                        FocusButton(buttonPlay);
                    }
                    else
                    {
                        nameLineEdit.GrabFocus();
                        FocusButton(null);
                    }
                }
                else if (selectedButton == buttonExit)
                {
                    FocusButton(buttonOptions);
                }
            }
            else if ((KeyList)@event.Scancode == KeyList.Down)
            {
                if (selectedButton == null)
                {
                    FocusButton(null);
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
                    nameLineEdit.GrabFocus();
                    FocusButton(null);
                }
            }
        }

        if ((KeyList)@event.Scancode == KeyList.Enter)
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

    public void FocusButton(MenuButton button)
    {
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

            if (keyEvent.Pressed)
            {
                if ((KeyList)keyEvent.Scancode == KeyList.Down)
                {
                    nameLineEdit.ReleaseFocus();

                    if (nameLineEdit.Text != "")
                    {
                        FocusButton(buttonPlay);
                    }
                    else
                    {
                        FocusButton(buttonOptions);
                    }

                    GetTree().SetInputAsHandled();
                }
                else if ((KeyList)keyEvent.Scancode == KeyList.Up)
                {
                    nameLineEdit.ReleaseFocus();
                    FocusButton(buttonExit);
                    GetTree().SetInputAsHandled();
                }

                if ((KeyList)keyEvent.Scancode == KeyList.Enter)
                {
                    buttonPlay._on_MenuButton_button_down();
                }
            }
            else
            {
                if ((KeyList)keyEvent.Scancode == KeyList.Enter && !buttonPlay.Disabled)
                {
                    _on_MenuButtonPlay_pressed();
                }
            }
        }
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
