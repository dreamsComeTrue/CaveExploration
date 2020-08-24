using Godot;

public class InGameMenu : Control
{
    AnimationPlayer animationPlayer;
    private Signals signals;
    private AudioManager audioManager;

    private MenuButton buttonResume;
    private MenuButton buttonOptions;
    private MenuButton buttonLeave;
    private MenuButton buttonQuit;

    private MenuButton selectedButton;

    private Control overlays;

    private ScenesFadeTransition scenesFadeTransition;

    public override void _Ready()
    {
        Visible = false;

        buttonResume = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonResume");
        buttonOptions = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonOptions");
        buttonLeave = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonLeave");
        buttonQuit = GetNode<MenuButton>("UIFrame/VBoxContainer/MenuButtonQuit");

        overlays = GetTree().Root.GetNode<Control>("Gameplay/GameUI/CanvasLayer/Overlays");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        scenesFadeTransition = (ScenesFadeTransition)GetNode("/root/ScenesFadeTransition");

        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.FocusMenuButton), this, nameof(FocusButton));
        signals.Connect(nameof(Signals.UnFocusMenuButton), this, nameof(OnUnFocusButton));

        audioManager = (AudioManager)GetNode("/root/AudioManager");
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

        return shown;
    }

    public void SlideFunction()
    {
        overlays.Visible = !this.Visible;
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (!Visible)
        {
            return;
        }

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

                audioManager.PlayMenuFocusOptionSound();
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

                audioManager.PlayMenuFocusOptionSound();
            }
        }

        KeyList key = (KeyList)@event.Scancode;

        if (key == KeyList.Enter || key == KeyList.KpEnter)
        {
            if (@event.Pressed)
            {
                if (selectedButton != null)
                {
                    audioManager.PlayMenuSelectSound();
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
                    _on_MenuButtonLeave_pressed();
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

    private void _on_MenuButtonLeave_pressed()
    {
        scenesFadeTransition.Run("res://scenes/ui/MainMenuScene.tscn");
    }

    private void _on_MenuButtonQuit_pressed()
    {
        GetTree().Quit();
    }
}
