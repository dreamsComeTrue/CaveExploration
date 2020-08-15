using Godot;
using System.Collections.Generic;

public class CaveGeneratorNode : Spatial
{
    private CaveGenerator caveGenerator;

    PackedScene wallScene;
    PackedScene groundTileScene;
    PackedScene treasureScene;

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
        treasureScene = (PackedScene)ResourceLoader.Load("res://scenes/Treasure.tscn");
        caveGenerator = new CaveGenerator(MapSize, MapSize, 16, 2);

        signals = (Signals)GetNode("/root/Signals");

        GenerateCaves();
    }

    private enum WallSegmentType
    {
        None,
        Horizontal,
        Veritcal,
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
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

                    case CaveGenerator.CellType.Wall:
                        GenerateGroundTile(x * 0.5f, y * 0.5f);

                        WallSegmentType wallSegmentType = WallSegmentType.Horizontal;

                        if (x - 1 >= 0 && x + 1 < mapData.GetUpperBound(0) &&
                            y - 1 >= 0 && y + 1 < mapData.GetUpperBound(1))
                        {
                            if ((mapData[x, y - 1] != CaveGenerator.CellType.Wall && mapData[x - 1, y - 1] == CaveGenerator.CellType.Wall))
                            {
                                wallSegmentType = WallSegmentType.BottomLeft;
                            }

                            if ((mapData[x - 1, y] == CaveGenerator.CellType.Wall || mapData[x + 1, y] == CaveGenerator.CellType.Wall) &&
                                ((mapData[x, y - 1] != CaveGenerator.CellType.Wall) && (mapData[x, y + 1] != CaveGenerator.CellType.Wall)))
                            {
                                wallSegmentType = WallSegmentType.Horizontal;
                            }

                            if ((mapData[x, y - 1] == CaveGenerator.CellType.Wall || mapData[x, y + 1] == CaveGenerator.CellType.Wall) &&
                                ((mapData[x - 1, y] != CaveGenerator.CellType.Wall) && (mapData[x + 1, y] != CaveGenerator.CellType.Wall)))
                            {
                                wallSegmentType = WallSegmentType.Veritcal;
                            }

                            if ((mapData[x, y - 1] == CaveGenerator.CellType.Wall && mapData[x + 1, y] == CaveGenerator.CellType.Wall) &&
                                (mapData[x - 1, y] != CaveGenerator.CellType.Wall) && (mapData[x, y + 1] != CaveGenerator.CellType.Wall))
                            {
                                wallSegmentType = WallSegmentType.TopRight;
                            }

                            if ((mapData[x, y - 1] == CaveGenerator.CellType.Wall && mapData[x - 1, y] == CaveGenerator.CellType.Wall) &&
                               (mapData[x + 1, y] != CaveGenerator.CellType.Wall) && (mapData[x, y + 1] != CaveGenerator.CellType.Wall))
                            {
                                wallSegmentType = WallSegmentType.TopLeft;
                            }

                            if ((mapData[x, y + 1] == CaveGenerator.CellType.Wall && mapData[x + 1, y] == CaveGenerator.CellType.Wall) &&
                                (mapData[x - 1, y] != CaveGenerator.CellType.Wall) && (mapData[x, y - 1] != CaveGenerator.CellType.Wall))
                            {
                                wallSegmentType = WallSegmentType.BottomRight;
                            }

                            if ((mapData[x, y + 1] == CaveGenerator.CellType.Wall && mapData[x - 1, y] == CaveGenerator.CellType.Wall) &&
                                (mapData[x + 1, y] != CaveGenerator.CellType.Wall) && (mapData[x, y - 1] != CaveGenerator.CellType.Wall))
                            {
                                wallSegmentType = WallSegmentType.BottomLeft;
                            }
                        }

                        GenerateWallSegment(x * 0.5f, y * 0.5f, wallSegmentType);
                        break;

                    case CaveGenerator.CellType.Treasure:
                        GenerateGroundTile(x * 0.5f, y * 0.5f);
                        GenerateTreasure(x * 0.5f, y * 0.5f);
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

    private void GenerateTreasure(float x, float y)
    {
        Spatial treasure = (Spatial)treasureScene.Instance();
        treasure.Translation = new Vector3(x, 0.0f, y);

        AddChild(treasure);
    }

    private void GenerateWallSegment(float x, float y, WallSegmentType wallSegmentType)
    {
        if (wallSegmentType == WallSegmentType.None)
        {
            return;
        }

        Spatial wallSegment = (Spatial)wallScene.Instance();

        switch (wallSegmentType)
        {
            case WallSegmentType.Horizontal:
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                break;

            case WallSegmentType.Veritcal:
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                wallSegment.RotationDegrees = new Vector3(0.0f, 90.0f, 0.0f);
                break;

            case WallSegmentType.TopRight:
                wallSegment.GetNode<StaticBody>("WallStraight").Visible = false;
                wallSegment.GetNode<StaticBody>("WallCorner").Visible = true;
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                wallSegment.RotationDegrees = new Vector3(0.0f, 180.0f, 0.0f);
                break;

            case WallSegmentType.TopLeft:
                wallSegment.GetNode<StaticBody>("WallStraight").Visible = false;
                wallSegment.GetNode<StaticBody>("WallCorner").Visible = true;
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                wallSegment.RotationDegrees = new Vector3(0.0f, 270.0f, 0.0f);
                break;

            case WallSegmentType.BottomRight:
                wallSegment.GetNode<StaticBody>("WallStraight").Visible = false;
                wallSegment.GetNode<StaticBody>("WallCorner").Visible = true;
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                wallSegment.RotationDegrees = new Vector3(0.0f, 90.0f, 0.0f);
                break;

            case WallSegmentType.BottomLeft:
                wallSegment.GetNode<StaticBody>("WallStraight").Visible = false;
                wallSegment.GetNode<StaticBody>("WallCorner").Visible = true;
                wallSegment.Translation = new Vector3(x, 0.28f, y);
                break;
        }

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
