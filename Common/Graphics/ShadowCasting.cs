using Luminance.Core.Graphics;
using System.Diagnostics;
using System.Net;
using Windfall.Content.NPCs.Debug;

namespace Windfall.Common.Graphics;

[Autoload(Side = ModSide.Client)]
public class ShadowCasting : ModSystem
{
    internal struct Cell()
    {
        internal int[] edge_id = [-1, -1, -1, -1];
        internal bool[] edge_exist = [false, false, false, false];
        internal bool exist = false;
    };

    internal Cell[][] worldCells;

    internal struct Edge
    {
        internal Vector2 Start; // Start coordinate
        internal Vector2 End; // End coordinate
    };

    internal List<Edge> edges = [];

    Matrix world = Matrix.CreateTranslation(0, 0, 0);
    Matrix view = new (
        Main.GameViewMatrix.Zoom.X, 0, 0, 0,
        0, Main.GameViewMatrix.Zoom.X, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
    BasicEffect basicEffect;
    VertexBuffer vertexBuffer;

    public static ManagedRenderTarget AlphaMap
    {
        get;
        private set;
    }

    public override void Load()
    {
        On_Main.DrawRain += DrawShadows;

        Main.QueueMainThreadAction(() =>
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            basicEffect = new(Main.graphics.GraphicsDevice)
            {
                World = world,
                View = view,
                VertexColorEnabled = true,                
            };

            vertexBuffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), (int)Math.Floor(short.MaxValue * 0.3), BufferUsage.WriteOnly);

            AlphaMap = new ManagedRenderTarget(false, ManagedRenderTarget.CreateScreenSizedTarget);
        });
    }

    public override void OnWorldLoad()
    {
        worldCells = new Cell[Main.maxTilesX][];
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            worldCells[x] = new Cell[Main.maxTilesY];
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                worldCells[x][y] = new();
            }
        }
    }

    public override void OnWorldUnload()
    {
        worldCells = null;
    }

    public override void PostUpdateEverything()
    {
        if((int)(Main.GlobalTimeWrappedHourly * 60) % 5 == 0)
        {
            Point playerTile = Main.LocalPlayer.Center.ToTileCoordinates();
            Rectangle area = new(playerTile.X - 55, playerTile.Y - 34, 110, 68);
            ConvertTileMapToPolyMap(area.X, area.Y, area.Width, area.Height);
        }  
    }

    public void ConvertTileMapToPolyMap(int sx, int sy, int w, int h)
    {
        int NORTH = 0;
        int SOUTH = 1;
        int EAST = 2;
        int WEST = 3;

        // Clear "PolyMap"
        edges.Clear();

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                for (int j = 0; j < 4; j++)
                {
                    worldCells[sx + x][sy + y].edge_exist[j] = false;
                    worldCells[sx + x][sy + y].edge_id[j] = 0;
                }

        // Iterate through region from top left to bottom right
        for (int i = 1; i < w - 1; i++)
            for (int j = 1; j < h - 1; j++)
            {
                int x = sx + i;
                int y = sy + j;

                // If this cell exists, check if it needs edges
                if (WorldGen.SolidTile(x, y))
                {
                    // If this cell has no western neighbour, it needs a western edge
                    if (!WorldGen.SolidTile(x - 1, y))
                    {
                        // It can either extend it from its northern neighbour if they have
                        // one, or It can start a new one.
                        if (worldCells[x][y - 1].edge_exist[WEST])
                        {
                            // Northern neighbour has a western edge, so grow it downwards
                            Edge edge = edges[worldCells[x][y - 1].edge_id[WEST]];
                            edge.End.Y += 16;
                            edges[worldCells[x][y - 1].edge_id[WEST]] = edge;

                            worldCells[x][y].edge_id[WEST] = worldCells[x][y - 1].edge_id[WEST];
                            worldCells[x][y].edge_exist[WEST] = true;
                        }
                        else
                        {
                            // Northern neighbour does not have one, so create one
                            Edge edge;
                            edge.Start = new(x * 16, y * 16);
                            edge.End = edge.Start + Vector2.UnitY * 16;

                            // Add edge to Polygon Pool
                            int edge_id = edges.Count;
                            edges.Add(edge);

                            // Update tile information with edge information
                            worldCells[x][y].edge_id[WEST] = edge_id;
                            worldCells[x][y].edge_exist[WEST] = true;
                        }
                    }

                    // If this cell dont have an eastern neignbour, It needs a eastern edge
                    if (!WorldGen.SolidTile(x + 1, y))
                    {
                        // It can either extend it from its northern neighbour if they have
                        // one, or It can start a new one.
                        if (worldCells[x][y - 1].edge_exist[EAST])
                        {
                            // Northern neighbour has one, so grow it downwards
                            Edge edge = edges[worldCells[x][y - 1].edge_id[EAST]];
                            edge.End.Y += 16;
                            edges[worldCells[x][y - 1].edge_id[EAST]] = edge;

                            worldCells[x][y].edge_id[EAST] = worldCells[x][y - 1].edge_id[EAST];
                            worldCells[x][y].edge_exist[EAST] = true;
                        }
                        else
                        {
                            // Northern neighbour does not have one, so create one
                            Edge edge;
                            edge.Start = new((x + 1) * 16, y * 16);
                            edge.End = edge.Start + Vector2.UnitY * 16;

                            // Add edge to Polygon Pool
                            int edge_id = edges.Count;
                            edges.Add(edge);

                            // Update tile information with edge information
                            worldCells[x][y].edge_id[EAST] = edge_id;
                            worldCells[x][y].edge_exist[EAST] = true;
                        }
                    }

                    // If this cell doesnt have a northern neignbour, It needs a northern edge
                    if (!WorldGen.SolidTile(x, y - 1))
                    {
                        // It can either extend it from its western neighbour if they have
                        // one, or It can start a new one.
                        if (worldCells[x - 1][y].edge_exist[NORTH])
                        {
                            // Western neighbour has one, so grow it eastwards
                            Edge edge = edges[worldCells[x - 1][y].edge_id[NORTH]];
                            edge.End.X += 16;
                            edges[worldCells[x - 1][y].edge_id[NORTH]] = edge;

                            worldCells[x][y].edge_id[NORTH] = worldCells[x - 1][y].edge_id[NORTH];
                            worldCells[x][y].edge_exist[NORTH] = true;
                        }
                        else
                        {
                            // Western neighbour does not have one, so create one
                            Edge edge;
                            edge.Start = new(x * 16, y * 16);
                            edge.End = edge.Start + Vector2.UnitX * 16;

                            // Add edge to Polygon Pool
                            int edge_id = edges.Count;
                            edges.Add(edge);

                            // Update tile information with edge information
                            worldCells[x][y].edge_id[NORTH] = edge_id;
                            worldCells[x][y].edge_exist[NORTH] = true;
                        }
                    }

                    // If this cell doesnt have a southern neignbour, It needs a southern edge
                    if (!WorldGen.SolidTile(x, y + 1))
                    {
                        // It can either extend it from its western neighbour if they have
                        // one, or It can start a new one.
                        if (worldCells[x - 1][y].edge_exist[SOUTH])
                        {
                            // Western neighbour has one, so grow it eastwards
                            Edge edge = edges[worldCells[x - 1][y].edge_id[SOUTH]];
                            edge.End.X += 16;
                            edges[worldCells[x - 1][y].edge_id[SOUTH]] = edge;

                            worldCells[x][y].edge_id[SOUTH] = worldCells[x - 1][y].edge_id[SOUTH];
                            worldCells[x][y].edge_exist[SOUTH] = true;
                        }
                        else
                        {
                            // Western neighbour does not have one, so I need to create one
                            Edge edge;
                            edge.Start = new(x * 16, (y + 1) * 16);
                            edge.End = edge.Start + Vector2.UnitX * 16;

                            // Add edge to Polygon Pool
                            int edge_id = edges.Count;
                            edges.Add(edge);

                            // Update tile information with edge information
                            worldCells[x][y].edge_id[SOUTH] = edge_id;
                            worldCells[x][y].edge_exist[SOUTH] = true;
                        }
                    }

                }

            }

        Edge screenTop;
        screenTop.Start = Main.screenPosition;
        screenTop.End = Main.screenPosition + Vector2.UnitX * Main.screenWidth;
        edges.Add(screenTop);

        Edge screenRight;
        screenRight.Start = screenTop.End;
        screenRight.End = screenRight.Start + Vector2.UnitY * Main.screenWidth;
        edges.Add(screenRight);

        Edge screenBottom;
        screenBottom.Start = screenRight.End;
        screenBottom.End = Main.screenPosition + Vector2.UnitY * Main.screenWidth;
        edges.Add(screenBottom);

        Edge screenLeft;
        screenLeft.Start = screenBottom.End;
        screenLeft.End = Main.screenPosition;
        edges.Add(screenLeft);
    }

    private void DrawShadows(On_Main.orig_DrawRain orig, Main self)
    {
        orig(self);

        bool debug = true;
        var watch = Stopwatch.StartNew();
        //Main.NewText(edges.Count);
        int rayCount = 0;
        bool display = true;
        float castRange = 840;

        HashSet<Vector2> checkedLocatons = [];
        List<Vector2> EndPoints = [];

        foreach (Edge edge in edges)
        {
            if (debug)
            {
                //Main.spriteBatch.DrawLineBetter(edge.Start, edge.End, Color.Cyan, 4);
                //Dust.QuickDust(edge.Start, Color.Red);
                //Dust.QuickDust(edge.End, Color.Red);
            }

            Vector2 Center = Main.LocalPlayer.Center;

            ProcessEdgePoint(edge.Start, Center, castRange, ref checkedLocatons, ref EndPoints, ref rayCount);              

            ProcessEdgePoint(edge.End, Center, castRange, ref checkedLocatons, ref EndPoints, ref rayCount);
        }

        if (EndPoints.Count == 0)
            return;

        if (display)
        {
            EndPoints.Sort(compare);

            List<Tuple<Vector2, Vector2, Vector2>> Triangles = [];

            //Triangles.Add(new(Main.screenPosition, Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight), Main.screenPosition + Vector2.UnitY * Main.screenHeight));
            //Triangles.Add(new(Main.screenPosition, Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight), Main.screenPosition + Vector2.UnitX * Main.screenWidth));

            for (int i = 0; i < EndPoints.Count - 1; i++)
            {
                Triangles.Add(new(Main.LocalPlayer.Center, EndPoints[i], EndPoints[i + 1]));
            }
            Triangles.Add(new(Main.LocalPlayer.Center, EndPoints[^1], EndPoints[0]));

            if (Triangles.Count == 0)
                return;

            List<Vector2> triangleVertices = [];

            foreach (Tuple<Vector2, Vector2, Vector2> triangle in Triangles)
            {
                triangleVertices.Add(triangle.Item1);// + Main.LocalPlayer.Center);
                triangleVertices.Add(triangle.Item2);// + Main.LocalPlayer.Center);
                triangleVertices.Add(triangle.Item3);// + Main.LocalPlayer.Center);
            }

            if (triangleVertices.Count < 3)
                return;

            VertexPositionColor[] vertices = new VertexPositionColor[triangleVertices.Count];
            GraphicsDevice GD = Main.graphics.GraphicsDevice;

            for (int i = 0; i < triangleVertices.Count; i++)
            {
                Vector2 triangle = WorldToWorldMatrixPos(triangleVertices[i] - Main.screenPosition);
                vertices[i] = new VertexPositionColor(new Vector3(triangle.X, triangle.Y, 0f), Color.White);
            }

            vertexBuffer?.SetData(vertices, SetDataOptions.Discard);
            
            GD.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,                
            };
            RasterizerState oldState = GD.RasterizerState;
            GD.RasterizerState = rasterizerState;

            if (AlphaMap.IsDisposed)
                AlphaMap.Recreate(Main.screenWidth, Main.screenHeight);

            // Take the contents of the screen for usage by the shatter pieces.

            GD.SetRenderTarget(AlphaMap);
            GD.Clear(Color.Black);
            //AlphaMap.Target.SetData(GD.GetVertexBuffers());
            int numTriangles = triangleVertices.Count / 3;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GD.DrawPrimitives(PrimitiveType.TriangleList, 0, numTriangles);
            }

            ManagedScreenFilter shadowShader = ShaderManager.GetFilter("Windfall.ShadowShader");
            shadowShader.SetTexture(AlphaMap, 2, SamplerState.LinearClamp);
            shadowShader.Activate();

            GD.SetRenderTarget(null);
            GD.SetVertexBuffer(null);
            GD.RasterizerState = oldState;
            GD.Clear(Color.Transparent);
        }

        if (debug)
        {
            Main.NewText("Edge Count: " + edges.Count);
            Main.NewText("Ray Count: " + rayCount);
            Main.NewText("Total Elapsed Time: " + watch.ElapsedMilliseconds + "ms");
        }
    }

    private static void ProcessEdgePoint(Vector2 point, Vector2 Center, float castRange, ref HashSet<Vector2> checkedLocations, ref List<Vector2> EndPoints, ref int rayCount)
    {
        if (!checkedLocations.Add(point))
        {
            Vector2 dir = point - Center;
            dir = dir.SafeNormalize(Vector2.Zero);

            for (int i = -1; i < 2; i++)
            {
                if (i == 0)
                    continue;

                rayCount++;
                Vector2 castDir = dir.RotatedBy(0.015f * i);
                Vector2? endPos = RayCast(Center, castDir, castRange, out float distMoved);

                if (endPos.HasValue)
                    EndPoints.Add(endPos.Value);
                else
                    EndPoints.Add(Center + castDir * castRange);
            }
        }
    }

    private int compare(Vector2 x, Vector2 y) => (x - Main.LocalPlayer.Center).ToRotation().CompareTo((y - Main.LocalPlayer.Center).ToRotation());

    private static Vector2 WorldToWorldMatrixPos(Vector2 input) => new Vector2(1, -1) * (input / (new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f) - Vector2.One);
}
