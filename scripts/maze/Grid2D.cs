using Godot;

public class Grid2D<T>
{
	T[] data;

	public Vector2 Size { get; private set; }
	public Vector2 Offset { get; set; }

	public Grid2D(Vector2 size, Vector2 offset)
	{
		Size = size;
		Offset = offset;

		data = new T[(int)size.x * (int)size.y];
	}

	public int GetIndex(Vector2 pos)
	{
		return (int)pos.x + ((int)Size.x * (int)pos.y);
	}

	public bool InBounds(Vector2 pos)
	{
		return new Rect2(Vector2.Zero, Size).HasPoint(pos + Offset);
	}

	public T this[int x, int y]
	{
		get
		{
			return this[new Vector2(x, y)];
		}
		set
		{
			this[new Vector2(x, y)] = value;
		}
	}

	public T this[Vector2 pos]
	{
		get
		{
			pos += Offset;
			return data[GetIndex(pos)];
		}
		set
		{
			pos += Offset;
			data[GetIndex(pos)] = value;
		}
	}
}
