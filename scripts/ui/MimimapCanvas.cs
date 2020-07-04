using Godot;
using System.Collections.Generic;

public class MimimapCanvas : Control
{
    private Signals signals;

    private MazeGeneratorWorker.CellType[,] mapData;
    private List<MazeGeneratorWorker.Triangle> triangles;
    public HashSet<Prim.Edge> mst;

    private Vector3 playerPosition;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.MapGenerated), this, nameof(OnMapGenerated));
        signals.Connect(nameof(Signals.PlayerMoved), this, nameof(OnPlayerMoved));
    }

    private void OnMapGenerated()
    {
        MazeGenerator generator = GetTree().Root.GetNode<MazeGenerator>("Gameplay/MazeGenerator");

        mapData = generator.mapData;
        triangles = generator.triangles;
        mst = generator.mst;
        Update();
    }

    private void OnPlayerMoved(Vector3 newPosition)
    {
        playerPosition = newPosition;
        Update();
    }

    public override void _Draw()
    {
        if (mapData == null)
        {
            return;
        }

        Vector2 scaler = new Vector2(this.RectSize.x / mapData.GetUpperBound(0), this.RectSize.y / mapData.GetUpperBound(1));

        for (int y = mapData.GetLowerBound(1); y < mapData.GetUpperBound(1); y++)
        {
            for (int x = mapData.GetLowerBound(0); x < mapData.GetUpperBound(0); x++)
            {
                if (mapData[x, y] == MazeGeneratorWorker.CellType.Room)
                {
                    Vector2 position = new Vector2(x * scaler.x, y * scaler.y);
                    Rect2 rect = new Rect2(position, scaler);

                    DrawRect(rect, Colors.Green);
                }
            }
        }

        Rect2 playerRect = new Rect2(new Vector2(playerPosition.x, playerPosition.z) * scaler * 2, scaler);
        DrawRect(playerRect, Colors.Red);

        foreach (MazeGeneratorWorker.Triangle triangle in triangles)
        {
            // DrawLine(triangle.pointA * size, triangle.pointB * size, Colors.LightGreen, 2);
            // DrawLine(triangle.pointB * size, triangle.pointC * size, Colors.LightGreen, 2);
            // DrawLine(triangle.pointC * size, triangle.pointA * size, Colors.LightGreen, 2);
        }

        foreach (Prim.Edge edge in mst)
        {
            // DrawLine(edge.U.Position * size, edge.V.Position * size, Colors.Red, 2);            
        }
    }
}
