using Godot;
using System.Collections.Generic;

public class CaveGenerator
{
    public enum CellType
    {
        Empty,
        Room,
        Wall,
        Treasure,
        Decoration,
        Start,
        End
    }

    public int GridWidth;
    public int GridHeight;

    public int MaxTreasuresPerRoom;
    public int MaxDecorationsPerRoom;

    public int MaxPlayersCount;
    public int MaxRoomsCount;

    public CellType[,] data;
    public List<Room> rooms;
    public List<Triangle> triangles;
    public HashSet<Prim.Edge> mst;

    Grid2D<CellType> grid;

    public CaveGenerator(int width, int height, int maxPlayersCount, int maxRoomsCount, int maxTreasuresPerRoomCount, int maxDecorationsPerRoom)
    {
        GridWidth = width;
        GridHeight = height;
        MaxPlayersCount = maxPlayersCount;
        MaxRoomsCount = maxRoomsCount;
        MaxTreasuresPerRoom = maxTreasuresPerRoomCount;
        MaxDecorationsPerRoom = maxDecorationsPerRoom;

        grid = new Grid2D<CellType>(new Vector2(GridWidth, GridHeight), Vector2.Zero);
    }

    public CellType[,] Generate()
    {
        ulong newSeed;
        GD.RandSeed(123, out newSeed);

        rooms = GenerateRooms();
        triangles = Triangulate(rooms);
        mst = CalculateMST(triangles);
        GenerateTreasures();
        GenerateDecorations();
        GenerateRoomData();
        FindPaths(rooms, mst);
        CleanUpBlocks();
        GeenrateStartPoints();
        GenerateExitRoom();

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
            int xStart = (int)(GD.RandRange(2, GridWidth));
            int yStart = (int)(GD.RandRange(2, GridHeight));
            int width = (int)(GD.RandRange(minRoomWidth, maxRoomWidth));
            int height = (int)(GD.RandRange(minRoomHeight, maxRoomHeight));

            Room room = new Room(xStart, yStart, width, height);

            if (room.Area.End.x >= GridWidth - 2 || room.Area.End.y >= GridHeight - 2)
            {
                continue;
            }

            bool overlapingRooms = false;
            foreach (Room existingRoom in rooms)
            {
                if (room.Area.Grow(2.0f).Intersects(existingRoom.Area, true))
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
            middlePoints[i] = rooms[i].Area.Position + rooms[i].Area.Size / 2;
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

    private Room FindRoomForPoint(Vector2 point)
    {
        for (int i = 0; i < rooms.Count; ++i)
        {
            Vector2 middlePoint = rooms[i].Area.Position + rooms[i].Area.Size / 2;

            if (middlePoint.IsEqualApprox(point))
            {
                return rooms[i];
            }
        }

        return null;
    }

    private HashSet<Prim.Edge> CalculateMST(List<Triangle> triangles)
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (Triangle triangle in triangles)
        {
            Graphs.Vertex a = new Graphs.Vertex<Room>(triangle.pointA, FindRoomForPoint(triangle.pointA));
            Graphs.Vertex b = new Graphs.Vertex<Room>(triangle.pointB, FindRoomForPoint(triangle.pointB));
            Graphs.Vertex c = new Graphs.Vertex<Room>(triangle.pointC, FindRoomForPoint(triangle.pointC));

            edges.Add(new Prim.Edge(a, b));
            edges.Add(new Prim.Edge(b, c));
            edges.Add(new Prim.Edge(c, a));
        }

        List<Prim.Edge> mstCalculated = Prim.MinimumSpanningTree(edges, edges[0].U);

        mst = new HashSet<Prim.Edge>(mstCalculated);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(mst);

        foreach (var edge in remainingEdges)
        {
            if (GD.Randf() < 0.125f)
            {
                mst.Add(edge);
            }
        }

        return mst;
    }

    private void GenerateRoomData()
    {
        data = new CellType[GridWidth, GridHeight];

        for (int y = 0; y < GridHeight; ++y)
        {
            for (int x = 0; x < GridWidth; ++x)
            {
                Room foundRoom = null;

                foreach (Room room in rooms)
                {
                    if (x >= room.Area.Position.x && x <= room.Area.End.x &&
                    y >= room.Area.Position.y && y <= room.Area.End.y)
                    {
                        foundRoom = room;
                        break;
                    }
                }

                if (foundRoom == null)
                {
                    data[x, y] = CellType.Empty;
                }
                else
                {
                    bool foundTreasure = IsTreasure(foundRoom, x, y);
                    bool foundDecoration = IsDecoration(foundRoom, x, y);

                    if (foundTreasure)
                    {
                        data[x, y] = CellType.Treasure;
                    }
                    else if (foundDecoration)
                    {
                        data[x, y] = CellType.Decoration;
                    }
                    else
                    {
                        data[x, y] = CellType.Room;
                    }

                    grid[x, y] = CellType.Room;
                }
            }
        }
    }

    private bool IsTreasure(Room foundRoom, int x, int y)
    {
        bool found = false;
        foreach (Vector2 treasurePos in foundRoom.Treasures)
        {
            if (Mathf.IsEqualApprox(foundRoom.Area.Position.x + treasurePos.x, x) &&
            Mathf.IsEqualApprox(foundRoom.Area.Position.y + treasurePos.y, y))
            {
                found = true;
                break;
            }
        }

        return found;
    }

    private bool IsDecoration(Room foundRoom, int x, int y)
    {
        bool found = false;
        foreach (Vector2 decorationPos in foundRoom.Decorations)
        {
            if (Mathf.IsEqualApprox(foundRoom.Area.Position.x + decorationPos.x, x) &&
            Mathf.IsEqualApprox(foundRoom.Area.Position.y + decorationPos.y, y))
            {
                found = true;
                break;
            }
        }

        return found;
    }

    private void FindPaths(List<Room> rooms, HashSet<Prim.Edge> edges)
    {
        Pathfinder2D aStar = new Pathfinder2D(new Vector2(GridWidth, GridHeight));

        foreach (var edge in mst)
        {
            var startRoom = (edge.U as Graphs.Vertex<Room>).Item;
            var endRoom = (edge.V as Graphs.Vertex<Room>).Item;

            var startPosf = startRoom.Area.Position + startRoom.Area.Size / 2;
            var endPosf = endRoom.Area.Position + endRoom.Area.Size / 2;
            var startPos = new Vector2((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (Pathfinder2D.Node a, Pathfinder2D.Node b) =>
            {
                var pathCost = new Pathfinder2D.PathCost();

                pathCost.cost = b.Position.DistanceTo(endPos);    //heuristic

                if (grid[b.Position] == CellType.Room)
                {
                    pathCost.cost += 10;
                }
                else if (grid[b.Position] == CellType.Empty)
                {
                    pathCost.cost += 5;
                }
                else if (grid[b.Position] == CellType.Wall)
                {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.Empty)
                    {
                        grid[current] = CellType.Room;
                    }
                }

                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Room)
                    {
                        data[(int)pos.x, (int)pos.y] = CellType.Room;
                    }
                }
            }
        }
    }

    private void CleanUpBlocks()
    {
        CellType[,] dataCopy = new CellType[GridWidth, GridHeight];
        for (int y = 0; y < GridHeight; ++y)
        {
            for (int x = 0; x < GridWidth; ++x)
            {
                dataCopy[x, y] = data[x, y];
            }
        }

        for (int y = 0; y < GridHeight; ++y)
        {
            for (int x = 0; x < GridWidth; ++x)
            {
                int foundAdjacentFilledBlocks = 0;

                for (int j = -1; j < 2; ++j)
                {
                    for (int i = -1; i < 2; ++i)
                    {
                        if ((x + i >= 0 && x + i < GridWidth) && (y + j >= 0 && y + j < GridHeight))
                        {
                            if (i == 0 && j == 0)
                            {
                                continue;
                            }

                            if (dataCopy[x + i, y + j] != CellType.Empty)
                            {
                                foundAdjacentFilledBlocks++;
                            }
                        }
                    }
                }

                // Also with world bounds -> || x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1
                if ((foundAdjacentFilledBlocks > 0 && dataCopy[x, y] == CellType.Empty))
                {
                    data[x, y] = CellType.Wall;
                }
            }
        }
    }

    private void GenerateTreasures()
    {
        foreach (Room room in rooms)
        {
            int currentTreasures = 0;
            int maxTreasuresInRoom = (int)GD.RandRange(0, MaxTreasuresPerRoom + 1);

            while (currentTreasures < maxTreasuresInRoom)
            {
                int x = (int)GD.RandRange(0, room.Area.Size.x);
                int y = (int)GD.RandRange(0, room.Area.Size.y);

                Vector2 treasurePos = new Vector2(x, y);

                if (room.Treasures.Contains(treasurePos))
                {
                    continue;
                }

                room.Treasures.Add(treasurePos);

                currentTreasures++;
            }
        }
    }

    private void GenerateDecorations()
    {
        foreach (Room room in rooms)
        {
            int currentDecorations = 0;
            int maxDecorationsInRoom = (int)GD.RandRange(1, MaxDecorationsPerRoom + 1);

            while (currentDecorations < maxDecorationsInRoom)
            {
                int x = (int)GD.RandRange(0, room.Area.Size.x);
                int y = (int)GD.RandRange(0, room.Area.Size.y);

                Vector2 decorationPos = new Vector2(x, y);

                if (room.Treasures.Contains(decorationPos) || room.Decorations.Contains(decorationPos))
                {
                    continue;
                }

                room.Decorations.Add(decorationPos);

                currentDecorations++;
            }
        }
    }

    private void GeenrateStartPoints()
    {
        int currentStartPoints = 0;

        while (currentStartPoints < MaxPlayersCount)
        {
            int roomIndex = (int)GD.RandRange(0, rooms.Count);

            if (!rooms[roomIndex].IsStartPoint)
            {
                rooms[roomIndex].IsStartPoint = true;
                currentStartPoints++;
            }
        }
    }

    private void GenerateExitRoom()
    {
        foreach (Room room in rooms)
        {

        }
    }

    public class Triangle
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public Vector2 pointC;
    }

    public class Room
    {
        public Rect2 Area;

        public bool IsExit;
        public bool IsStartPoint;

        public List<Vector2> Treasures;
        public List<Vector2> Decorations;

        public Room(int x, int y, int width, int height)
        {
            Area = new Rect2(x, y, width, height);
            Treasures = new List<Vector2>();
            Decorations = new List<Vector2>();
            IsExit = false;
            IsStartPoint = false;
        }
    }
}