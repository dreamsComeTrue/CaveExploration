using Godot;
using System;

public class AudioManager : Node
{
    private AudioStreamPlayer menuMusicPlayer;
    private AudioStreamPlayer menuTypeWriterPlayer;
    private AudioStreamPlayer menuRolloutSoundPlayer;
    private AudioStreamPlayer menuFocusSoundPlayer;
    private AudioStreamPlayer menuOpenSoundPlayer;
    private AudioStreamPlayer menuSelectSoundPlayer;
    
    private const float SOUND_VOLUME = -12.0f;

    public override void _Ready()
    {
        menuMusicPlayer = AddSound("res://music/doodle.ogg");

        menuFocusSoundPlayer = AddSound("res://sounds/ui/sfx_movement_footsteps5.wav");
        menuFocusSoundPlayer.VolumeDb = SOUND_VOLUME;

        menuSelectSoundPlayer = AddSound("res://sounds/ui/sfx_menu_move3.wav");
        menuSelectSoundPlayer.VolumeDb = SOUND_VOLUME;

        menuRolloutSoundPlayer = AddSound("res://sounds/ui/sfx_menu_select2.wav");
        menuRolloutSoundPlayer.VolumeDb = SOUND_VOLUME;

        menuOpenSoundPlayer = AddSound("res://sounds/ui/sfx_sounds_interaction15.wav");
        menuOpenSoundPlayer.VolumeDb = SOUND_VOLUME;

        menuTypeWriterPlayer = AddSound("res://sounds/ui/Menu_Select_00.wav");
        menuTypeWriterPlayer.VolumeDb = SOUND_VOLUME;
    }

    private AudioStreamPlayer AddSound(string resPath)
    {
        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = ResourceLoader.Load<AudioStream>(resPath);        
        AddChild(player);

        return player;
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

    public void PlayMenuRolloutSound()
    {
        menuRolloutSoundPlayer.Play();
    }

    public void PlayMenuOpenSound()
    {
        menuOpenSoundPlayer.Play();
    }

    public void PlayTypeWriterSound()
    {
        menuTypeWriterPlayer.Play();
    }
}
