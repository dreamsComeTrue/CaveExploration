using Godot;
using System;

public class Treasure : Spatial
{
    private MeshInstance mesh;
    private ObjectFloater objectFloater;

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance>("Mesh");
        objectFloater = new ObjectFloater();
        objectFloater.Initialize(mesh.Translation.y, (float)GD.RandRange(0.0f, 5.0f));
    }

    public override void _Process(float delta)
    {
        mesh.Translation = objectFloater.CalculateMeshFloat(delta, mesh.Translation, 60.0f);
    }
}
