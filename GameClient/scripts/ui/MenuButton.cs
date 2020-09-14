using Godot;
using System;

public class MenuButton : TextureButton
{
    private AnimationPlayer pulsateAnimationPlayer;
    private AnimationPlayer focusAnimationPlayer;

    private NinePatchRect menuButtonTexture;
    private NinePatchRect ninePatchRect;
    private Texture blueFrameTexture;
    private Texture violetFrameTexture;
    private Color normalColor = Colors.White;
    private Color pressedColor = new Color(1.0f, 1.4f, 1.0f);

    private Signals signals;
    private AudioManager audioManager;


    public override void _Ready()
    {
        pulsateAnimationPlayer = GetNode<AnimationPlayer>("PulsateAnimationPlayer");
        focusAnimationPlayer = GetNode<AnimationPlayer>("FocusAnimationPlayer");
        menuButtonTexture = GetNode<NinePatchRect>("MenuButtonTexture");
        ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");

        blueFrameTexture = ResourceLoader.Load("res://gfx/ui/frame_blue_single.png") as Texture;
        violetFrameTexture = ResourceLoader.Load("res://gfx/ui/frame_violet_single.png") as Texture;

        signals = (Signals)GetNode("/root/Signals");
        audioManager = (AudioManager)GetNode("/root/AudioManager");
    }

    private void _on_MenuButton_mouse_entered()
    {
        signals.EmitSignal(nameof(Signals.FocusMenuButton), this);
    }

    private void _on_MenuButton_mouse_exited()
    {
        //  Do not force to UnFocus button (better UI responsivness)
    }

    public void FocusButton()
    {
        pulsateAnimationPlayer.Play("pulsate");
        focusAnimationPlayer.Play("focus");
    }

    public void UnfocusButton(bool overrideModulate = false)
    {
        pulsateAnimationPlayer.Stop(true);

        if (!overrideModulate)
        {
            focusAnimationPlayer.PlayBackwards("focus");
        }

        menuButtonTexture.Texture = null;
        ninePatchRect.SelfModulate = normalColor;
    }

    public void _on_MenuButton_button_down()
    {
        menuButtonTexture.Texture = blueFrameTexture;
        ninePatchRect.SelfModulate = pressedColor;
    }

    public void _on_MenuButton_button_up()
    {
        menuButtonTexture.Texture = null;
        ninePatchRect.SelfModulate = normalColor;

        audioManager.PlayMenuSelectSound();
    }
}
