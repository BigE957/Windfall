﻿using CalamityMod.Particles;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Windfall.Common.Systems;
public class PathfindingSystem : ModSystem
{
    // Pathfinding System based on theses videos' implementation of the A* algorithim:
    // https://www.youtube.com/watch?v=i0x5fj4PqP4
    // https://www.youtube.com/watch?v=alU04hvz6L4
    // Give em a watch!


    private static Node[][] NodeMap;
    public static readonly Point16[] Dirs =
    [
        new(-1, -1), // 0: Top-left
        new(0, -1),  // 1: Top
        new(1, -1),  // 2: Top-right
        new(-1, 0),  // 3: Left
        new(1, 0),   // 4: Right
        new(-1, 1),  // 5: Bottom-left
        new(0, 1),   // 6: Bottom
        new(1, 1)    // 7: Bottom-right
    ];

    public override void OnWorldLoad()
    {
        Windfall.Instance.Logger.Debug("Beginning Pathfinding Initialization.");
        ushort MaxX = (ushort)Main.maxTilesX; // 6400 or 8400
        ushort MaxY = (ushort)Main.maxTilesY; // 1800 or 2400
        NodeMap = new Node[MaxX][];

        // Configuration for optimal parallelism with large grids
        int processorCount = Environment.ProcessorCount;

        Windfall.Instance.Logger.Debug("System processor count: " + processorCount);

        Windfall.Instance.Logger.Debug("Beginning NodeMap array initialization.");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // Step 1: Initialize the arrays in parallel with custom partitioning
        // Store the task for proper synchronization
        Task initNodesTask = Task.Run(() =>
        {
            Parallel.For(0, MaxX, new ParallelOptions { MaxDegreeOfParallelism = processorCount }, x =>
            {
                NodeMap[x] = new Node[MaxY];

                // Create nodes in bulk for each column
                for (ushort y = 0; y < MaxY; y++)
                    NodeMap[x][y] = new Node((ushort)x, y);
            });
        });

        // Ensure the first step completes before proceeding
        initNodesTask.Wait();
        watch.Stop();
        Windfall.Instance.Logger.Debug("NodeMap array initialized in " + watch.ElapsedMilliseconds + "ms");

        Windfall.Instance.Logger.Debug("Beginning Neighbor initialization.");
        watch = System.Diagnostics.Stopwatch.StartNew();

        // Step 2: Initialize neighbors in parallel with improved partitioning
        Task initNeighborsTask = Task.Run(() =>
        {
            Parallel.For(0, MaxX, new ParallelOptions { MaxDegreeOfParallelism = processorCount }, x =>
            {
                // Cache the column to improve memory access patterns
                Node[] column = NodeMap[x];

                // Pre-calculate frequently accessed values
                int maxXIdx = MaxX - 1;
                int maxYIdx = MaxY - 1;

                for (int y = 0; y < MaxY; y++)
                {
                    // For struct types we need to get a copy, modify it, and place it back
                    Node node = NodeMap[x][y];
                    node.InitNeighbors(maxXIdx, maxYIdx);
                    NodeMap[x][y] = node;
                }
            });
        });

        initNeighborsTask.Wait();
        watch.Stop();
        Windfall.Instance.Logger.Debug("Neighbors initialized in" + watch.ElapsedMilliseconds + "ms");
    }

    public override void OnWorldUnload()
    {
        for (int i = 0; i < NodeMap.Length; i++)
            NodeMap[i] = null;
        NodeMap = null;
    }

    public struct Node(ushort x, ushort y) : IComparable<Node>
    {
        public readonly ushort X = x;
        public readonly ushort Y = y;

        public Point TilePosition => new(X, Y);

        public byte[] NeighborLocations = new byte[8];
        public byte NeighborCount = 0;
        public Point16 ConnectionLocation = new(-1, -1);

        public int G = int.MaxValue;
        public int F = int.MaxValue;

        [NonSerialized]
        public int QueueIndex = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConnection(Point16 p) => ConnectionLocation = p;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetG(int g) => G = g;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetF(int f) => F = f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetDistance(int targetX, int targetY)
        {
            int dx = Math.Abs(X - targetX);
            int dy = Math.Abs(Y - targetY);
            int min = Math.Min(dx, dy);
            // For 10*x and 14*y, we use optimized multiplications
            // 10*x = 8*x + 2*x = x<<3 + x<<1
            // 14*y = 16*y - 2*y = y<<4 - y<<1
            return (min << 4) - (min << 1) + ((dx + dy - 2 * min) << 3) + ((dx + dy - 2 * min) << 1);
        }

        public void InitNeighbors(int maxX, int maxY)
        {
            NeighborCount = 0;

            // Boundary checks using pre-calculated values passed into the method
            bool hasLeft = X > 0;
            bool hasRight = X < maxX;
            bool hasTop = Y > 0;
            bool hasBottom = Y < maxY;

            // Top-left
            if (hasLeft && hasTop)
                NeighborLocations[NeighborCount++] = 0;
            // Top
            if (hasTop)
                NeighborLocations[NeighborCount++] = 1;
            // Top-right
            if (hasRight && hasTop)
                NeighborLocations[NeighborCount++] = 2;
            // Left
            if (hasLeft)
                NeighborLocations[NeighborCount++] = 3;
            // Right
            if (hasRight)
                NeighborLocations[NeighborCount++] = 4;
            // Bottom-left
            if (hasLeft && hasBottom)
                NeighborLocations[NeighborCount++] = 5;
            // Bottom
            if (hasBottom)
                NeighborLocations[NeighborCount++] = 6;
            // Bottom-right
            if (hasRight && hasBottom)
                NeighborLocations[NeighborCount++] = 7;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetState()
        {
            G = int.MaxValue;
            F = int.MaxValue;
            ConnectionLocation = new(-1, -1);
        }

        public readonly int CompareTo(Node other) => (F != other.F) ? F.CompareTo(other.F) : G.CompareTo(other.G);
    }

    public class FoundPath(Point[] nodes)
    {
        public Point[] Points { get; } = nodes;

        public void DrawPath(SpriteBatch sb)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                sb.DrawLineBetween(Points[i].ToWorldCoordinates(), Points[i + 1].ToWorldCoordinates(), Color.Red, 4);
                Particle p = new GlowOrbParticle(Points[i].ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, Color.Red);
                GeneralParticleHandler.SpawnParticle(p);
            }
        }
    }

    public class PriorityQueue(int capacity = 16)
    {
        private struct QueueItem
        {
            public Node Item;
            public float Priority;
        }

        private QueueItem[] heap = new QueueItem[capacity > 0 ? capacity : 16];
        private readonly Dictionary<Point16, int> itemPosToIndex = new(capacity);
        private int count = 0;

        public int Count => count;

        public bool Contains(Node item) => itemPosToIndex.ContainsKey(new Point16(item.X, item.Y));

        public void Enqueue(Node item, float priority)
        {
            if (count == heap.Length)
                Resize(heap.Length * 2);

            heap[count] = new QueueItem { Item = item, Priority = priority };
            itemPosToIndex[new Point16(item.X, item.Y)] = count;
            SortUp(count);
            count++;
        }

        public Node Dequeue()
        {
            if (count == 0)
                throw new InvalidOperationException("Queue is empty");

            Node result = heap[0].Item;
            count--;

            if (count > 0)
            {
                heap[0] = heap[count];
                itemPosToIndex[new Point16(heap[0].Item.X, heap[0].Item.Y)] = 0;
            }
            itemPosToIndex.Remove(new Point16(result.X, result.Y));

            if (count > 0)
                SortDown(0);

            return result;
        }

        public void UpdatePriority(Node item, float newPriority)
        {
            Point16 pos = new(item.X, item.Y);
            if (!itemPosToIndex.TryGetValue(pos, out int index))
                throw new InvalidOperationException("Item not found in queue");

            float oldPriority = heap[index].Priority;
            heap[index].Item = item;
            heap[index].Priority = newPriority;

            if (newPriority < oldPriority)
                SortUp(index);
            else if (newPriority > oldPriority)
                SortDown(index);
        }

        public void Clear()
        {
            Array.Clear(heap);
            itemPosToIndex.Clear();
            count = 0;
        }

        private void SortUp(int index)
        {
            QueueItem item = heap[index];
            Point16 itemPos = new(item.Item.X, item.Item.Y);

            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (item.Priority >= heap[parentIndex].Priority)
                    break;

                heap[index] = heap[parentIndex];
                itemPosToIndex[new Point16(heap[index].Item.X, heap[index].Item.Y)] = index;
                index = parentIndex;
            }

            heap[index] = item;
            itemPosToIndex[itemPos] = index;
        }

        private void SortDown(int index)
        {
            QueueItem item = heap[index];
            Point16 itemPos = new(item.Item.X, item.Item.Y);

            while (true)
            {
                int leftChild = index * 2 + 1;
                int rightChild = index * 2 + 2;

                if (leftChild >= count)
                    break;

                int smallestChild = (rightChild < count && heap[rightChild].Priority < heap[leftChild].Priority)
                    ? rightChild
                    : leftChild;

                if (item.Priority <= heap[smallestChild].Priority)
                    break;

                heap[index] = heap[smallestChild];
                itemPosToIndex[new Point16(heap[index].Item.X, heap[index].Item.Y)] = index;
                index = smallestChild;
            }

            heap[index] = item;
            itemPosToIndex[itemPos] = index;
        }

        private void Resize(int newSize) => Array.Resize(ref heap, newSize);
    }

    public class PathFinding
    {
        public FoundPath MyPath { get; private set; } = null;

        private readonly PriorityQueue OpenSet = new(4096);
        private readonly HashSet<Point16> ClosedSet = new(4096);
        private readonly HashSet<Point16> ModifiedNodesLocations = new(100);

        public void FindPathInRadius(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, int> costFunction = null, float searchRadius = 1225)
        {
            Point StartPoint = startWorld.ToTileCoordinates();
            Point TargetPoint = targetWorld.ToTileCoordinates();

            if (StartPoint == new Point(-1, -1) || TargetPoint == new Point(-1, -1))
            {
                MyPath = null;
                return;
            }

            if (StartPoint == TargetPoint)
            {
                MyPath = new FoundPath([StartPoint, TargetPoint]);
                return;
            }

            float radiusSquared = searchRadius * searchRadius;

            if (Vector2.DistanceSquared(targetWorld, startWorld) > radiusSquared)
            {
                MyPath = null;
                return;
            }

            FindPath(startWorld, targetWorld, isWalkable, new(p => Vector2.DistanceSquared(p.ToWorldCoordinates(), targetWorld) <= radiusSquared), costFunction);
        }

        public void FindPathInArea(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Rectangle searchArea, Func<Point, int> costFunction = null)
        {
            Point StartPoint = startWorld.ToTileCoordinates();
            Point TargetPoint = targetWorld.ToTileCoordinates();

            if (StartPoint == new Point(-1, -1) || TargetPoint == new Point(-1, -1))
            {
                MyPath = null;
                return;
            }

            if (StartPoint == TargetPoint)
            {
                MyPath = new FoundPath([StartPoint, TargetPoint]);
                return;
            }

            if (!searchArea.Contains(startWorld.ToTileCoordinates()) || !searchArea.Contains(targetWorld.ToTileCoordinates()))
            {
                MyPath = null;
                return;
            }

            FindPath(startWorld, targetWorld, isWalkable, searchArea.Contains, costFunction);
        }

        public void FindPath(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, bool> isValid, Func<Point, int> costFunction = null)
        {
            if (ModifiedNodesLocations.Count > 0)
                ClearNodeStates(ModifiedNodesLocations);

            OpenSet.Clear();
            ClosedSet.Clear();

            Point startPoint = startWorld.ToTileCoordinates();
            Point targetPoint = targetWorld.ToTileCoordinates();
            int targetX = targetPoint.X;
            int targetY = targetPoint.Y;

            ref Node startNode = ref NodeMap[startPoint.X][startPoint.Y];

            startNode.ResetState();
            startNode.SetG(0);
            startNode.SetF(startNode.GetDistance(targetPoint.X, targetPoint.Y));

            //NodeMap[startPoint.X][startPoint.Y] = startNode;
            
            ModifiedNodesLocations.Add(new(startNode.X, startNode.Y));

            const int maxIterations = 4096 * 2;
            int iterations = 0;

            Func<Point, int> cachedCostFunction = costFunction ?? (_ => 0);

            OpenSet.Enqueue(startNode, startNode.F);

            Point16 neighborLoc = new();
            Point neighborPoint = new();

            while (OpenSet.Count > 0 && iterations < maxIterations)
            {
                Node current = OpenSet.Dequeue();

                //Particle p = new GlowOrbParticle(current.TilePosition.ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, Color.Red);
                //GeneralParticleHandler.SpawnParticle(p);

                if (current.X == targetX && current.Y == targetY)
                {
                    Main.NewText("Iteration Count: " + iterations);
                    MyPath = ReconstructPath(current, startNode);
                    //Main.NewText("Path Length: " + MyPath.Points.Length);
                    ClearNodeStates(ModifiedNodesLocations);
                    return;
                }

                ClosedSet.Add(new(current.X, current.Y));

                for (int i = 0; i < current.NeighborCount; i++)
                {
                    byte dirIndex = current.NeighborLocations[i];
                    ref readonly Point16 dir = ref Dirs[dirIndex];

                    neighborLoc = new((short)(current.X + dir.X), (short)(current.Y + dir.Y));

                    if (ClosedSet.Contains(neighborLoc))
                        continue;

                    neighborPoint.X = neighborLoc.X;
                    neighborPoint.Y = neighborLoc.Y;

                    if (!isValid(neighborPoint))
                        continue;

                    if (!isWalkable(current.TilePosition, neighborPoint))
                        continue;

                    ref Node neighbor = ref NodeMap[neighborLoc.X][neighborLoc.Y];
                    /*
                    int moveCost = current.GetDistance(neighborLoc.X, neighborLoc.Y);
                    int additionalCost = cachedCostFunction(neighborPoint);
                    int tentativeG = current.G + moveCost + additionalCost;
                    */
                    int tentativeG = current.G + current.GetDistance(neighborLoc.X, neighborLoc.Y) + cachedCostFunction(neighborPoint);

                    if (tentativeG < neighbor.G)
                    {
                        ModifiedNodesLocations.Add(neighborLoc);
                        
                        neighbor.SetConnection(new Point16(current.X, current.Y));
                        neighbor.SetG(tentativeG);

                        int newF = tentativeG + neighbor.GetDistance(targetPoint.X, targetPoint.Y);
                        neighbor.SetF(newF);

                        //NodeMap[neighborLoc.X][neighborLoc.Y] = neighbor;

                        if (OpenSet.Contains(neighbor))
                            OpenSet.UpdatePriority(neighbor, newF);
                        else
                            OpenSet.Enqueue(neighbor, newF);
                    }
                }
                iterations++;
            }

            ClearNodeStates(ModifiedNodesLocations);
            MyPath = null;
        }

        private static void ClearNodeStates(HashSet<Point16> locations)
        {
            foreach (Point16 pos in locations)
                NodeMap[pos.X][pos.Y].ResetState();
            
            locations.Clear();
        }

        private static FoundPath ReconstructPath(Node endNode, Node startNode)
        {
            int estimatedLength = (int)(Math.Sqrt(
                Math.Pow(endNode.X - startNode.X, 2) +
                Math.Pow(endNode.Y - startNode.Y, 2)
            )) + 10;

            List<Point> path = new(estimatedLength);
            Node current = endNode;

            while (current.TilePosition != startNode.TilePosition)
            {
                path.Add(current.TilePosition);
                Point16 connLoc = current.ConnectionLocation;
                current = NodeMap[connLoc.X][connLoc.Y];
            }
            path.Add(current.TilePosition);

            Point[] pathArray = [.. path];
            Array.Reverse(pathArray);

            return new FoundPath(pathArray);
        }
    }
}
