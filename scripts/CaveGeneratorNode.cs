using Godot;
using System.Collections.Generic;

public class CaveGeneratorNode : Spatial
{
    private CaveGenerator caveGenerator;

    PackedScene wallScene;
    PackedScene groundTileScene;
    PackedScene treasureScene;
    PackedScene decorationScene;

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
        decorationScene = (PackedScene)ResourceLoader.Load("res://scenes/LevelDecoration.tscn");
        caveGenerator = new CaveGenerator(MapSize, MapSize, 16, 2, 5);

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
        BottomLeft,
        TopLeftRight,
        TopBottomRight,
        BottomLeftRight,
        TopBottomLeft,
        TopBottomLeftRight
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

                        string binaryStr = "";
                        for (int j = -1; j < 2; ++j)
                        {
                            for (int i = -1; i < 2; ++i)
                            {
                                if ((x + i >= 0 && x + i < mapData.GetUpperBound(0)) && (y + j >= 0 && y + j < mapData.GetUpperBound(1)))
                                {
                                    if (mapData[x + i, y + j] == CaveGenerator.CellType.Wall)
                                    {
                                        binaryStr = "1" + binaryStr;
                                    }
                                    else
                                    {
                                        binaryStr = "0" + binaryStr;
                                    }
                                }
                                else
                                {
                                    binaryStr = "0" + binaryStr;
                                }
                            }
                        }

                        binaryStr = binaryStr.PadLeft(9, '0');

                        int binary = System.Convert.ToInt32(binaryStr, 2);

                        WallSegmentType wallSegmentType = WallSegmentType.None;
                        System.Func<int, int, bool> checkBit = (number, bit) => (number & 1 << bit) == 1 << bit;

                        if (checkBit(binary, 3) || checkBit(binary, 5))
                        {
                            wallSegmentType = WallSegmentType.Horizontal;
                        }

                        if (checkBit(binary, 1) || checkBit(binary, 7))
                        {
                            wallSegmentType = WallSegmentType.Veritcal;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 5))
                        {
                            wallSegmentType = WallSegmentType.TopRight;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 3))
                        {
                            wallSegmentType = WallSegmentType.TopLeft;
                        }

                        if (checkBit(binary, 7) && checkBit(binary, 3))
                        {
                            wallSegmentType = WallSegmentType.BottomLeft;
                        }

                        if (checkBit(binary, 7) && checkBit(binary, 5))
                        {
                            wallSegmentType = WallSegmentType.BottomRight;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 3) && checkBit(binary, 5))
                        {
                            wallSegmentType = WallSegmentType.TopLeftRight;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 7) && checkBit(binary, 5))
                        {
                            wallSegmentType = WallSegmentType.TopBottomRight;
                        }

                        if (checkBit(binary, 3) && checkBit(binary, 5) && checkBit(binary, 7))
                        {
                            wallSegmentType = WallSegmentType.BottomLeftRight;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 3) && checkBit(binary, 7))
                        {
                            wallSegmentType = WallSegmentType.TopBottomLeft;
                        }

                        if (checkBit(binary, 1) && checkBit(binary, 3) && checkBit(binary, 5) && checkBit(binary, 7))
                        {
                            wallSegmentType = WallSegmentType.TopBottomLeftRight;
                        }

                        GenerateWallSegment(x * 0.5f, y * 0.5f, wallSegmentType);
                        break;

                    case CaveGenerator.CellType.Treasure:
                        GenerateGroundTile(x * 0.5f, y * 0.5f);
                        GenerateTreasure(x * 0.5f, y * 0.5f);
                        break;

                    case CaveGenerator.CellType.Decoration:
                        GenerateGroundTile(x * 0.5f, y * 0.5f);
                        GenerateDecoration(x * 0.5f, y * 0.5f);
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

    private void GenerateDecoration(float x, float y)
    {
        Spatial decoration = (Spatial)decorationScene.Instance();
        decoration.Translation = new Vector3(x, 0.0f, y);
        
        int randAngle = (int)(GD.Randi() % 24) * 15;
        decoration.RotationDegrees = new Vector3(0.0f, randAngle, 0.0f);

        int decorationIdx = (int)GD.Randi() % decoration.GetChildCount();

        for (int i = 0; i < decoration.GetChildCount(); ++i)
        {
            if (i == decorationIdx)
            {
                continue;
            }

            Node child = decoration.GetChild(i);
            decoration.RemoveChild(child);
            child.QueueFree();
        }

        AddChild(decoration);
    }

    private void RemoveWallParts(Spatial wallSegment, params string[] children)
    {
        foreach (string child in children)
        {
            Node childNode = wallSegment.GetNode<StaticBody>(child);
            wallSegment.RemoveChild(childNode);
            childNode.QueueFree();
        }
    }

    private void GenerateWallSegment(float x, float y, WallSegmentType wallSegmentType)
    {
        if (wallSegmentType == WallSegmentType.None)
        {
            return;
        }

        Spatial wallSegment = (Spatial)wallScene.Instance();
        wallSegment.Translation = new Vector3(x, 0.28f, y);

        switch (wallSegmentType)
        {
            case WallSegmentType.Horizontal:
                RemoveWallParts(wallSegment, "WallCorner", "WallSplit", "WallIntersection");
                break;

            case WallSegmentType.Veritcal:
                RemoveWallParts(wallSegment, "WallCorner", "WallSplit", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 90.0f, 0.0f);
                break;

            case WallSegmentType.TopRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallSplit", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 180.0f, 0.0f);
                break;

            case WallSegmentType.TopLeft:
                RemoveWallParts(wallSegment, "WallStraight", "WallSplit", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 270.0f, 0.0f);
                break;

            case WallSegmentType.BottomRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallSplit", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 90.0f, 0.0f);
                break;

            case WallSegmentType.BottomLeft:
                RemoveWallParts(wallSegment, "WallStraight", "WallSplit", "WallIntersection");
                break;

            case WallSegmentType.TopLeftRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallCorner", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 180.0f, 0.0f);
                break;

            case WallSegmentType.TopBottomRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallCorner", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 90.0f, 0.0f);
                break;

            case WallSegmentType.BottomLeftRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallCorner", "WallIntersection");
                break;

            case WallSegmentType.TopBottomLeft:
                RemoveWallParts(wallSegment, "WallStraight", "WallCorner", "WallIntersection");
                wallSegment.RotationDegrees = new Vector3(0.0f, 270.0f, 0.0f);
                break;

            case WallSegmentType.TopBottomLeftRight:
                RemoveWallParts(wallSegment, "WallStraight", "WallCorner", "WallSplit");
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
