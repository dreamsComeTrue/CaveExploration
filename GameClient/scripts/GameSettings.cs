using System.Collections.Generic;
using Godot;
using System;

public class GameSettings : Node
{
    private AudioManager audioManager;
    private Signals signals;

    public string filePath;

    public bool CameraLagEnabled = true;
    public bool MusicMuted = false;
    public bool SoundsMuted = false;

    public override void _Ready()
    {
        filePath = "user://unHiddenGameSettings.save";

        audioManager = (AudioManager)GetNode("/root/AudioManager");
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.GameSettingsUpdated), this, nameof(SaveGameSettings));

        LoadGameSettings();
    }

    public void LoadGameSettings()
    {
        File saveGame = new File();

        if (!saveGame.FileExists(filePath))
        {
            return; // Error!  We don't have a save to load.
        }

        saveGame.Open(filePath, File.ModeFlags.Read);

        try
        {
            MusicMuted = !(bool)saveGame.GetVar();
            SoundsMuted = !(bool)saveGame.GetVar();
            CameraLagEnabled = (bool)saveGame.GetVar();
        }
        catch (Exception e)
        {
            GD.PrintErr("Exception while reading game settings file! " + e.Message);
        }
        finally
        {
            saveGame.Close();
        }
    }

    public void SaveGameSettings()
    {
        File saveGame = new File();
        saveGame.Open(filePath, File.ModeFlags.Write);

        saveGame.StoreVar(!MusicMuted);
        saveGame.StoreVar(!SoundsMuted);
        saveGame.StoreVar(CameraLagEnabled);

        saveGame.Close();
    }
}
