using Godot;
using System;
using System.Collections.Generic;

public class MazeGeneratorWorker
{
	public enum CellType
	{
		Empty,
		Wall,
		Start,
		End
	}

	public int GridWidth;
	public int GridHeight;

	public CellType[,] data;

	public MazeGeneratorWorker(int width, int height)
	{
		GridWidth = width;
		GridHeight = height;
	}

	private Random rng = new Random();

	private void Shuffle(List<Vector2> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			Vector2 value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public CellType[,] Generate()
	{
		data = new CellType[GridWidth, GridHeight];

		int FILLED_TILES_RATIO = (int)(Mathf.Floor((GridWidth * GridHeight) / 2));

		for (int j = 0; j < GridHeight; ++j)
		{
			for (int i = 0; i < GridWidth; ++i)
			{
				data[i, j] = CellType.Wall;
			}
		}

		int filledTilesCounter = 0;
		List<Vector2> openTiles = new List<Vector2>();
		List<Vector2> allOpenTiles = new List<Vector2>();

		GD.Randomize();

		long xStart = GD.Randi() % GridWidth;
		long yStart = GD.Randi() % GridHeight;

		while (filledTilesCounter < FILLED_TILES_RATIO)
		{
			Shuffle(openTiles);
			Shuffle(allOpenTiles);

			int x = -1;
			int y = -1;

			if (openTiles.Count > 0)
			{
				Vector2 tile = openTiles[openTiles.Count - 1];
				x = (int)tile.x;
				y = (int)tile.y;

				openTiles.RemoveAt(openTiles.Count - 1);

				if (openTiles.Count > 1 && GD.RandRange(0, 20) > 4)
				{
					openTiles.RemoveAt(openTiles.Count - 1);
				}
				if (openTiles.Count > 1 && GD.RandRange(0, 20) > 2)
				{
					openTiles.RemoveAt(openTiles.Count - 1);
				}
			}

			if (x < 0 && allOpenTiles.Count > 0)
			{
				Vector2 tile = allOpenTiles[0];
				x = (int)tile.x;
				y = (int)tile.y;
			}

			// if x is set then we expect y to be set too, so we do not check at this point for it
			if (x < 0)
			{
				x = (int)xStart;
				y = (int)yStart;
			}

			if (data[x, y] != CellType.Wall)
			{
				continue;
			}

			data[x, y] = CellType.Empty;
			filledTilesCounter++;

			// north
			if (y > 0 && data[x, y - 1] == CellType.Wall)
			{
				Vector2 newTile = new Vector2(x, y - 1);
				openTiles.Add(newTile);
				allOpenTiles.Add(newTile);
			}
			// east
			if (x < GridWidth - 1 && data[x + 1, y] == CellType.Wall)
			{
				Vector2 newTile = new Vector2(x + 1, y);
				openTiles.Add(newTile);
				allOpenTiles.Add(newTile);
			}
			// south
			if (y < GridHeight - 1 && data[x, y + 1] == CellType.Wall)
			{
				Vector2 newTile = new Vector2(x, y + 1);
				openTiles.Add(newTile);
				allOpenTiles.Add(newTile);
			}
			// west
			if (x > 0 && data[x - 1, y] == CellType.Wall)
			{
				Vector2 newTile = new Vector2(x - 1, y);
				openTiles.Add(newTile);
				allOpenTiles.Add(newTile);
			}
		}

		return data;
	}
}
