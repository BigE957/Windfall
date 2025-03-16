using CalamityMod.Items.Weapons.Ranged;

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

        public int G = int.MaxValue;
        public int F = int.MaxValue;

        [NonSerialized]
        public int QueueIndex = -1;

        public void SetConnection(Node node) => Connection = node;
        public void SetG(int g) => G = g;
        public void SetF(int f) => F = f;

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

        public void ResetState()
        {
            G = int.MaxValue;
            F = int.MaxValue;
            Connection = null;
        }

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

    public class FoundPath(Point[] nodes)
    {
        public Point[] Points { get; } = nodes;

        public int Length { get => Points.Length; }

        public void DrawPath(SpriteBatch sb)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                sb.DrawLineBetter(Points[i].ToWorldCoordinates(), Points[i + 1].ToWorldCoordinates(), Color.Red, 4);
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

        public static Point FindNearestValidNode(Vector2 WorldPos, Func<Point, Point, bool> isWalkable)
        {
            Point NodePos = WorldPos.ToTileCoordinates();
            NodePos.X += NodePos.X % 2;
            NodePos.Y += NodePos.Y % 2;

            //Dust.NewDustPerfect(NodePos.ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);

            Single dist = 999;
            if (WorldGen.InWorld(NodePos.X, NodePos.Y, 20) && !Framing.GetTileSafely(NodePos.X, NodePos.Y).IsTileSolid())
                Vector2.Distance(NodePos.ToWorldCoordinates(), WorldPos);

            //Dust.NewDustPerfect(WorldPos, DustID.Electric, Vector2.Zero).noGravity = true;

            for (int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    Point checkPoint = NodePos + new Point(x * TileScalar, y * TileScalar);
                    //Dust.NewDustPerfect(checkPoint.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero);
                    if (WorldGen.InWorld(NodePos.X, NodePos.Y) && !Framing.GetTileSafely(checkPoint.X, checkPoint.Y).IsTileSolid() && Collision.CanHit(WorldPos, 1, 1, checkPoint.ToWorldCoordinates(), 1, 1))
                    {
                        Single newDist = Vector2.Distance(WorldPos, checkPoint.ToWorldCoordinates());
                        if (newDist < dist)
                        {
                            dist = newDist;
                            NodePos = checkPoint;
                        }
                    }
                }
            }

            //Dust.NewDustPerfect(NodePos.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
            if (dist == 999)
                return new Point(-1, -1);
            return NodePos; //Failed to find a Node, give up.
        }

        public void FindPath(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, int> costFunction = null, float searchRadius = 1225)
        {
            ClearNodeStates(ModifiedNodes);
            ModifiedNodes.Clear();
            OpenSet.Clear();
            ClosedSet.Clear();

            float radiusSquared = searchRadius * searchRadius;

            Point StartPoint = FindNearestValidNode(startWorld, isWalkable);
            Point TargetPoint = FindNearestValidNode(targetWorld, isWalkable);

            if (StartPoint == new Point(-1, -1) || TargetPoint == new Point(-1, -1))
            {
                MyPath = null;
                return;
            }

            if (StartPoint == TargetPoint)
            {
                MyPath = new FoundPath([]);
                return;
            }

            Node startNode = NodeMap[StartPoint.X + StartPoint.X % 2][StartPoint.Y + StartPoint.Y % 2];

            int startToTarget = startNode.GetDistance(TargetPoint.X, TargetPoint.Y);

            if (startToTarget > searchRadius)
            {
                MyPath = null;
                return;
            }

            startNode.ResetState();
            startNode.SetG(0);
            startNode.SetF(startToTarget);
            ModifiedNodes.Add(startNode);

            const int maxIterations = 5000;
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
                    //Main.NewText("Iteration Count: " + iterations);
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

                    if ((targetWorld - neighbor.WorldPosition).LengthSquared() > radiusSquared)
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
