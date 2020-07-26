using Godot;

public class InGameMenu : Control
{
    AnimationPlayer animationPlayer;
    private Signals signals;

    private MenuButton buttonResume;
    private MenuButton buttonOptions;
    private MenuButton buttonLeave;
    private MenuButton buttonQuit;

    private MenuButton selectedButton;

    public override void _Ready()
    {
        Visible = false;

        buttonResume = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonResume");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonOptions");
        buttonLeave = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonLeave");
        buttonQuit = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonQuit");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
        signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));
    }

    public void ToggleVisibility()
    {
        if (!Visible)
        {
            animationPlayer.Play("slide");
            signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), true);

            if (selectedButton == null)
            {
                FocusButton(buttonResume);
            }
            else
            {
                FocusButton(selectedButton);
            }
        }
        else
        {
            HideInGameMenu();
        }
    }

    private void HideInGameMenu()
    {
        animationPlayer.PlayBackwards("slide");
        signals.EmitSignal(nameof(Signals.InGameMenuVisibilityChanged), false);

        selectedButton?.UnfocusButton();
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (@event.Pressed)
        {
            if ((KeyList)@event.Scancode == KeyList.Up)
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
            }
            else if ((KeyList)@event.Scancode == KeyList.Down)
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
                if (selectedButton == buttonResume)
                {
                    _on_MenuButtonResume_pressed();
                }
                else if (selectedButton == buttonOptions)
                {
                    _on_MenuButtonResume_pressed();
                }
                else if (selectedButton == buttonLeave)
                {
                    _on_MenuButtonResume_pressed();
                }
                else if (selectedButton == buttonQuit)
                {
                    _on_MenuButtonQuit_pressed();
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

    private void _on_MenuButtonResume_pressed()
    {
        HideInGameMenu();
    }

    private void _on_MenuButtonQuit_pressed()
    {
        GetTree().Quit();
    }
}
