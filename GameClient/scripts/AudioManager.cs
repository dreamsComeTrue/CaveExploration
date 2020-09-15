using Godot;
using System;

public class AudioManager : Node
{
    private Signals signals;

    private AudioStreamPlayer menuMusicPlayer;
    private AudioStreamPlayer menuTypeWriterPlayer;
    private AudioStreamPlayer menuRolloutSoundPlayer;
    private AudioStreamPlayer menuFocusSoundPlayer;
    private AudioStreamPlayer menuOpenSoundPlayer;
    private AudioStreamPlayer menuSelectSoundPlayer;
    private AudioStreamPlayer flashlightSoundPlayer;
    private AudioStreamPlayer toggleSoundPlayer;

    private const float SOUND_VOLUME = -15.0f;

    public bool MusicMuted = false;
    public bool SoundsMuted = false;

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

        flashlightSoundPlayer = AddSound("res://sounds/player/sfx_sounds_interaction22.wav");
        flashlightSoundPlayer.VolumeDb = SOUND_VOLUME;

        toggleSoundPlayer = AddSound("res://sounds/ui/Menu_Select_00.wav");
        toggleSoundPlayer.VolumeDb = SOUND_VOLUME;

        signals = (Signals)GetNode("/root/Signals");
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
        if (!MusicMuted)
        {
            menuMusicPlayer.Play();
        }
    }

    public void PlayMenuFocusOptionSound()
    {
        if (!SoundsMuted)
        {
            menuFocusSoundPlayer.Play();
        }
    }

    public void PlayMenuSelectSound()
    {
        if (!SoundsMuted)
        {
            menuSelectSoundPlayer.Play();
        }
    }

    public void PlayMenuRolloutSound()
    {
        if (!SoundsMuted)
        {
            menuRolloutSoundPlayer.Play();
        }
    }

    public void PlayMenuOpenSound()
    {
        if (!SoundsMuted)
        {
            menuOpenSoundPlayer.Play();
        }
    }

    public void PlayTypeWriterSound()
    {
        if (!SoundsMuted)
        {
            menuTypeWriterPlayer.Play();
        }
    }

    public void PlayFlashlightSound()
    {
        if (!SoundsMuted)
        {
            flashlightSoundPlayer.Play();
        }
    }

    public void PlayToggleSound()
    {
        if (!SoundsMuted)
        {
            toggleSoundPlayer.Play();
        }
    }

    public void ToggleSounds()
    {
        menuSelectSoundPlayer.Play();

        SoundsMuted = !SoundsMuted;

        signals.EmitSignal(nameof(Signals.SoundsMuted), SoundsMuted);
    }

    public void ToggleMusic()
    {
        menuSelectSoundPlayer.Play();

        MusicMuted = !MusicMuted;

        menuMusicPlayer.StreamPaused = MusicMuted;
        signals.EmitSignal(nameof(Signals.MusicMuted), MusicMuted);
    }

    public bool IsMusicEnabled()
    {
        return !MusicMuted;
    }

    public bool IsSoundsEnabled()
    {
        return !SoundsMuted;
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (@event.Pressed)
        {
            KeyList pressedKey = (KeyList)@event.Scancode;

            if (pressedKey == KeyList.F10)
            {
                ToggleSounds();
            }

            if (pressedKey == KeyList.F11)
            {
                ToggleMusic();
            }
        }
    }
}
