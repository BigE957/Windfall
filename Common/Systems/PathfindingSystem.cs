using CalamityMod.Particles;
using System.Runtime.CompilerServices;
using Windfall.Common.Utils;

namespace Windfall.Common.Systems;

public class PathfindingSystem : ModSystem
{
    // Pathfinding System based on these videos' implementation of the A* algorithm:
    // https://www.youtube.com/watch?v=i0x5fj4PqP4
    // https://www.youtube.com/watch?v=alU04hvz6L4

    public const int CHUNK_SIZE = 64;
    public const int MAX_LOADED_CHUNKS = 256;

    public static NodeChunk[][] ChunkMap { get; private set; }
    public static int ChunkCountX { get; private set; }
    public static int ChunkCountY { get; private set; }
    private static int WorldMaxX;
    private static int WorldMaxY;

    private static readonly LinkedList<(int cx, int cy)> LruList = new();
    private static readonly Dictionary<(int, int), LinkedListNode<(int, int)>> LruIndex = [];
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
        if (ChunkMap == null) return;

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

    public static NodeChunk GetOrLoadChunk(int cx, int cy)
    {
        NodeChunk chunk = ChunkMap[cx][cy];
        if (chunk != null)
        {
            var node = LruIndex[(cx, cy)];
            LruList.Remove(node);
            LruList.AddLast(node);
            return chunk;
        }

        if (LoadedChunkCount >= MAX_LOADED_CHUNKS)
            EvictLruChunk();

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
        if (lruNode == null) return;

        (int cx, int cy) = lruNode.Value;
        LruList.RemoveFirst();
        LruIndex.Remove((cx, cy));
        ChunkMap[cx][cy] = null;
        LoadedChunkCount--;
    }

    public class NodeChunk
    {
        public readonly Node[] Nodes;
        public readonly int Width;
        public readonly int Height;

        public NodeChunk(int chunkX, int chunkY, int maxWorldX, int maxWorldY)
        {
            int startX = chunkX * CHUNK_SIZE;
            int startY = chunkY * CHUNK_SIZE;

            Width = Math.Min(CHUNK_SIZE, maxWorldX + 1 - startX);
            Height = Math.Min(CHUNK_SIZE, maxWorldY + 1 - startY);
            Nodes = new Node[Width * Height];

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

        public readonly int CompareTo(Node other) =>
            F != other.F ? F.CompareTo(other.F) : G.CompareTo(other.G);
    }

    public struct JumpSegment(Point start, Point end, int startIdx, int endIdx, bool isDrop = false, bool isDropThrough = false)
    {
        public Point StartPoint = start;
        public Point EndPoint = end;
        public int StartIndex = startIdx;
        public int EndIndex = endIdx;
        public bool IsDrop = isDrop;
        public bool IsDropThrough = isDropThrough;
    }

    public class FoundPath
    {
        public Point[] Points { get; private set; }
        public JumpSegment[] Jumps { get; private set; }
        public bool IsGroundedPath { get; private set; }

        // Tile A* — runs heuristic jump detection
        public FoundPath(Point[] nodes, bool isGroundedPath)
        {
            IsGroundedPath = isGroundedPath;
            if (isGroundedPath && nodes.Length > 1)
                ProcessJumps(nodes);
            else
            {
                Points = nodes;
                Jumps = Array.Empty<JumpSegment>();
            }
        }

        // Platform graph — jumps pre-validated, skip detection
        public FoundPath(Point[] points, JumpSegment[] jumps)
        {
            Points = points;
            Jumps = jumps;
            IsGroundedPath = true;
        }

        private void ProcessJumps(Point[] originalPath)
        {
            List<Point> newPath = [];
            List<JumpSegment> jumpList = [];

            int i = 0;
            while (i < originalPath.Length)
            {
                int jumpEnd = DetectJumpSequence(originalPath, i);
                if (jumpEnd > i)
                {
                    newPath.Add(originalPath[i]);
                    jumpList.Add(new JumpSegment(originalPath[i], originalPath[jumpEnd], newPath.Count - 1, newPath.Count));
                    newPath.Add(originalPath[jumpEnd]);
                    i = jumpEnd;
                }
                else
                {
                    newPath.Add(originalPath[i++]);
                }
            }

            Points = newPath.ToArray();
            Jumps = jumpList.ToArray();
        }

        private static int DetectJumpSequence(Point[] path, int startIndex)
        {
            if (startIndex >= path.Length - 1) return startIndex;

            Point next = path[startIndex + 1];
            if (WindfallUtils.IsSolidOrPlatform(next + new Point(0, 1))) return startIndex;

            int endIndex = startIndex + 1;
            for (int i = startIndex + 1; i < path.Length; i++)
            {
                endIndex = i;
                if (WindfallUtils.IsSolidOrPlatform(path[i] + new Point(0, 1))) break;
            }

            return path[endIndex].Y - 2 > path[startIndex].Y ? startIndex : endIndex;
        }

        public void DrawPath(SpriteBatch sb)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                bool isJump = false;
                foreach (var j in Jumps) { if (i == j.StartIndex) { isJump = true; break; } }

                if (!isJump)
                {
                    sb.DrawLineBetween(Points[i].ToWorldCoordinates(), Points[i + 1].ToWorldCoordinates(), Color.Red, 4);
                    GeneralParticleHandler.SpawnParticle(
                        new GlowOrbParticle(Points[i].ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, Color.Red));
                }
            }

            foreach (var j in Jumps)
                DrawJumpArc(sb,
                    j.StartPoint.ToWorldCoordinates(),
                    j.EndPoint.ToWorldCoordinates(),
                    j.IsDrop || j.IsDropThrough ? Color.Orange : Color.CornflowerBlue);
        }

        private static void DrawJumpArc(SpriteBatch sb, Vector2 start, Vector2 end, Color color)
        {
            float dist = Vector2.Distance(start, end);
            Vector2 peak = (start + end) / 2f - new Vector2(0, Math.Min(dist * 0.3f, 200f));
            int segments = Math.Max(8, (int)(dist / 16f));
            Vector2 prev = start;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float u = 1f - t;
                Vector2 cur = u * u * start + 2f * u * t * peak + t * t * end;
                sb.DrawLineBetween(prev, cur, color, 4);
                prev = cur;
            }

            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(start, Vector2.Zero, false, 3, 0.5f, Color.Blue));
            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(end, Vector2.Zero, false, 3, 0.5f, Color.Cyan));
        }
    }

    public class PathFinding
    {
        public FoundPath MyPath { get; private set; } = null;

        private readonly PriorityQueue OpenSet = new(4096);
        private readonly HashSet<Point16> ClosedSet = new(4096);
        private readonly HashSet<Point16> ModifiedNodesLocations = new(100);

        /// <summary>Proxy constructor — wraps a pre-built FoundPath for use with movement helpers.</summary>
        public PathFinding(FoundPath existingPath) { MyPath = existingPath; }
        public PathFinding() { }

        public void FindPathInRadius(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, int> costFunction = null, float searchRadius = 1225, bool processJumps = false)
        {
            Point start = startWorld.ToTileCoordinates();
            Point target = targetWorld.ToTileCoordinates();
            if (start == new Point(-1, -1) || target == new Point(-1, -1)) { MyPath = null; return; }
            if (start == target) { MyPath = new FoundPath([start, target], processJumps); return; }

            float r2 = searchRadius * searchRadius;
            if (Vector2.DistanceSquared(targetWorld, startWorld) > r2) { MyPath = null; return; }

            FindPath(startWorld, targetWorld, isWalkable,
                new(p => Vector2.DistanceSquared(p.ToWorldCoordinates(), targetWorld) <= r2),
                costFunction, processJumps);
        }

        public void FindPathInArea(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Rectangle searchArea, Func<Point, int> costFunction = null, bool processJumps = false)
        {
            Point start = startWorld.ToTileCoordinates();
            Point target = targetWorld.ToTileCoordinates();
            if (start == new Point(-1, -1) || target == new Point(-1, -1)) { MyPath = null; return; }
            if (start == target) { MyPath = new FoundPath([start, target], processJumps); return; }
            if (!searchArea.Contains(start) || !searchArea.Contains(target)) { MyPath = null; return; }

            FindPath(startWorld, targetWorld, isWalkable, searchArea.Contains, costFunction, processJumps);
        }

        public unsafe void FindPath(Vector2 startWorld, Vector2 targetWorld, Func<Point, Point, bool> isWalkable, Func<Point, bool> isValid, Func<Point, int> costFunction = null, bool processJumps = false)
        {
            if (ModifiedNodesLocations.Count > 0) ClearNodeStates(ModifiedNodesLocations);
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

            Func<Point, int> cachedCost = costFunction ?? (_ => 0);
            OpenSet.Enqueue(startKey, startNode.F);

            const int maxIterations = 4096 * 2;
            int iterations = 0;
            Point16 neighborLoc = new();
            Point neighborPoint = new();

            while (OpenSet.Count > 0 && iterations < maxIterations)
            {
                Point16 currentKey = OpenSet.Dequeue();
                ref Node current = ref GetNode(currentKey.X, currentKey.Y);

                if (current.X == targetX && current.Y == targetY)
                {
                    //Main.NewText("Iteration Count: " + iterations);
                    MyPath = ReconstructPath(current, startNode, processJumps);
                    ClearNodeStates(ModifiedNodesLocations);
                    return;
                }

                ClosedSet.Add(currentKey);

                for (int i = 0; i < current.NeighborCount; i++)
                {
                    byte dirIndex = current.NeighborLocations[i];
                    ref readonly Point16 dir = ref Dirs[dirIndex];

                    neighborLoc = new((short)(current.X + dir.X), (short)(current.Y + dir.Y));
                    if (ClosedSet.Contains(neighborLoc)) continue;

                    neighborPoint.X = neighborLoc.X;
                    neighborPoint.Y = neighborLoc.Y;

                    if (!isValid(neighborPoint)) continue;
                    if (!isWalkable(current.TilePosition, neighborPoint)) continue;

                    ref Node neighbor = ref GetNode(neighborLoc.X, neighborLoc.Y);
                    int tentativeG = current.G + current.GetDistance(neighborLoc.X, neighborLoc.Y) + cachedCost(neighborPoint);

                    if (tentativeG < neighbor.G)
                    {
                        ModifiedNodesLocations.Add(neighborLoc);
                        neighbor.SetConnection(new Point16(current.X, current.Y));
                        neighbor.SetG(tentativeG);
                        int newF = tentativeG + neighbor.GetDistance(targetX, targetY);
                        neighbor.SetF(newF);

                        if (OpenSet.Contains(neighborLoc)) OpenSet.UpdatePriority(neighborLoc, newF);
                        else OpenSet.Enqueue(neighborLoc, newF);
                    }
                }
                iterations++;
            }

            ClearNodeStates(ModifiedNodesLocations);
            MyPath = null;
        }

        private static void ClearNodeStates(HashSet<Point16> locations)
        {
            foreach (Point16 pos in locations) GetNode(pos.X, pos.Y).ResetState();
            locations.Clear();
        }

        private static FoundPath ReconstructPath(Node endNode, Node startNode, bool processJumps = false)
        {
            List<Point> path = new((int)Math.Sqrt(Math.Pow(endNode.X - startNode.X, 2) + Math.Pow(endNode.Y - startNode.Y, 2)) + 10);
            Node current = endNode;

            while (current.TilePosition != startNode.TilePosition)
            {
                path.Add(current.TilePosition);
                Point16 conn = current.ConnectionLocation;
                current = GetNode(conn.X, conn.Y);
            }
            path.Add(current.TilePosition);

            Point[] arr = [.. path];
            Array.Reverse(arr);
            return new FoundPath(arr, processJumps);
        }
    }

    public class PlatformPathFinding
    {
        public FoundPath MyPath { get; private set; } = null;

        private const float G_ACCEL = 0.4f;
        private const int MaxJumpRangeTiles = 30;
        private const int MaxJumpHeightTiles = 20;

        private readonly Dictionary<int, float> _gScore = [];
        private readonly IntPriorityQueue _open = new(256);
        private readonly HashSet<int> _closed = [];
        private readonly Dictionary<int, int> _cameFrom = [];
        private readonly Dictionary<int, int> _cameEdge = [];

        private readonly List<PlatformNode> _nodes = [];
        private readonly List<PlatformEdge> _edges = [];

        public void FindPath(Vector2 startWorld, Vector2 targetWorld, float jumpForce, float maxXSpeed, int npcHeightTiles, float searchRadius = 1300f)
        {
            MyPath = null;
            _nodes.Clear();
            _edges.Clear();

            Point startTile = startWorld.ToTileCoordinates();
            Point targetTile = targetWorld.ToTileCoordinates();

            int pad = MaxJumpRangeTiles;
            int minX = Math.Max(0, Math.Min(startTile.X, targetTile.X) - pad);
            int maxX = Math.Min(Main.maxTilesX - 1, Math.Max(startTile.X, targetTile.X) + pad);
            int minY = Math.Max(0, Math.Min(startTile.Y, targetTile.Y) - MaxJumpHeightTiles - pad);
            int maxY = Math.Min(Main.maxTilesY - 1, Math.Max(startTile.Y, targetTile.Y) + pad);

            var _swExtract = System.Diagnostics.Stopwatch.StartNew();
            ExtractPlatforms(minX, maxX, minY, maxY, npcHeightTiles);
            _swExtract.Stop();

            //Main.NewText($"[PF] Scan ({minX},{minY})-({maxX},{maxY})  Platforms: {_nodes.Count}  extract={_swExtract.ElapsedMilliseconds}ms");

            if (_nodes.Count == 0) 
            { 
                Main.NewText("[PF] No platforms found - aborting"); 
                return; 
            }

            PlatformNode startNode = FindPlatformAt(startTile, npcHeightTiles);
            PlatformNode targetNode = FindPlatformAt(targetTile, npcHeightTiles);

            //Main.NewText($"[PF] Start platform: {(startNode == null ? "NULL" : $"id={startNode.Id} x=[{startNode.MinX},{startNode.MaxX}] y={startNode.MinSurfaceY}")}");
            //Main.NewText($"[PF] Target platform: {(targetNode == null ? "NULL" : $"id={targetNode.Id} x=[{targetNode.MinX},{targetNode.MaxX}] y={targetNode.MinSurfaceY}")}");

            if (startNode == null || targetNode == null) return;

            // Same or adjacent surface — trivial single-platform path
            if (startNode == targetNode || OnSameSurface(startNode, targetNode))
            {
                MyPath = BuildFoundPath([startNode.Id], [-1], startWorld, targetWorld);
                return;
            }

            var _swConnect = System.Diagnostics.Stopwatch.StartNew();
            ConnectAllPlatforms(npcHeightTiles, jumpForce, maxXSpeed);
            _swConnect.Stop();

            int walks = 0, drops = 0, jumps = 0;
            foreach (var e in _edges)
            {
                if (e.IsWalk) 
                    walks++;
                else if (e.IsDrop || e.IsDropThrough) 
                    drops++;
                else 
                    jumps++;
            }
            //Main.NewText($"[PF] Edges: {_edges.Count} total  walk={walks} drop={drops} jump={jumps}  connect={_swConnect.ElapsedMilliseconds}ms");

            float radiusSq = searchRadius * searchRadius;
            _gScore.Clear(); _open.Clear(); _closed.Clear();
            _cameFrom.Clear(); _cameEdge.Clear();

            _gScore[startNode.Id] = 0f;
            _open.Enqueue(startNode.Id, Vector2.Distance(startNode.CenterWorld, targetNode.CenterWorld));

            const int maxIter = 2048;
            int iterCount = 0;
            for (int iter = 0; _open.Count > 0 && iter < maxIter; iter++, iterCount = iter)
            {
                int curId = _open.Dequeue();

                if (curId == targetNode.Id)
                {
                    MyPath = ReconstructPath(curId, startNode.Id, startWorld, targetWorld);
                    //Main.NewText($"[PF] Path found! {MyPath.Points.Length} pts, {MyPath.Jumps.Length} jumps  ({iterCount} A* iters)  build={_swConnect.ElapsedMilliseconds}ms");
                    return;
                }

                _closed.Add(curId);
                float curG = _gScore.TryGetValue(curId, out float g) ? g : float.MaxValue;

                foreach (int edgeIdx in _nodes[curId].EdgeIndices)
                {
                    PlatformEdge edge = _edges[edgeIdx];
                    int nextId = edge.TargetId;

                    if (_closed.Contains(nextId)) continue;

                    float tentG = curG + edge.Cost;
                    if (_gScore.TryGetValue(nextId, out float existing) && tentG >= existing) continue;

                    _cameFrom[nextId] = curId;
                    _cameEdge[nextId] = edgeIdx;
                    _gScore[nextId] = tentG;

                    float f = tentG + Vector2.Distance(_nodes[nextId].CenterWorld, targetNode.CenterWorld);
                    if (_open.Contains(nextId)) _open.UpdatePriority(nextId, f);
                    else _open.Enqueue(nextId, f);
                }
            }

            //Main.NewText($"[PF] No path found  ({iterCount} A* iters, {_nodes.Count} platforms)");
        }

        private void ExtractPlatforms(int minX, int maxX, int minY, int maxY, int npcHeightTiles)
        {
            HashSet<long> claimed = [];

            for (int y = minY; y <= maxY; y++)
            {
                int startRun = -1;

                for (int x = minX; x <= maxX; x++)
                {
                    if (IsSurfaceTile(x, y, npcHeightTiles))
                    {
                        startRun = startRun < 0 ? x : startRun;
                    }
                    else
                    {
                        if (startRun >= 0)
                        {
                            AddPlatformRun(startRun, x - 1, y, npcHeightTiles, claimed);
                            startRun = -1;
                        }
                    }
                }
                if (startRun >= 0)
                    AddPlatformRun(startRun, maxX, y, npcHeightTiles, claimed);
            }
        }

        private void AddPlatformRun(int startX, int endX, int scanY, int npcHeightTiles, HashSet<long> claimed)
        {
            var node = new PlatformNode { Id = _nodes.Count };

            for (int x = startX; x <= endX; x++)
            {
                long key = (long)x << 20 | (uint)scanY;
                if (claimed.Contains(key)) continue;
                if (!IsSurfaceTile(x, scanY, npcHeightTiles)) continue;

                node.SurfaceTiles[x] = scanY;
                claimed.Add(key);
            }

            if (node.SurfaceTiles.Count == 0) return;

            node.MinX = node.SurfaceTiles.Keys.Min();
            node.MaxX = node.SurfaceTiles.Keys.Max();
            node.MinSurfaceY = node.SurfaceTiles.Values.Min();
            node.MaxSurfaceY = node.SurfaceTiles.Values.Max();

            foreach (var existing in _nodes)
            {
                if (!AreAdjacentSurfaces(existing, node)) 
                    continue;

                foreach (var (x, sy) in node.SurfaceTiles)
                    existing.SurfaceTiles[x] = sy;

                existing.MinX = existing.SurfaceTiles.Keys.Min();
                existing.MaxX = existing.SurfaceTiles.Keys.Max();
                existing.MinSurfaceY = existing.SurfaceTiles.Values.Min();
                existing.MaxSurfaceY = existing.SurfaceTiles.Values.Max();
                return;
            }

            _nodes.Add(node);
        }

        private static bool IsSurfaceTile(int x, int y, int npcHeightTiles)
        {
            if (!WorldGen.InWorld(x, y)) return false;
            Tile tile = Main.tile[x, y];
            if (tile == null) return false;

            bool isSlopedOrHalf = tile.HasTile && (tile.IsHalfBlock || tile.Slope != SlopeType.Solid);
            bool isPlatform = tile.HasTile && TileID.Sets.Platforms[tile.TileType];

            if (isSlopedOrHalf)
            {
                for (int i = 1; i < npcHeightTiles; i++)
                {
                    if (!WorldGen.InWorld(x, y - i)) return false;
                    Tile above = Main.tile[x, y - i];
                    if (above != null && WorldGen.SolidTile(above)) return false;
                }
                return true;
            }

            if (WorldGen.SolidTile(tile)) return false;

            if (!isPlatform && !HasGroundBelow(x, y + 1))
                return false;

            for (int i = 1; i < npcHeightTiles; i++)
            {
                if (!WorldGen.InWorld(x, y - i)) return false;
                Tile above = Main.tile[x, y - i];
                if (above != null && WorldGen.SolidTile(above)) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if (x, y) can act as ground for an NPC standing above it.
        /// Accepts fully solid, sloped, half-block, and platform tiles.
        /// </summary>
        private static bool HasGroundBelow(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) 
                return false;
            Tile t = Main.tile[x, y];
            if (t == null || !t.HasTile) 
                return false;
            if (TileID.Sets.Platforms[t.TileType]) 
                return true;
            return Main.tileSolid[t.TileType];
        }

        private void ConnectAllPlatforms(int npcHeightTiles, float jumpForce, float maxXSpeed)
        {
            float maxRangePx = MaxJumpRangeTiles * 16f;
            float maxHeightPx = MaxJumpHeightTiles * 16f;

            for (int i = 0; i < _nodes.Count; i++)
            {
                for (int j = 0; j < _nodes.Count; j++)
                {
                    if (i == j) continue;

                    PlatformNode src = _nodes[i];
                    PlatformNode tgt = _nodes[j];

                    float hDist = Math.Max(0f, Math.Max(src.MinX - tgt.MaxX, tgt.MinX - src.MaxX) * 16f);
                    float vDist = Math.Abs(src.MinSurfaceY - tgt.MinSurfaceY) * 16f;
                    if (hDist > maxRangePx || vDist > maxHeightPx) continue;

                    bool already = false;
                    foreach (int ei in src.EdgeIndices)
                        if (_edges[ei].TargetId == j) { already = true; break; }
                    if (already) continue;

                    if (AreAdjacentSurfaces(src, tgt)) { AddWalkEdge(i, j); continue; }

                    bool dropAdded = false;
                    if (tgt.MinSurfaceY > src.MaxSurfaceY)
                        dropAdded = TryAddDropEdge(i, j, maxXSpeed);

                    if (!dropAdded)
                        TryAddJumpEdge(i, j, npcHeightTiles, jumpForce, maxXSpeed);
                }
            }
        }

        private static bool AreAdjacentSurfaces(PlatformNode a, PlatformNode b)
        {
            if (!(a.MaxX >= b.MinX - 1 && b.MaxX >= a.MinX - 1)) return false;
            int aEdgeX = a.MaxX < b.MinX ? a.MaxX : a.MinX;
            int bEdgeX = a.MaxX < b.MinX ? b.MinX : b.MaxX;
            if (!a.SurfaceTiles.TryGetValue(aEdgeX, out int aY)) return false;
            if (!b.SurfaceTiles.TryGetValue(bEdgeX, out int bY)) return false;
            return Math.Abs(aY - bY) <= 1;
        }

        private void AddWalkEdge(int srcId, int tgtId)
        {
            PlatformNode src = _nodes[srcId];
            PlatformNode tgt = _nodes[tgtId];
            Point launch = src.GetTileAt(src.MaxX < tgt.MinX ? src.MaxX : src.MinX);
            Point land = tgt.GetTileAt(src.MaxX < tgt.MinX ? tgt.MinX : tgt.MaxX);

            int idx = _edges.Count;
            _edges.Add(new PlatformEdge { TargetId = tgtId, LaunchPoint = launch, LandPoint = land, Cost = 1f, IsWalk = true });
            src.EdgeIndices.Add(idx);
        }

        private bool TryAddDropEdge(int srcId, int tgtId, float maxXSpeed)
        {
            PlatformNode src = _nodes[srcId];
            PlatformNode tgt = _nodes[tgtId];

            bool tgtIsRight = tgt.Center.X >= src.Center.X;
            int edgeTileX = tgtIsRight ? src.MaxX : src.MinX;

            for (int setback = 0; setback <= 2; setback++)
            {
                int launchX = tgtIsRight ? edgeTileX - setback : edgeTileX + setback;
                launchX = Math.Clamp(launchX, src.MinX, src.MaxX);
                if (!src.SurfaceTiles.TryGetValue(launchX, out int launchY)) 
                    continue;

                Point launch = new(launchX, launchY);
                Point land = tgt.GetTileAt(Math.Clamp(launchX, tgt.MinX, tgt.MaxX));

                float dx = (land.X - launch.X) * 16f;
                float dy = (land.Y - launch.Y) * 16f;
                if (dy <= 0f) 
                    continue;

                float fallTime = MathF.Sqrt(2f * dy / G_ACCEL);
                if (fallTime <= 0f) 
                    continue;
                if (MathF.Abs(dx) / fallTime > maxXSpeed) 
                    continue;

                float vx = dx / fallTime;

                Point floorTile = new(launch.X, launch.Y + 1);
                bool dropThru = WorldGen.InWorld(floorTile.X, floorTile.Y) && TileID.Sets.Platforms[Main.tile[floorTile.X, floorTile.Y].TileType];

                bool blocked = false;
                for (int y = launch.Y + 1; y < land.Y && !blocked; y++)
                {
                    float t = MathF.Sqrt(2f * (y - launch.Y) * 16f / G_ACCEL);
                    float worldX = launch.X * 16f + vx * t;
                    int tileX = (int)MathF.Floor(worldX / 16f);

                    if (!WorldGen.InWorld(tileX, y)) continue;

                    Tile tile = Main.tile[tileX, y];
                    if (WorldGen.SolidTile(tile))
                    {
                        blocked = true;
                        break;
                    }

                    if (TileID.Sets.Platforms[tile.TileType])
                    {
                        if (y == launch.Y + 1 && dropThru && tileX >= src.MinX && tileX <= src.MaxX)
                            continue;

                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    int idx = _edges.Count;
                    _edges.Add(new PlatformEdge
                    {
                        TargetId = tgtId,
                        LaunchPoint = launch,
                        LandPoint = land,
                        Cost = fallTime,
                        IsDrop = !dropThru,
                        IsDropThrough = dropThru,
                    });
                    src.EdgeIndices.Add(idx);
                    return true;
                }
            }
            return false;
        }

        private void TryAddJumpEdge(int srcId, int tgtId, int npcHeightTiles, float jumpForce, float maxXSpeed)
        {
            PlatformNode src = _nodes[srcId];
            PlatformNode tgt = _nodes[tgtId];

            bool tgtIsRight = tgt.Center.X >= src.Center.X;
            int edgeTileX = tgtIsRight ? src.MaxX : src.MinX;

            for (int setback = 0; setback <= 5; setback++)
            {
                int launchX = tgtIsRight ? edgeTileX - setback : edgeTileX + setback;

                launchX = Math.Clamp(launchX, src.MinX, src.MaxX);
                if (!src.SurfaceTiles.TryGetValue(launchX, out int launchY)) continue;

                Point launch = new(launchX, launchY);
                Point land = tgt.GetTileAt(Math.Clamp(launchX, tgt.MinX, tgt.MaxX));
                Vector2 launchW = launch.ToWorldCoordinates();
                Vector2 landW = land.ToWorldCoordinates();

                float dx = landW.X - launchW.X;
                float dy = landW.Y - launchW.Y;

                if (MathF.Abs(dx) < 4f && dy >= 0f) 
                    continue;

                float vy0;
                if (dy < 0f)
                {
                    float minVy = -MathF.Sqrt(2f * G_ACCEL * MathF.Abs(dy));
                    if (minVy < -jumpForce) 
                        continue;
                    vy0 = minVy;
                }
                else
                    vy0 = -jumpForce;

                float disc = vy0 * vy0 + 2f * G_ACCEL * dy;
                if (disc < 0f) 
                    continue;
                float t = (-vy0 + MathF.Sqrt(disc)) / G_ACCEL;
                if (t <= 0f) 
                    continue;

                float vx = dx / t;
                if (MathF.Abs(vx) > maxXSpeed) continue;

                bool blocked = false;
                int steps = Math.Max(16, (int)(MathF.Abs(dx) / 8f));
                for (int s = 0; s <= steps && !blocked; s++)
                {
                    float st = s / (float)steps * t;
                    float wx = launchW.X + vx * st;
                    float wy = launchW.Y + vy0 * st + 0.5f * G_ACCEL * st * st;
                    float vyNow = vy0 + G_ACCEL * st;

                    for (int h = 0; h < npcHeightTiles; h++)
                    {
                        Point tile = new Vector2(wx, wy - h * 16f).ToTileCoordinates();
                        if (!WorldGen.InWorld(tile.X, tile.Y)) 
                            continue;
                        if (WorldGen.SolidTile(tile.X, tile.Y)) 
                        { 
                            blocked = true;
                            break;
                        }
                        if (TileID.Sets.Platforms[Main.tile[tile.X, tile.Y].TileType] && vyNow > 0f && h == 0) 
                        { 
                            blocked = true;
                            break;
                        }
                    }
                }

                if (!blocked)
                {
                    int idx = _edges.Count;
                    _edges.Add(new PlatformEdge
                    {
                        TargetId = tgtId,
                        LaunchPoint = launch,
                        LandPoint = land,
                        Cost = t,
                    });
                    src.EdgeIndices.Add(idx);
                    return;
                }
            }
        }

        private FoundPath ReconstructPath(int endId, int startId, Vector2 startWorld, Vector2 targetWorld)
        {
            List<int> ids = [];
            List<int> edges = [];

            int cur = endId;
            while (cur != startId)
            {
                ids.Add(cur);
                edges.Add(_cameEdge.TryGetValue(cur, out int ei) ? ei : -1);
                cur = _cameFrom[cur];
            }
            ids.Add(startId);
            edges.Add(-1);

            ids.Reverse();
            edges.Reverse();
            return BuildFoundPath(ids, edges, startWorld, targetWorld);
        }

        private FoundPath BuildFoundPath(List<int> ids, List<int> edgeIdxs, Vector2 startWorld, Vector2 targetWorld)
        {
            List<Point> points = [];
            List<JumpSegment> jumps = [];

            Point startTile = startWorld.ToTileCoordinates();
            Point targetTile = targetWorld.ToTileCoordinates();

            for (int i = 0; i < ids.Count; i++)
            {
                PlatformNode node = _nodes[ids[i]];
                bool isFirst = i == 0;
                bool isLast = i == ids.Count - 1;

                int inEdge = isFirst ? -1 : edgeIdxs[i];
                Point entry = isFirst
                    ? node.GetTileAt(startTile.X)
                    : _edges[inEdge].LandPoint;

                int outEdge = isLast ? -1 : edgeIdxs[i + 1];
                Point exit = isLast
                    ? node.GetTileAt(targetTile.X)
                    : _edges[outEdge].LaunchPoint;

                points.Add(entry);
                if (entry.X != exit.X)
                {
                    int step = entry.X < exit.X ? 1 : -1;
                    for (int x = entry.X + step; x != exit.X; x += step)
                        if (node.SurfaceTiles.TryGetValue(x, out int sy))
                            points.Add(new Point(x, sy));
                }
                if (exit != entry)
                    points.Add(exit);

                // Outgoing jump segment
                if (!isLast)
                {
                    PlatformEdge e = _edges[outEdge];
                    if (!e.IsWalk)
                        jumps.Add(new JumpSegment(e.LaunchPoint, e.LandPoint, points.Count - 1, points.Count, e.IsDrop, e.IsDropThrough));
                }
            }

            return new FoundPath(points.ToArray(), jumps.ToArray());
        }

        public void DrawDebug(SpriteBatch sb)
        {
            if (_nodes.Count == 0) return;

            foreach (var node in _nodes)
            {
                // Draw platform surface as a green line
                if (node.SurfaceTiles.Count > 0)
                {
                    Point pMin = node.GetTileAt(node.MinX);
                    Point pMax = node.GetTileAt(node.MaxX);
                    Vector2 wMin = pMin.ToWorldCoordinates() + new Vector2(0, 8);
                    Vector2 wMax = pMax.ToWorldCoordinates() + new Vector2(16, 8);
                    sb.DrawLineBetween(wMin, wMax, Color.LimeGreen, 4);
                }
            }

            foreach (var edge in _edges)
            {
                if (edge.TargetId < 0 || edge.TargetId >= _nodes.Count) continue;

                Vector2 from = edge.LaunchPoint.ToWorldCoordinates() + new Vector2(8, 8);
                Vector2 to = edge.LandPoint.ToWorldCoordinates() + new Vector2(8, 8);

                Color col = edge.IsWalk ? Color.Gray : (edge.IsDrop || edge.IsDropThrough) ? Color.Orange : Color.CornflowerBlue;

                if (!edge.IsWalk && !edge.IsDrop && !edge.IsDropThrough)
                {
                    float dist = Vector2.Distance(from, to);
                    Vector2 peak = (from + to) / 2f - new Vector2(0, Math.Min(dist * 0.4f, 120f));
                    int segs = Math.Max(6, (int)(dist / 32f));
                    Vector2 prev = from;
                    for (int s = 1; s <= segs; s++)
                    {
                        float tt = s / (float)segs;
                        float u = 1f - tt;
                        Vector2 cur = (u * u * from + 2f * u * tt * peak + tt * tt * to);
                        sb.DrawLineBetween(prev, cur, col, 2);
                        prev = cur;
                    }
                }
                else
                    sb.DrawLineBetween(from, to, col, 2);
            }
        }

        private PlatformNode FindPlatformAt(Point tilePos, int npcHeightTiles)
        {
            foreach (var p in _nodes)
            {
                if (p.Contains(tilePos)) return p;
                for (int dy = -npcHeightTiles; dy <= npcHeightTiles; dy++)
                    if (p.Contains(new Point(tilePos.X, tilePos.Y + dy))) 
                        return p;
            }
            return null;
        }

        private static bool OnSameSurface(PlatformNode a, PlatformNode b) => a.MaxX >= b.MinX - 1 && b.MaxX >= a.MinX - 1 && Math.Abs(a.MinSurfaceY - b.MinSurfaceY) <= 2;
    }

    public class PlatformNode
    {
        public int Id;
        public Dictionary<int, int> SurfaceTiles = []; // X -> Y
        public int MinX, MaxX, MinSurfaceY, MaxSurfaceY;
        public List<int> EdgeIndices = [];

        public Point Center => new((MinX + MaxX) / 2, SurfaceTiles.TryGetValue((MinX + MaxX) / 2, out int cy) ? cy : MinSurfaceY);
        public Vector2 CenterWorld => Center.ToWorldCoordinates();

        public Point GetTileAt(int worldX)
        {
            int cx = Math.Clamp(worldX, MinX, MaxX);
            return new Point(cx, SurfaceTiles[cx]);
        }

        public bool Contains(Point tile) =>
            SurfaceTiles.TryGetValue(tile.X, out int y) && y == tile.Y;
    }

    public class PlatformEdge
    {
        public int TargetId;
        public Point LaunchPoint;
        public Point LandPoint;
        public float Cost;
        public bool IsDrop = false;
        public bool IsDropThrough = false;
        public bool IsWalk = false;
    }

    public class IntPriorityQueue(int capacity = 16)
    {
        private struct QueueItem { public int Key; public float P; }
        private QueueItem[] _h = new QueueItem[Math.Max(capacity, 16)];
        private readonly Dictionary<int, int> _i = new(capacity);
        private int _n;

        public int Count => _n;
        public bool Contains(int key) => _i.ContainsKey(key);

        public void Enqueue(int key, float p)
        {
            if (_n == _h.Length) Array.Resize(ref _h, _h.Length * 2);
            _h[_n] = new QueueItem { Key = key, P = p };
            _i[key] = _n;
            Up(_n++);
        }

        public int Dequeue()
        {
            int r = _h[0].Key; _n--;
            if (_n > 0)
            { 
                _h[0] = _h[_n];
                _i[_h[0].Key] = 0;
            }
            _i.Remove(r);
            if (_n > 0) 
                Down(0);
            return r;
        }

        public void UpdatePriority(int key, float p)
        {
            if (!_i.TryGetValue(key, out int idx))
                return;

            float old = _h[idx].P;
            _h[idx].P = p;

            if (p < old)
                Up(idx);
            else
                Down(idx);
        }

        public void Clear()
        { 
            _n = 0; 
            _i.Clear();
        }

        private void Up(int i)
        {
            QueueItem it = _h[i];
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (it.P >= _h[p].P) 
                    break;

                _h[i] = _h[p]; 
                _i[_h[i].Key] = i; 
                i = p;
            }
            _h[i] = it; _i[it.Key] = i;
        }

        private void Down(int i)
        {
            QueueItem it = _h[i];
            while (true)
            {
                int l = i * 2 + 1;
                int r = i * 2 + 2;
                if (l >= _n)
                    break;
                int s = (r < _n && _h[r].P < _h[l].P) ? r : l;
                if (it.P <= _h[s].P)
                    break;
                _h[i] = _h[s];
                _i[_h[i].Key] = i;
                i = s;
            }
            _h[i] = it; _i[it.Key] = i;
        }
    }

    public class PriorityQueue(int capacity = 16)
    {
        private struct QueueItem { public Point16 Key; public float P; }
        private QueueItem[] _h = new QueueItem[Math.Max(capacity, 16)];
        private readonly Dictionary<Point16, int> _i = new(capacity);
        private int _n;

        public int Count => _n;
        public bool Contains(Point16 key) => _i.ContainsKey(key);

        public void Enqueue(Point16 key, float p)
        {
            if (_n == _h.Length)
                Array.Resize(ref _h, _h.Length * 2);
            _h[_n] = new QueueItem { Key = key, P = p };
            _i[key] = _n;
            Up(_n++);
        }

        public Point16 Dequeue()
        {
            Point16 r = _h[0].Key;
            _n--;
            if (_n > 0)
            {
                _h[0] = _h[_n];
                _i[_h[0].Key] = 0;
            }
            _i.Remove(r);
            if (_n > 0) 
                Down(0);
            return r;
        }

        public void UpdatePriority(Point16 key, float p)
        {
            if (!_i.TryGetValue(key, out int idx)) 
                return;
            float old = _h[idx].P; 
            _h[idx].P = p;
            if (p < old)
                Up(idx);
            else
                Down(idx);
        }

        public void Clear()
        {
            _n = 0;
            _i.Clear();
        }

        private void Up(int i)
        {
            QueueItem it = _h[i];
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (it.P >= _h[p].P)
                    break; 

                _h[i] = _h[p];
                _i[_h[i].Key] = i;
                i = p;
            }
            _h[i] = it; 
            _i[it.Key] = i;
        }

        private void Down(int i)
        {
            QueueItem it = _h[i];
            while (true)
            {
                int l = i * 2 + 1;
                int r = i * 2 + 2;
                if (l >= _n) 
                    break;

                int s = (r < _n && _h[r].P < _h[l].P) ? r : l;
                if (it.P <= _h[s].P)
                    break;

                _h[i] = _h[s]; _i[_h[i].Key] = i; i = s;
            }
            _h[i] = it; 
            _i[it.Key] = i;
        }
    }
}