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

    public int MaxRoomsCount;

    public CellType[,] data;
    public List<Room> rooms;
    public List<Triangle> triangles;
    public HashSet<Prim.Edge> mst;

    public MazeGeneratorWorker(int width, int height, int maxRoomsCount)
    {
        GridWidth = width;
        GridHeight = height;
        MaxRoomsCount = maxRoomsCount;
    }

    public CellType[,] Generate()
    {
        GD.Randomize();

        data = new CellType[GridWidth, GridHeight];
        rooms = GenerateRooms();
        triangles = Triangulate(rooms);
        mst = CalculateMST(triangles);

        for (int y = 0; y < GridHeight; ++y)
        {
            for (int x = 0; x < GridWidth; ++x)
            {
                bool foundRoom = false;

                foreach (Room room in rooms)
                {
                    if (x >= room.rect.Position.x && x <= room.rect.End.x &&
                    y >= room.rect.Position.y && y <= room.rect.End.y)
                    {
                        foundRoom = true;
                        break;
                    }
                }

                if (!foundRoom)
                {
                    data[x, y] = CellType.Empty;
                }
                else
                {
                    data[x, y] = CellType.Wall;
                }
            }
        }

        return data;
    }

    private List<Room> GenerateRooms()
    {
        int maxRoomWidth = (int)(GridWidth / 4.0f);
        int minRoomWidth = (int)(0.3f * maxRoomWidth);

        int maxRoomHeight = (int)(GridHeight / 4.0f);
        int minRoomHeight = (int)(0.3f * maxRoomHeight);

        List<Room> rooms = new List<Room>();
        int numberOfOverlappingAttempts = 0;
        while (rooms.Count < MaxRoomsCount || numberOfOverlappingAttempts < 10)
        {
            int xStart = (int)(GD.Randi() % GridWidth);
            int yStart = (int)(GD.Randi() % GridHeight);
            int width = (int)(GD.RandRange(minRoomWidth, maxRoomWidth));
            int height = (int)(GD.RandRange(minRoomHeight, maxRoomHeight));

            Room room = new Room(xStart, yStart, width, height);

            if (room.rect.End.x > GridWidth || room.rect.End.y > GridHeight)
            {
                continue;
            }

            bool overlapingRooms = false;
            foreach (Room existingRoom in rooms)
            {
                if (room.rect.Grow(2.0f).Intersects(existingRoom.rect, true))
                {
                    overlapingRooms = true;
                    break;
                }
            }

            if (!overlapingRooms)
            {
                rooms.Add(room);
            }
            else
            {
                numberOfOverlappingAttempts++;
            }
        }

        return rooms;
    }

    private List<Triangle> Triangulate(List<Room> rooms)
    {
        Vector2[] middlePoints = new Vector2[rooms.Count];

        for (int i = 0; i < rooms.Count; ++i)
        {
            middlePoints[i] = rooms[i].rect.Position + rooms[i].rect.Size / 2;
        }

        int[] indicies = Geometry.TriangulateDelaunay2d(middlePoints);
        List<Triangle> triangles = new List<Triangle>();

        for (int i = 0; i < indicies.Length; i += 3)
        {
            Triangle triangle = new Triangle();
            triangle.pointA = middlePoints[indicies[i]];
            triangle.pointB = middlePoints[indicies[i + 1]];
            triangle.pointC = middlePoints[indicies[i + 2]];

            triangles.Add(triangle);
        }

        return triangles;
    }

    private HashSet<Prim.Edge> CalculateMST(List<Triangle> triangles)
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (Triangle triangle in triangles)
        {
            Graphs.Vertex a = new Graphs.Vertex(triangle.pointA);
            Graphs.Vertex b = new Graphs.Vertex(triangle.pointB);
            Graphs.Vertex c = new Graphs.Vertex(triangle.pointC);

            edges.Add(new Prim.Edge(a, b));
            edges.Add(new Prim.Edge(b, c));
            edges.Add(new Prim.Edge(c, a));
        }

        List<Prim.Edge> mstCalculated = Prim.MinimumSpanningTree(edges, edges[0].U);

        mst = new HashSet<Prim.Edge>(mstCalculated);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(mst);

        Random random = new Random(0);
        foreach (var edge in remainingEdges)
        {
            if (random.NextDouble() < 0.125)
            {
                mst.Add(edge);
            }
        }

        return mst;
    }

    public class Triangle
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public Vector2 pointC;
    }

    public class Room
    {
        public Rect2 rect;

        public Room(int x, int y, int width, int height)
        {
            rect = new Rect2(x, y, width, height);
        }
    }
}
