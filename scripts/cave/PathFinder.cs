using System;
using System.Collections.Generic;
using Godot;

public class Pathfinder2D
{
	public class Node
	{
		public Vector2 Position { get; private set; }
		public Node Previous { get; set; }
		public float Cost { get; set; }

		public Node(Vector2 position)
		{
			Position = position;
		}
	}

	public struct PathCost
	{
		public bool traversable;
		public float cost;
	}

	static readonly Vector2[] neighbors = {
		new Vector2(1, 0),
		new Vector2(-1, 0),
		new Vector2(0, 1),
		new Vector2(0, -1),
	};

	Grid2D<Node> grid;
	SimplePriorityQueue<Node, float> queue;
	HashSet<Node> closed;
	Stack<Vector2> stack;

	public Pathfinder2D(Vector2 size)
	{
		grid = new Grid2D<Node>(size, Vector2.Zero);

		queue = new SimplePriorityQueue<Node, float>();
		closed = new HashSet<Node>();
		stack = new Stack<Vector2>();

		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				grid[x, y] = new Node(new Vector2(x, y));
			}
		}
	}

	void ResetNodes()
	{
		var size = grid.Size;

		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				var node = grid[x, y];
				node.Previous = null;
				node.Cost = float.PositiveInfinity;
			}
		}
	}

	public List<Vector2> FindPath(Vector2 start, Vector2 end, Func<Node, Node, PathCost> costFunction)
	{
		ResetNodes();
		queue.Clear();
		closed.Clear();

		queue = new SimplePriorityQueue<Node, float>();
		closed = new HashSet<Node>();

		grid[start].Cost = 0;
		queue.Enqueue(grid[start], 0);

		while (queue.Count > 0)
		{
			Node node = queue.Dequeue();
			closed.Add(node);

			if (node.Position == end)
			{
				return ReconstructPath(node);
			}

			foreach (var offset in neighbors)
			{
				if (!grid.InBounds(node.Position + offset)) continue;
				var neighbor = grid[node.Position + offset];
				if (closed.Contains(neighbor)) continue;

				var pathCost = costFunction(node, neighbor);
				if (!pathCost.traversable) continue;

				float newCost = node.Cost + pathCost.cost;

				if (newCost < neighbor.Cost)
				{
					neighbor.Previous = node;
					neighbor.Cost = newCost;

					if (queue.TryGetPriority(node, out float existingPriority))
					{
						queue.UpdatePriority(node, newCost);
					}
					else
					{
						queue.Enqueue(neighbor, neighbor.Cost);
					}
				}
			}
		}

		return null;
	}

	List<Vector2> ReconstructPath(Node node)
	{
		List<Vector2> result = new List<Vector2>();

		while (node != null)
		{
			stack.Push(node.Position);
			node = node.Previous;
		}

		while (stack.Count > 0)
		{
			result.Add(stack.Pop());
		}

		return result;
	}
}
