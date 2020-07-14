using Godot;
using System.Collections.Generic;

public class CaveGeneratorNode : Spatial
{
    private CaveGenerator caveGenerator;

    PackedScene wallScene;
    PackedScene groundTileScene;

    private Signals signals;

    private int MapSize = 64;

    public CaveGenerator.CellType[,] mapData;
    public List<CaveGenerator.Room> rooms;
    public List<CaveGenerator.Triangle> triangles;
    public HashSet<Prim.Edge> mst;

    public override void _Ready()
    {
        wallScene = (PackedScene)ResourceLoader.Load("res://scenes/Wall.tscn");
        groundTileScene = (PackedScene)ResourceLoader.Load("res://scenes/GroundTile.tscn");
        caveGenerator = new CaveGenerator(MapSize, MapSize, 16);

        signals = (Signals)GetNode("/root/Signals");

        GenerateCaves();
    }
    private void GenerateCaves()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        mapData = caveGenerator.Generate();
        rooms = caveGenerator.rooms;
        triangles = caveGenerator.triangles;
        mst = caveGenerator.mst;

        for (int y = mapData.GetLowerBound(1); y <= mapData.GetUpperBound(1); y++)
        {
            for (int x = mapData.GetLowerBound(0); x <= mapData.GetUpperBound(0); x++)
            {
                switch (mapData[x, y])
                {
                    case CaveGenerator.CellType.Room:
                        GenerateGroundTile(x * 0.5f, y * 0.5f);
                        break;

                    case CaveGenerator.CellType.None:
						GenerateGroundTile(x * 0.5f, y * 0.5f);
                        GenerateWallSegment(x * 0.5f, y * 0.5f);
                        break;
                }
            }
        }

        signals.EmitSignal(nameof(Signals.MapGenerated));
    }

    private void GenerateGroundTile(float x, float y)
    {
        Spatial groundTile = (Spatial)groundTileScene.Instance();
        groundTile.Translation = new Vector3(x, 0.0f, y);

        AddChild(groundTile);
    }

    private void GenerateWallSegment(float x, float y)
    {
        Spatial wallSegment = (Spatial)wallScene.Instance();
        wallSegment.Translation = new Vector3(x, 0.28f, y);

        AddChild(wallSegment);
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.G && !@event.Pressed)
        {
            GenerateCaves();
        }
    }
}
