using System.Collections.Generic;
using Godot;

public class MainMenuPlayer : MeshInstance
{
    private ObjectFloater objectFloater;

    public override void _Ready()
    {
        objectFloater = new ObjectFloater();
        objectFloater.Initialize(this.Translation.y);
    }

    public override void _Process(float delta)
    {
        UpdateMeshFloat(delta);
    }

    private void UpdateMeshFloat(float delta)
    {
        float floatFrequency = objectFloater.FloatFrequency;

        this.Translation = objectFloater.CalculateMeshFloat(delta, this.Translation, floatFrequency);
    }
}
