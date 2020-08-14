using Godot;
using System;

public class AudioManager : Node
{
    private AudioStreamPlayer menuMusicPlayer;
    private AudioStreamPlayer menuTypeWriterPlayer;
    private AudioStreamPlayer menuFocusSoundPlayer;
    private AudioStreamPlayer menuSelectSoundPlayer;

    public override void _Ready()
    {
        menuMusicPlayer = new AudioStreamPlayer();
        menuMusicPlayer.Stream = ResourceLoader.Load<AudioStream>("res://music/doodle.ogg");
        menuMusicPlayer.VolumeDb = -5.0f;
        AddChild(menuMusicPlayer);

        menuFocusSoundPlayer = new AudioStreamPlayer();
        menuFocusSoundPlayer.Stream = ResourceLoader.Load<AudioStream>("res://sounds/ui/Mobeyee_Sounds_Metal_Click.wav");
        menuFocusSoundPlayer.VolumeDb = -8.0f;
        AddChild(menuFocusSoundPlayer);

        menuSelectSoundPlayer = new AudioStreamPlayer();
        menuSelectSoundPlayer.Stream = ResourceLoader.Load<AudioStream>("res://sounds/ui/GUI_Sound_Effects_by_Lokif_misc_menu_4.wav");
        menuSelectSoundPlayer.VolumeDb = -8.0f;
        AddChild(menuSelectSoundPlayer);

        menuTypeWriterPlayer = new AudioStreamPlayer();
        menuTypeWriterPlayer.Stream = ResourceLoader.Load<AudioStream>("res://sounds/ui/Menu_Select_00.wav");
        menuTypeWriterPlayer.VolumeDb = -15.0f;
        AddChild(menuTypeWriterPlayer);
    }

    public void PlayMainMenuMusic()
    {
        menuMusicPlayer.Play();
    }

    public void PlayMenuFocusOptionSound()
    {
        menuFocusSoundPlayer.Play();
    }

    public void PlayMenuSelectSound()
    {
        menuSelectSoundPlayer.Play();
    }

    public void PlayTypeWriterSound()
    {
        menuTypeWriterPlayer.Play();
    }
}
