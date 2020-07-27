using Godot;
using System;

public class InventoryItem : NinePatchRect
{
    private AnimationPlayer animationPlayer;

    private Signals signals;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.FocusInventoryItem), this, nameof(OnFocusInventoryItem));
    }

    private void OnFocusInventoryItem(InventoryItem item)
    {
        if (item == this)
        {
            FocusInventoryItem();
        }
        else
        {
            UnFocusInventoryItem();
        }
    }

    public void FocusInventoryItem()
    {
        animationPlayer.Play("pulsate");
        SelfModulate = Colors.White;
    }

    public void UnFocusInventoryItem()
    {
        animationPlayer.Stop(true);
        animationPlayer.Seek(0.0f, true);
        SelfModulate = new Color(1.0f, 1.0f, 1.0f, 100.0f / 255.0f);
    }
}
