using Godot;
using System;
using System.Collections.Generic;

public class MazeGeneratorWorker
{
    public enum CellType
    {
        None,
        Empty,
        Room,
        Hallway,
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

    Grid2D<CellType> grid;

    public MazeGeneratorWorker(int width, int height, int maxRoomsCount)
    {
        GridWidth = width;
        GridHeight = height;
        MaxRoomsCount = maxRoomsCount;

        grid = new Grid2D<CellType>(new Vector2(GridWidth, GridHeight), Vector2.Zero);
    }

    public CellType[,] Generate()
    {
        ulong newSeed;
        GD.RandSeed(123, out newSeed);

        rooms = GenerateRooms();
        triangles = Triangulate(rooms);
        mst = CalculateMST(triangles);
        GenerateRoomData();
        FindPaths(rooms, mst);
        CleanUpBlocks();

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

            if (room.rect.End.x >= GridWidth - 2 || room.rect.End.y >= GridHeight - 2)
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

    private Room FindRoomForPoint(Vector2 point)
    {
        for (int i = 0; i < rooms.Count; ++i)
        {
            Vector2 middlePoint = rooms[i].rect.Position + rooms[i].rect.Size / 2;

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
                    data[x, y] = CellType.None;
                }
                else
                {
                    data[x, y] = CellType.Room;
                    grid[x, y] = CellType.Room;
                }
            }
        }
    }

    private void FindPaths(List<Room> rooms, HashSet<Prim.Edge> edges)
    {
        Pathfinder2D aStar = new Pathfinder2D(new Vector2(GridWidth, GridHeight));

        foreach (var edge in mst)
        {
            var startRoom = (edge.U as Graphs.Vertex<Room>).Item;
            var endRoom = (edge.V as Graphs.Vertex<Room>).Item;

            var startPosf = startRoom.rect.Position + startRoom.rect.Size / 2;
            var endPosf = endRoom.rect.Position + endRoom.rect.Size / 2;
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
                else if (grid[b.Position] == CellType.None)
                {
                    pathCost.cost += 5;
                }
                else if (grid[b.Position] == CellType.Hallway)
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

                    if (grid[current] == CellType.None)
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


        // AStar2D aStar2D = new AStar2D();
        // aStar2D.ReserveSpace(GridWidth * GridHeight);

        // for (int y = 0; y < GridHeight; ++y)
        // {
        //     for (int x = 0; x < GridWidth; ++x)
        //     {
        //         bool foundRoom = false;

        //         foreach (Room room in rooms)
        //         {
        //             if (x >= room.rect.Position.x && x <= room.rect.End.x &&
        //             y >= room.rect.Position.y && y <= room.rect.End.y)
        //             {
        //                 foundRoom = true;
        //                 break;
        //             }
        //         }

        //         if (!foundRoom)
        //         {
        //             int tileId = aStar2D.GetAvailablePointId();
        //             Vector2 tilePosition = new Vector2(x, y);
        //             aStar2D.AddPoint(tileId, tilePosition);

        //             Vector2[] checks = {
        //               new Vector2(-1, 0),
        //               new Vector2(1, 0),
        //               new Vector2(0, -1),
        //               new Vector2(0, 1)
        //             };

        //             for (int i = 0; i < checks.Length; ++i)
        //             {
        //                 Vector2 probePosition = tilePosition + checks[i];
        //                 int probePoint = aStar2D.GetClosestPoint(probePosition);

        //                 if (probePoint != -1 && !aStar2D.ArePointsConnected(tileId, probePoint))
        //                 {
        //                     aStar2D.ConnectPoints(tileId, probePoint);
        //                 }
        //             }
        //         }
        //     }
        // }

        // foreach (Prim.Edge edge in edges)
        // {
        //     int pointA = aStar2D.GetClosestPoint(edge.U.Position);
        //     int pointB = aStar2D.GetClosestPoint(edge.V.Position);

        //     Vector2[] path = aStar2D.GetPointPath(pointA, pointB);
        //     if (path != null && path.Length > 0)
        //     {
        //         foreach (Vector2 point in path)
        //         {
        //             data[(int)point.x, (int)point.y] = CellType.Empty;
        //         }
        //     }
        // }
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
                bool north = false;

                if (y > 0)
                {
                    north = dataCopy[x, y - 1] == CellType.None;
                }

                bool south = false;

                if (y < GridHeight - 1)
                {
                    south = dataCopy[x, y + 1] == CellType.None;
                }

                bool east = false;

                if (x < GridWidth - 1)
                {
                    east = dataCopy[x + 1, y] == CellType.None;
                }

                bool west = false;

                if (x > 0)
                {
                    west = dataCopy[x - 1, y] == CellType.None;
                }

                if ((north && south && east && west) || x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1)
                {
                    data[x, y] = CellType.Empty;
                }
            }
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
        public Rect2 rect;

        public Room(int x, int y, int width, int height)
        {
            rect = new Rect2(x, y, width, height);
        }
    }
}
