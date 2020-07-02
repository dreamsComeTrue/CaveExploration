using Godot;
using System;
using System.Collections.Generic;

public class MimimapCanvas : Control
{
    private Signals signals;

    private MazeGeneratorWorker.CellType[,] mapData;
    private List<MazeGeneratorWorker.Triangle> triangles;
    public HashSet<Prim.Edge> mst;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.MapGenerated), this, nameof(OnMapGenerated));
    }

    private void OnMapGenerated()
    {
        MazeGenerator generator = GetTree().Root.GetNode<MazeGenerator>("Gameplay/MazeGenerator");

        mapData = generator.mapData;
        triangles = generator.triangles;
        mst = generator.mst;
        Update();
    }

    public override void _Draw()
    {
        if (mapData == null)
        {
            return;
        }

        Vector2 size = new Vector2(3, 3);

        for (int y = mapData.GetLowerBound(1); y <= mapData.GetUpperBound(1); y++)
        {
            for (int x = mapData.GetLowerBound(0); x <= mapData.GetUpperBound(0); x++)
            {
                if (mapData[x, y] == MazeGeneratorWorker.CellType.Wall)
                {
                    Vector2 position = new Vector2(x * size.x, y * size.y);
                    Rect2 rect = new Rect2(position, size);

                    DrawRect(rect, Colors.White);
                }
            }
        }

        foreach (MazeGeneratorWorker.Triangle triangle in triangles)
        {
            DrawLine(triangle.pointA * size, triangle.pointB * size, Colors.LightGreen, 2);
            DrawLine(triangle.pointB * size, triangle.pointC * size, Colors.LightGreen, 2);
            DrawLine(triangle.pointC * size, triangle.pointA * size, Colors.LightGreen, 2);
        }

        foreach (Prim.Edge edge in mst)
        {
            DrawLine(edge.U.Position * size, edge.V.Position * size, Colors.Red, 2);            
        }
    }
}
