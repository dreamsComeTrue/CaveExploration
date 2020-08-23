using Godot;
using System;

public class MenuButton : TextureButton
{
    private AnimationPlayer pulsateAnimationPlayer;
    private AnimationPlayer focusAnimationPlayer;

    private NinePatchRect menuButtonTexture;
    private ColorRect colorRect;
    private Texture blueFrameTexture;
    private Texture violetFrameTexture;
    private Color normalColor = new Color(0.14f, 0.14f, 0.3f);
    private Color pressedColor = new Color(0.16f, 0.16f, 0.35f);

    private Signals signals;
    private AudioManager audioManager;


    public override void _Ready()
    {
        pulsateAnimationPlayer = GetNode<AnimationPlayer>("PulsateAnimationPlayer");
        focusAnimationPlayer = GetNode<AnimationPlayer>("FocusAnimationPlayer");
        menuButtonTexture = GetNode<NinePatchRect>("MenuButtonTexture");
        colorRect = GetNode<ColorRect>("ColorRect");

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

        menuButtonTexture.Texture = violetFrameTexture;
        colorRect.Color = normalColor;
    }

    public void _on_MenuButton_button_down()
    {
        menuButtonTexture.Texture = blueFrameTexture;
        colorRect.Color = pressedColor;
    }

    public void _on_MenuButton_button_up()
    {
        menuButtonTexture.Texture = violetFrameTexture;
        colorRect.Color = normalColor;

        audioManager.PlayMenuSelectSound();
    }
}
