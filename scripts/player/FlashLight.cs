using Godot;
using System;

public class FlashLight : SpotLight
{
    private MeshInstance mesh;

    private Timer timer;

    private Signals signals;

    private int currentLightBars = 24;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
        mesh = GetParent().GetNode<MeshInstance>("Mesh");
        timer = GetNode<Timer>("FlashLightTimer");
    }

    private void ToggleFlashlight()
    {
        this.Visible = !this.Visible;

        (mesh.GetSurfaceMaterial(0) as SpatialMaterial).FlagsDisableAmbientLight = !this.Visible;
        (mesh.GetSurfaceMaterial(1) as SpatialMaterial).FlagsDisableAmbientLight = !this.Visible;

        timer.Paused = !this.Visible;
        
        signals.EmitSignal(nameof(Signals.FlashLightToggled), this.Visible);
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (Input.IsActionJustPressed("action_tool") && currentLightBars > 0)
        {
            ToggleFlashlight();
        }
    }

    public void _on_FlashLightTimer_timeout()
    {
        currentLightBars--;
        signals.EmitSignal(nameof(Signals.LightBarsChanged), currentLightBars);

        if (currentLightBars <= 0)
        {
            this.Visible = false;
            (mesh.GetSurfaceMaterial(0) as SpatialMaterial).FlagsDisableAmbientLight = true;
            (mesh.GetSurfaceMaterial(1) as SpatialMaterial).FlagsDisableAmbientLight = true;
            timer.Stop();
        }
    }
}
