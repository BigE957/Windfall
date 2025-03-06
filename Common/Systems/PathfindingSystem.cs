using CalamityMod.Items.Accessories;
using static Terraria.Utilities.NPCUtils;

namespace Windfall.Common.Systems;
public class PathfindingSystem : ModSystem
{
    // Pathfinding System based on theses videos' implementation of the A* algorithim:
    // https://www.youtube.com/watch?v=i0x5fj4PqP4
    // https://www.youtube.com/watch?v=alU04hvz6L4
    // Give em a watch!


    private static Node[][] NodeMap;
    private static readonly int TileScalar = 2;
    private static readonly Point[] Dirs =
    [
        new(0, TileScalar), new(-TileScalar, 0), new(0, -TileScalar), new(TileScalar, 0),
        new(TileScalar, TileScalar), new(TileScalar, -TileScalar), new(-TileScalar, -TileScalar), new(-TileScalar, TileScalar)
    ];

    public override void OnWorldLoad()
    {
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
    }

    public override void OnWorldUnload()
    {
        NodeMap = null;
    }

    public class Node(Point position) : IComparable
    {
        public Point TilePosition { get; } = position;
        public Vector2 WorldPosition { get; } = position.ToWorldCoordinates();

        public int X => TilePosition.X;
        public int Y => TilePosition.Y;

        public Node[] Neighbors;
        public Node Connection;
        public float G = float.MaxValue;
        public float F = float.MaxValue;

        public void SetConnection(Node node) => Connection = node;
        public void SetG(float g) => G = g;
        public void SetF(float f) => F = f;

        public int GetDistance(int targetX, int targetY)
        {
            int dx = Math.Abs(X - targetX);
            int dy = Math.Abs(Y - targetY);
            return (dx > dy) ? (14 * dy + 10 * (dx - dy)) : (14 * dx + 10 * (dy - dx));
        }

        public void InitNeighbors()
        {
            // Pre-allocate array of exact size based on valid neighbors
            List<Node> validNeighbors = new(8);

            foreach (Point p in Dirs)
            {
                int nx = TilePosition.X + p.X;
                int ny = TilePosition.Y + p.Y;

                if (WorldGen.InWorld(nx, ny))
                {
                    validNeighbors.Add(NodeMap[nx][ny]);
                }
            }

            Neighbors = [.. validNeighbors];
        }

        public void ResetState()
        {
            G = float.MaxValue;
            F = float.MaxValue;
            Connection = null;
        }

        // IComparable implementation directly in the class
        public int CompareTo(object other)
        {
            if (other == null || other is not Node)
                return 0;
            Node node = (Node)other;
            if (F != node.F)
                return F.CompareTo(node.F);

            return G.CompareTo(node.G);
        }
    }

    public class Path(Point[] nodes)
    {
        public Point[] Points { get; } = nodes;

        public void DrawPath(SpriteBatch sb)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                sb.DrawLineBetter(Points[i].ToWorldCoordinates(), Points[i + 1].ToWorldCoordinates(), Color.Red, 4);
            }
        }
    }

    public class FastPriorityQueue<T> where T : class
    {
        private struct QueueItem
        {
            public T Item;
            public float Priority;
        }

        private QueueItem[] _heap;
        private Dictionary<T, int> _itemToIndex;
        private int _count;

        public int Count => _count;

        public FastPriorityQueue(int capacity)
        {
            _heap = new QueueItem[capacity];
            _itemToIndex = new Dictionary<T, int>(capacity);
            _count = 0;
        }

        public bool Contains(T item)
        {
            return _itemToIndex.ContainsKey(item);
        }

        public void Enqueue(T item, float priority)
        {
            if (_count == _heap.Length)
                Resize(_count * 2);

            _heap[_count] = new QueueItem { Item = item, Priority = priority };
            _itemToIndex[item] = _count;
            BubbleUp(_count);
            _count++;
        }

        public T Dequeue()
        {
            if (_count == 0)
                throw new InvalidOperationException("Queue is empty");

            T result = _heap[0].Item;
            _count--;

            _heap[0] = _heap[_count];
            _itemToIndex[_heap[0].Item] = 0;
            _itemToIndex.Remove(result);

            if (_count > 0)
                BubbleDown(0);

            return result;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            if (!_itemToIndex.TryGetValue(item, out int index))
                throw new InvalidOperationException("Item not found in queue");

            float oldPriority = _heap[index].Priority;
            _heap[index].Priority = newPriority;

            if (newPriority < oldPriority)
                BubbleUp(index);
            else if (newPriority > oldPriority)
                BubbleDown(index);
        }

        public void Clear()
        {
            Array.Clear(_heap);
            _itemToIndex.Clear();
            _count = 0;
        }

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_heap[index].Priority >= _heap[parentIndex].Priority)
                    break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void BubbleDown(int index)
        {
            while (true)
            {
                int leftChild = index * 2 + 1;
                int rightChild = index * 2 + 2;
                int smallest = index;

                if (leftChild < _count && _heap[leftChild].Priority < _heap[smallest].Priority)
                    smallest = leftChild;

                if (rightChild < _count && _heap[rightChild].Priority < _heap[smallest].Priority)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            QueueItem temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;

            _itemToIndex[_heap[i].Item] = i;
            _itemToIndex[_heap[j].Item] = j;
        }

        private void Resize(int newSize)
        {
            Array.Resize(ref _heap, newSize);
        }
    }

    public class PathFinding
    {
        public Path MyPath { get; private set; } = null;

        private readonly FastPriorityQueue<Node> OpenSet = new(10000);
        private readonly HashSet<Point> ClosedSet = new(4096);
        private readonly HashSet<Node> ModifiedNodes = new(100);

        public void FindPath(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, float searchRadius = 1225)
        {
            float radiusSquared = searchRadius * searchRadius;

            Point startTile = startWorld.ToTileCoordinates();
            Point targetTile = targetWorld.ToTileCoordinates();

            // Quick early exit for out-of-bounds
            if (!WorldGen.InWorld(startTile.X, startTile.Y) || !WorldGen.InWorld(targetTile.X, targetTile.Y))
            {
                MyPath = null;
                return;
            }

            Node startNode = NodeMap[startTile.X + startTile.X % 2][startTile.Y + startTile.Y % 2];

            int cachedTargetX = targetTile.X + (targetTile.X % 2 == 0 ? 0 : 1);
            int cachedTargetY = targetTile.Y + (targetTile.Y % 2 == 0 ? 0 : 1);

            if (startNode.GetDistance(cachedTargetX, cachedTargetY) > searchRadius)
            {
                MyPath = null;
                return;
            }

            if (startTile == targetTile)
            {
                MyPath = new Path([]);
                return;
            }

            startNode.ResetState();
            startNode.SetG(0);
            startNode.SetF(startNode.GetDistance(cachedTargetX, cachedTargetY));

            const int maxIterations = 5000;
            int iterations = 0;

            OpenSet.Clear();
            ClosedSet.Clear();
            ModifiedNodes.Clear();

            OpenSet.Enqueue(startNode, startNode.F);

            while (OpenSet.Count > 0 && iterations < maxIterations)
            {
                Node current = OpenSet.Dequeue();

                //Particle p = new GlowOrbParticle(current.TilePosition.ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, Color.Red);
                //GeneralParticleHandler.SpawnParticle(p);

                int distanceToTarget = current.GetDistance(cachedTargetX, cachedTargetY);
                if (distanceToTarget <= 20 && (targetWorld - current.WorldPosition).LengthSquared() < 800)
                {
                    Main.NewText("Iteration Count: " + iterations);
                    MyPath = ReconstructPath(current, startNode);
                    ClearNodeStates(ModifiedNodes);
                    return;
                }

                ClosedSet.Add(current.TilePosition);

                foreach (Node neighbor in current.Neighbors)
                {
                    // Fast fail for invalid neighbors
                    if (!isWalkable(current.TilePosition, neighbor.TilePosition))
                        continue;

                    // World distance check before processing
                    if ((targetWorld - neighbor.WorldPosition).LengthSquared() > radiusSquared)
                        continue;

                    // Skip already processed nodes
                    if (ClosedSet.Contains(neighbor.TilePosition))
                        continue;

                    float tentativeG = current.G + current.GetDistance(neighbor.X, neighbor.Y);

                    // Update if better path found
                    if (tentativeG < neighbor.G)
                    {
                        ModifiedNodes.Add(neighbor);
                        neighbor.SetConnection(current);
                        neighbor.SetG(tentativeG);

                        float newF = tentativeG + neighbor.GetDistance(cachedTargetX, cachedTargetY);
                        neighbor.SetF(newF);

                        // Enqueue with new priority
                        OpenSet.Enqueue(neighbor, newF);
                    }
                }
                iterations++;
            }

            Main.NewText("Iteration Count: " + iterations);
            ClearNodeStates(ModifiedNodes);
            MyPath = null;
            return;
        }

        public static bool StandardIsWalkable(Point fromPoint, Point toPoint)
        {
            //return !Framing.GetTileSafely(toPoint.X, toPoint.Y).IsTileSolid();
            if (Framing.GetTileSafely(toPoint.X, toPoint.Y).IsTileSolid())
                return false;

            int dirX = (toPoint.X - fromPoint.X) / 2;
            int dirY = (toPoint.Y - fromPoint.Y) / 2;

            int midX = fromPoint.X + dirX;
            int midY = fromPoint.Y + dirY;

            if (Framing.GetTileSafely(midX, midY).IsTileSolid())
                return false;

            if (dirX != 0 && dirY != 0)
            {
                if (Framing.GetTileSafely(fromPoint.X + dirX, fromPoint.Y).IsTileSolid())
                    return false;

                if (Framing.GetTileSafely(fromPoint.X, fromPoint.Y + dirY).IsTileSolid())
                    return false;
            }
            return true;

        }

        private static void ClearNodeStates(HashSet<Node> nodes)
        {
            foreach (Node node in nodes)
                node.ResetState();
            nodes.Clear();
        }

        private static Path ReconstructPath(Node endNode, Node startNode)
        {
            // Estimate path length to avoid resizing
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

            // Convert to array for better performance
            Point[] pathArray = [.. path];
            Array.Reverse(pathArray);

            return new Path(pathArray);
        }
    }
}
