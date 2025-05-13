using CalamityMod.Items.Weapons.Ranged;
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


    private static Node[][] NodeMap;
    public static readonly Point[] Dirs =
    [
        new(0, 1), new(-1, 0), new(0, -1), new(1, 0),
        new(1, 1), new(1, -1), new(-1, -1), new(-1, 1)
    ];

    public override void OnWorldLoad()
    {
        int MaxX = Main.maxTilesX;
        int MaxY = Main.maxTilesY;

        NodeMap = new Node[MaxX][];
        Parallel.For(0, MaxX, x => {
            NodeMap[x] = new Node[MaxY];
        });

        int processorCount = Environment.ProcessorCount;

        // Use square chunks for better cache efficiency
        int chunkSize = (int)Math.Sqrt(MaxX * MaxY / (processorCount * 4));
        chunkSize = Math.Max(16, chunkSize); // Ensure reasonable chunk size

        // The number of chunks in each dimension
        int numChunksX = (MaxX + chunkSize - 1) / chunkSize;
        int numChunksY = (MaxY + chunkSize - 1) / chunkSize;
        int totalChunks = numChunksX * numChunksY;

        // Create nodes in chunks
        Parallel.For(0, totalChunks, chunkIndex => {
            // Calculate this chunk's bounds
            int chunkX = chunkIndex % numChunksX;
            int chunkY = chunkIndex / numChunksX;

            int startX = chunkX * chunkSize;
            int startY = chunkY * chunkSize;
            int endX = Math.Min(startX + chunkSize, MaxX);
            int endY = Math.Min(startY + chunkSize, MaxY);

            // Process this chunk - good cache locality
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    NodeMap[x][y] = new Node(new Point(x, y));
                }
            }
        });

        // Initialize neighbors with the same chunking strategy
        Parallel.For(0, totalChunks, chunkIndex => {
            // Calculate this chunk's bounds
            int chunkX = chunkIndex % numChunksX;
            int chunkY = chunkIndex / numChunksX;

            int startX = chunkX * chunkSize;
            int startY = chunkY * chunkSize;
            int endX = Math.Min(startX + chunkSize, MaxX);
            int endY = Math.Min(startY + chunkSize, MaxY);

            // Process neighbor initialization in the same chunk
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    // Use the faster neighbor initialization for a completely filled grid
                    // This approach is much faster for a dense grid
                    NodeMap[x][y].InitNeighborsFast();
                }
            }
        });


        /*
        NodeMap = new Node[Main.maxTilesX][];
        for (int x = 0; x < Main.maxTilesX; x += TileScalar)
        {
            NodeMap[x] = new Node[Main.maxTilesY];
            for (int y = 0; y < Main.maxTilesY; y += TileScalar)
            {
                NodeMap[x][y] = new Node(new Point(x, y));
            }
        }
        for (int x = 0; x < Main.maxTilesX; x += TileScalar)
        {
            for (int y = 0; y < Main.maxTilesY; y += TileScalar)
            {
                NodeMap[x][y].InitNeighbors();
            }
        }
        */
    }

    public override void OnWorldUnload()
    {
        NodeMap = null;
    }

    public class Node(Point position) : IComparable<Node>
    {
        public Point TilePosition { get; } = position;
        public Vector2 WorldPosition { get; } = position.ToWorldCoordinates();

        public int X => TilePosition.X;
        public int Y => TilePosition.Y;

        public Node[] Neighbors;
        public Node Connection;

        public int G = int.MaxValue;
        public int F = int.MaxValue;

        [NonSerialized]
        public int QueueIndex = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetConnection(Node node) => Connection = node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetG(int g) => G = g;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetF(int f) => F = f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDistance(int targetX, int targetY)
        {
            int dx = Math.Abs(X - targetX);
            int dy = Math.Abs(Y - targetY);
            int min = Math.Min(dx, dy);
            return 14 * min + 10 * (dx + dy - 2 * min);
        }

        public void InitNeighbors()
        {
            List<Node> validNeighbors = new(8);

            foreach (Point p in Dirs)
            {
                int nx = TilePosition.X + p.X;
                int ny = TilePosition.Y + p.Y;

                if (WorldGen.InWorld(nx, ny))
                    validNeighbors.Add(NodeMap[nx][ny]);
            }

            Neighbors = [.. validNeighbors];
        }

        public void InitNeighborsFast()
        {
            Neighbors = new Node[8];
            int count = 0;

            // Unroll the loop for maximum performance
            // Check top-left
            if (X > 0 && Y > 0)
                Neighbors[count++] = NodeMap[X - 1][Y - 1];

            // Check top
            if (Y > 0)
                Neighbors[count++] = NodeMap[X][Y - 1];

            // Check top-right
            if (X < Main.maxTilesX - 1 && Y > 0)
                Neighbors[count++] = NodeMap[X + 1][Y - 1];

            // Check left
            if (X > 0)
                Neighbors[count++] = NodeMap[X - 1][Y];

            // Check right
            if (X < Main.maxTilesX - 1)
                Neighbors[count++] = NodeMap[X + 1][Y];

            // Check bottom-left
            if (X > 0 && Y < Main.maxTilesY - 1)
                Neighbors[count++] = NodeMap[X - 1][Y + 1];

            // Check bottom
            if (Y < Main.maxTilesY - 1)
                Neighbors[count++] = NodeMap[X][Y + 1];

            // Check bottom-right
            if (X < Main.maxTilesX - 1 && Y < Main.maxTilesY - 1)
                Neighbors[count++] = NodeMap[X + 1][Y + 1];

            // Resize array if needed (only for edge cases)
            if (count < 8)
            {
                var resized = new Node[count];
                Array.Copy(Neighbors, resized, count);
                Neighbors = resized;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetState()
        {
            G = int.MaxValue;
            F = int.MaxValue;
            Connection = null;
        }

        public int CompareTo(Node other)
        {
            if (other == null)
                return 1;

            return (F != other.F) ? F.CompareTo(other.F) : G.CompareTo(other.G);
        }
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
        private readonly Dictionary<Node, int> itemToIndex = new(capacity);
        private int count = 0;

        public int Count => count;

        public bool Contains(Node item) => itemToIndex.ContainsKey(item);

        public void Enqueue(Node item, float priority)
        {
            if (count == heap.Length)
                Resize(heap.Length * 2);

            heap[count] = new QueueItem { Item = item, Priority = priority };
            itemToIndex[item] = count;
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
                itemToIndex[heap[0].Item] = 0;
            }

            itemToIndex.Remove(result);

            if (count > 0)
                SortDown(0);

            return result;
        }

        public void UpdatePriority(Node item, float newPriority)
        {
            if (!itemToIndex.TryGetValue(item, out int index))
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
            itemToIndex.Clear();
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
                itemToIndex[heap[index].Item] = index;
                index = parentIndex;
            }

            heap[index] = item;
            itemToIndex[item.Item] = index;
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
                itemToIndex[heap[index].Item] = index;
                index = smallestChild;
            }

            heap[index] = item;
            itemToIndex[item.Item] = index;
        }

        private void Resize(int newSize) => Array.Resize(ref heap, newSize);
    }

    public class PathFinding
    {
        public FoundPath MyPath { get; private set; } = null;

        private readonly PriorityQueue OpenSet = new(4096);
        private readonly HashSet<Point> ClosedSet = new(4096);
        private readonly HashSet<Node> ModifiedNodes = new(100);

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
            ClearNodeStates(ModifiedNodes);
            ModifiedNodes.Clear();
            OpenSet.Clear();
            ClosedSet.Clear();

            Point StartPoint = startWorld.ToTileCoordinates();
            Point TargetPoint = targetWorld.ToTileCoordinates();

            Node startNode = NodeMap[StartPoint.X][StartPoint.Y];

            int startToTarget = startNode.GetDistance(TargetPoint.X, TargetPoint.Y);

            startNode.ResetState();
            startNode.SetG(0);
            startNode.SetF(startToTarget);
            ModifiedNodes.Add(startNode);

            const int maxIterations = 4096 * 2;
            int iterations = 0;

            OpenSet.Enqueue(startNode, startNode.F);

            while (OpenSet.Count > 0 && iterations < maxIterations)
            {
                Node current = OpenSet.Dequeue();

                //Particle p = new GlowOrbParticle(current.TilePosition.ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, Color.Red);
                //GeneralParticleHandler.SpawnParticle(p);

                int distanceToTarget = current.GetDistance(TargetPoint.X, TargetPoint.Y);
                if (distanceToTarget <= 20 && (targetWorld - current.WorldPosition).LengthSquared() < 800)
                {
                    Main.NewText("Iteration Count: " + iterations);
                    MyPath = ReconstructPath(current, startNode);
                    //Main.NewText("Path Length: " + MyPath.Points.Length);
                    ClearNodeStates(ModifiedNodes);
                    return;
                }

                ClosedSet.Add(current.TilePosition);

                foreach (Node neighbor in current.Neighbors)
                {
                    if (ClosedSet.Contains(neighbor.TilePosition))
                        continue;

                    if (!isWalkable(current.TilePosition, neighbor.TilePosition))
                        continue;

                    if (!isValid.Invoke(neighbor.TilePosition))
                        continue;

                    int tentativeG = current.G + current.GetDistance(neighbor.X, neighbor.Y) + (costFunction == null ? 0 : costFunction(neighbor.TilePosition));

                    if (tentativeG < neighbor.G)
                    {
                        ModifiedNodes.Add(neighbor);
                        neighbor.SetConnection(current);
                        neighbor.SetG(tentativeG);

                        int newF = tentativeG + neighbor.GetDistance(TargetPoint.X, TargetPoint.Y);
                        neighbor.SetF(newF);

                        if (OpenSet.Contains(neighbor))
                            OpenSet.UpdatePriority(neighbor, newF);
                        else
                            OpenSet.Enqueue(neighbor, newF);
                    }
                }
                iterations++;
            }

            ClearNodeStates(ModifiedNodes);
            MyPath = null;
            return;
        }

        private static void ClearNodeStates(HashSet<Node> nodes)
        {
            foreach (Node node in nodes)
                node.ResetState();
            nodes.Clear();
        }

        private static FoundPath ReconstructPath(Node endNode, Node startNode)
        {
            int estimatedLength = (int)(Math.Sqrt(
                Math.Pow(endNode.X - startNode.X, 2) +
                Math.Pow(endNode.Y - startNode.Y, 2)
            )) + 10;

            List<Point> path = new(estimatedLength);
            Node current = endNode;

            while (current != null && current != startNode)
            {
                path.Add(current.TilePosition);
                current = current.Connection;
            }
            path.Add(current.TilePosition);

            Point[] pathArray = [.. path];
            Array.Reverse(pathArray);

            return new FoundPath(pathArray);
        }
    }
}
