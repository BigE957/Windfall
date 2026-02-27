using CalamityMod.Particles;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Windfall.Common.Systems;
public class PathfindingSystem : ModSystem
{
    // Pathfinding System based on theses videos' implementation of the A* algorithim:
    // https://www.youtube.com/watch?v=i0x5fj4PqP4
    // https://www.youtube.com/watch?v=alU04hvz6L4
    // Give em a watch!


    // --- Chunk Configuration ---
    // Each chunk covers a CHUNK_SIZE x CHUNK_SIZE tile region.
    // Larger values = fewer chunks, more memory per chunk load.
    // Smaller values = more chunks, finer-grained eviction.
    public const int CHUNK_SIZE = 64;

    // Maximum number of chunks to keep loaded simultaneously.
    // A 1225-tile-radius search area covers roughly a 39x39 chunk region at most,
    public const int MAX_LOADED_CHUNKS = 256;

    private static NodeChunk[][] ChunkMap;   // [chunkX][chunkY]
    private static int ChunkCountX;
    private static int ChunkCountY;
    private static int WorldMaxX;
    private static int WorldMaxY;

    // LRU eviction: tracks which chunks were accessed most recently
    private static readonly LinkedList<(int cx, int cy)> LruList = new();
    private static readonly Dictionary<(int, int), LinkedListNode<(int, int)>> LruIndex = new();
    private static int LoadedChunkCount = 0;

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

        WorldMaxX = Main.maxTilesX;
        WorldMaxY = Main.maxTilesY;

        ChunkCountX = (WorldMaxX + CHUNK_SIZE - 1) / CHUNK_SIZE;
        ChunkCountY = (WorldMaxY + CHUNK_SIZE - 1) / CHUNK_SIZE;

        ChunkMap = new NodeChunk[ChunkCountX][];
        for (int cx = 0; cx < ChunkCountX; cx++)
            ChunkMap[cx] = new NodeChunk[ChunkCountY];

        LruList.Clear();
        LruIndex.Clear();
        LoadedChunkCount = 0;

        Windfall.Instance.Logger.Debug(
            $"Chunk map initialized: {ChunkCountX}x{ChunkCountY} chunks " +
            $"({ChunkCountX * ChunkCountY} total, max {MAX_LOADED_CHUNKS} loaded at once). " +
            $"World size: {WorldMaxX}x{WorldMaxY}");
    }

    public override void OnWorldUnload()
    {
        if (ChunkMap == null)
            return;

        for (int cx = 0; cx < ChunkCountX; cx++)
        {
            if (ChunkMap[cx] != null)
            {
                for (int cy = 0; cy < ChunkCountY; cy++)
                    ChunkMap[cx][cy] = null;
                ChunkMap[cx] = null;
            }
        }

        ChunkMap = null;
        LruList.Clear();
        LruIndex.Clear();
        LoadedChunkCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Node GetNode(int worldX, int worldY)
    {
        int cx = worldX / CHUNK_SIZE;
        int cy = worldY / CHUNK_SIZE;
        NodeChunk chunk = GetOrLoadChunk(cx, cy);
        int localX = worldX - cx * CHUNK_SIZE;
        int localY = worldY - cy * CHUNK_SIZE;
        return ref chunk.Nodes[localX * chunk.Height + localY];
    }

    private static NodeChunk GetOrLoadChunk(int cx, int cy)
    {
        NodeChunk chunk = ChunkMap[cx][cy];
        if (chunk != null)
        {
            // Promote to most-recently-used
            var node = LruIndex[(cx, cy)];
            LruList.Remove(node);
            LruList.AddLast(node);
            return chunk;
        }

        // Evict least-recently-used chunk if we're at the cap
        if (LoadedChunkCount >= MAX_LOADED_CHUNKS)
            EvictLruChunk();

        // Load (generate) the new chunk
        chunk = new NodeChunk(cx, cy, WorldMaxX - 1, WorldMaxY - 1);
        ChunkMap[cx][cy] = chunk;
        LoadedChunkCount++;

        var lruNode = LruList.AddLast((cx, cy));
        LruIndex[(cx, cy)] = lruNode;

        return chunk;
    }

    private static void EvictLruChunk()
    {
        var lruNode = LruList.First;
        if (lruNode == null)
            return;

        (int cx, int cy) = lruNode.Value;
        LruList.RemoveFirst();
        LruIndex.Remove((cx, cy));

        ChunkMap[cx][cy] = null;
        LoadedChunkCount--;
    }

    public class NodeChunk
    {
        public readonly Node[] Nodes;
        public readonly int Width;   // actual tile width  (may be < CHUNK_SIZE at world edge)
        public readonly int Height;  // actual tile height (may be < CHUNK_SIZE at world edge)

        public NodeChunk(int chunkX, int chunkY, int maxWorldX, int maxWorldY)
        {
            int startX = chunkX * CHUNK_SIZE;
            int startY = chunkY * CHUNK_SIZE;

            Width = Math.Min(CHUNK_SIZE, maxWorldX + 1 - startX);
            Height = Math.Min(CHUNK_SIZE, maxWorldY + 1 - startY);

            Nodes = new Node[Width * Height];

            // Initialize all nodes in this chunk (neighbors computed at init time)
            for (int lx = 0; lx < Width; lx++)
            {
                for (int ly = 0; ly < Height; ly++)
                {
                    ushort wx = (ushort)(startX + lx);
                    ushort wy = (ushort)(startY + ly);
                    ref Node n = ref Nodes[lx * Height + ly];
                    n = new Node(wx, wy);
                    n.InitNeighbors(maxWorldX, maxWorldY);
                }
            }
        }
    }

    public unsafe struct Node(ushort x, ushort y) : IComparable<Node>
    {
        public readonly ushort X = x;
        public readonly ushort Y = y;

        public Point TilePosition => new(X, Y);

        public fixed byte NeighborLocations[8];
        public byte NeighborCount = 0;
        public Point16 ConnectionLocation = new(-1, -1);
        public int G = int.MaxValue;
        public int F = int.MaxValue;

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
            return (min << 4) - (min << 1) + ((dx + dy - 2 * min) << 3) + ((dx + dy - 2 * min) << 1);
        }

        public void InitNeighbors(int maxX, int maxY)
        {
            NeighborCount = 0;

            bool hasLeft = X > 0;
            bool hasRight = X < maxX;
            bool hasTop = Y > 0;
            bool hasBottom = Y < maxY;

            if (hasLeft && hasTop) NeighborLocations[NeighborCount++] = 0;
            if (hasTop) NeighborLocations[NeighborCount++] = 1;
            if (hasRight && hasTop) NeighborLocations[NeighborCount++] = 2;
            if (hasLeft) NeighborLocations[NeighborCount++] = 3;
            if (hasRight) NeighborLocations[NeighborCount++] = 4;
            if (hasLeft && hasBottom) NeighborLocations[NeighborCount++] = 5;
            if (hasBottom) NeighborLocations[NeighborCount++] = 6;
            if (hasRight && hasBottom) NeighborLocations[NeighborCount++] = 7;
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
            public Point16 Key;
            public float Priority;
        }

        private QueueItem[] heap = new QueueItem[capacity > 0 ? capacity : 16];
        private readonly Dictionary<Point16, int> keyToIndex = new(capacity);
        private int count = 0;

        public int Count => count;

        public bool Contains(Point16 key) => keyToIndex.ContainsKey(key);

        public void Enqueue(Point16 key, float priority)
        {
            if (count == heap.Length)
                Resize(heap.Length * 2);

            heap[count] = new QueueItem { Key = key, Priority = priority };
            keyToIndex[key] = count;
            SortUp(count);
            count++;
        }

        public Point16 Dequeue()
        {
            if (count == 0)
                throw new InvalidOperationException("Queue is empty");

            Point16 result = heap[0].Key;
            count--;

            if (count > 0)
            {
                heap[0] = heap[count];
                keyToIndex[heap[0].Key] = 0;
            }
            keyToIndex.Remove(result);

            if (count > 0)
                SortDown(0);

            return result;
        }

        public void UpdatePriority(Point16 key, float newPriority)
        {
            if (!keyToIndex.TryGetValue(key, out int index))
                throw new InvalidOperationException("Item not found in queue");

            float oldPriority = heap[index].Priority;
            heap[index].Priority = newPriority;

            if (newPriority < oldPriority)
                SortUp(index);
            else if (newPriority > oldPriority)
                SortDown(index);
        }

        public void Clear()
        {
            Array.Clear(heap);
            keyToIndex.Clear();
            count = 0;
        }

        private void SortUp(int index)
        {
            QueueItem item = heap[index];

            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (item.Priority >= heap[parentIndex].Priority)
                    break;

                heap[index] = heap[parentIndex];
                keyToIndex[heap[index].Key] = index;
                index = parentIndex;
            }

            heap[index] = item;
            keyToIndex[item.Key] = index;
        }

        private void SortDown(int index)
        {
            QueueItem item = heap[index];

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
                keyToIndex[heap[index].Key] = index;
                index = smallestChild;
            }

            heap[index] = item;
            keyToIndex[item.Key] = index;
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

        public unsafe void FindPath(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, bool> isValid, Func<Point, int> costFunction = null)
        {
            if (ModifiedNodesLocations.Count > 0)
                ClearNodeStates(ModifiedNodesLocations);

            OpenSet.Clear();
            ClosedSet.Clear();

            Point startPoint = startWorld.ToTileCoordinates();
            Point targetPoint = targetWorld.ToTileCoordinates();
            int targetX = targetPoint.X;
            int targetY = targetPoint.Y;

            ref Node startNode = ref GetNode(startPoint.X, startPoint.Y);
            Point16 startKey = new(startNode.X, startNode.Y);

            startNode.ResetState();
            startNode.SetG(0);
            startNode.SetF(startNode.GetDistance(targetX, targetY));

            ModifiedNodesLocations.Add(startKey);

            const int maxIterations = 4096 * 2;
            int iterations = 0;

            Func<Point, int> cachedCostFunction = costFunction ?? (_ => 0);

            OpenSet.Enqueue(startKey, startNode.F);

            Point16 neighborLoc = new();
            Point neighborPoint = new();

            while (OpenSet.Count > 0 && iterations < maxIterations)
            {
                Point16 currentKey = OpenSet.Dequeue();
                ref Node current = ref GetNode(currentKey.X, currentKey.Y);

                if (current.X == targetX && current.Y == targetY)
                {
                    Main.NewText("Iteration Count: " + iterations);
                    MyPath = ReconstructPath(current, startNode);
                    ClearNodeStates(ModifiedNodesLocations);
                    return;
                }

                ClosedSet.Add(currentKey);

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

                    ref Node neighbor = ref GetNode(neighborLoc.X, neighborLoc.Y);

                    int tentativeG = current.G + current.GetDistance(neighborLoc.X, neighborLoc.Y) + cachedCostFunction(neighborPoint);

                    if (tentativeG < neighbor.G)
                    {
                        ModifiedNodesLocations.Add(neighborLoc);

                        neighbor.SetConnection(new Point16(current.X, current.Y));
                        neighbor.SetG(tentativeG);

                        int newF = tentativeG + neighbor.GetDistance(targetX, targetY);
                        neighbor.SetF(newF);

                        if (OpenSet.Contains(neighborLoc))
                            OpenSet.UpdatePriority(neighborLoc, newF);
                        else
                            OpenSet.Enqueue(neighborLoc, newF);
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
                GetNode(pos.X, pos.Y).ResetState();

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
                current = GetNode(connLoc.X, connLoc.Y);
            }
            path.Add(current.TilePosition);

            Point[] pathArray = [.. path];
            Array.Reverse(pathArray);

            return new FoundPath(pathArray);
        }
    }
}