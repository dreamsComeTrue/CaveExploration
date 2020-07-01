using Godot;
using System;

public class MazeGenerator : Spatial
{
	private MazeGeneratorWorker worker;

	PackedScene wallScene;

	private int MapSize = 32;

	public override void _Ready()
	{
		wallScene = (PackedScene)ResourceLoader.Load("res://scenes/Wall.tscn");
		worker = new MazeGeneratorWorker(MapSize, MapSize);

		GenerateMaze();
	}
	private void GenerateMaze()
	{
		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}

		int xOffset = (int)(MapSize / 2 * 0.5);
		int yOffset = (int)(MapSize / 2 * 0.5);

		MazeGeneratorWorker.CellType[,] data = worker.Generate();

		for (int y = data.GetLowerBound(1); y <= data.GetUpperBound(1); y++)
		{
			//   string line = "";
			for (int x = data.GetLowerBound(0); x <= data.GetUpperBound(0); x++)
			{
				if (x == data.GetLowerBound(0) || x == data.GetUpperBound(0) || y == data.GetLowerBound(1) || y == data.GetUpperBound(1))
				{
					GenerateWallSegment(x * 0.5f - xOffset, y * 0.5f - yOffset);
				}

				switch (data[x, y])
				{
					case MazeGeneratorWorker.CellType.Empty:
						//     line += "O ";
						break;
					case MazeGeneratorWorker.CellType.Wall:
						GenerateWallSegment(x * 0.5f - xOffset, y * 0.5f - yOffset);
						//  line += "X ";
						break;
				}
			}

			// GD.Print(line);
		}
	}

	private void GenerateWallSegment(float x, float y)
	{
		Spatial wallSegment = (Spatial)wallScene.Instance();
		wallSegment.Translation = new Vector3(x, 0.28f, y);

		AddChild(wallSegment);
	}

	public override void _UnhandledKeyInput(InputEventKey @event)
	{
		if ((KeyList)@event.Scancode == KeyList.G)
		{
			GenerateMaze();
		}
	}
}
