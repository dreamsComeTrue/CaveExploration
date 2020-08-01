using Godot;
using System;

public class MainMenuUI : Control
{
    AnimationPlayer animationPlayer;
    private ScenesFadeTransition scenesFadeTransition;
    private Signals signals;

    private MenuButton buttonOptions;
    private MenuButton buttonPlay;

    private MenuButton selectedButton;

    public override void _Ready()
    {
        Visible = false;

        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonOptions");
        buttonPlay = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonPlay");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
        signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));

        ToggleVisibility();
    }

    public void ToggleVisibility()
    {
        if (!Visible)
        {
            animationPlayer.Play("slide");
            signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), true);

            if (selectedButton == null)
            {
                FocusButton(buttonPlay);
            }
            else
            {
                FocusButton(selectedButton);
            }
        }
        else
        {
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
                    FocusButton(buttonPlay);
                }
                else if (selectedButton == buttonPlay)
                {
                    FocusButton(buttonOptions);
                }
                else if (selectedButton == buttonOptions)
                {
                    FocusButton(buttonPlay);
                }
            }
            else if ((KeyList)@event.Scancode == KeyList.Down)
            {
                if (selectedButton == null)
                {
                    FocusButton(buttonPlay);
                }
                else if (selectedButton == buttonPlay)
                {
                    FocusButton(buttonOptions);
                }
                else if (selectedButton == buttonOptions)
                {
                    FocusButton(buttonPlay);
                }
            }
        }


        if ((KeyList)@event.Scancode == KeyList.Enter)
        {
            if (@event.Pressed)
            {
                if (selectedButton != null)
                {
                    selectedButton._on_MenuButton_button_down();
                }
            }
            else
            {
                if (selectedButton == buttonPlay)
                {
                    _on_MenuButtonPlay_pressed();
                }
                else if (selectedButton == buttonOptions)
                {
                    _on_MenuButtonPlay_pressed();
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
        scenesFadeTransition.Run("res://scenes/GameplayScene.tscn");
    }

    public void _on_PlayerNameLineEdit_text_entered(string playerName)
    {
        _on_MenuButtonPlay_pressed();
    }
}
