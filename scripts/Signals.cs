using Godot;
using System;

public class Signals : Node
{
    [Signal]
    public delegate void InGameMenuVisibilityChanged(bool shown);
    
    [Signal]
    public delegate void PulseGameplayTimer(float timeLeft);
}
