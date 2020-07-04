using Godot;
using System.Collections.Generic;

public class MazeGenerator : Spatial
{
	private MazeGeneratorWorker worker;

	PackedScene wallScene;

	private Signals signals;

	private int MapSize = 64;

	public MazeGeneratorWorker.CellType[,] mapData;
	public List<MazeGeneratorWorker.Room> rooms;
	public List<MazeGeneratorWorker.Triangle> triangles;
	public HashSet<Prim.Edge> mst;

	public override void _Ready()
	{
		wallScene = (PackedScene)ResourceLoader.Load("res://scenes/Wall.tscn");
		worker = new MazeGeneratorWorker(MapSize, MapSize, 12);

		signals = (Signals)GetNode("/root/Signals");

		GenerateMaze();
	}
	private void GenerateMaze()
	{
		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}

		mapData = worker.Generate();
		rooms = worker.rooms;
		triangles = worker.triangles;
		mst = worker.mst;

		for (int y = mapData.GetLowerBound(1); y <= mapData.GetUpperBound(1); y++)
		{
			for (int x = mapData.GetLowerBound(0); x <= mapData.GetUpperBound(0); x++)
			{
				if (x == mapData.GetLowerBound(0) || x == mapData.GetUpperBound(0) || y == mapData.GetLowerBound(1) || y == mapData.GetUpperBound(1))
				{
					GenerateWallSegment(x * 0.5f, y * 0.5f);
				}

				switch (mapData[x, y])
				{
					case MazeGeneratorWorker.CellType.Room:
						break;

					case MazeGeneratorWorker.CellType.None:
						GenerateWallSegment(x * 0.5f, y * 0.5f);
						break;
				}
			}
		}

		signals.EmitSignal(nameof(Signals.MapGenerated));
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
			GenerateMaze();
		}
	}
}
