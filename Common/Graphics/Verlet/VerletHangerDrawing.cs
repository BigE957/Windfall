using Luminance.Common.VerletIntergration;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Common.Graphics.Verlet;
public class VerletHangerDrawing : ModSystem
{
    public static readonly List<Point16> hangers = [];

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawHangerVerlets;
    }

    public override void ClearWorld()
    {
        hangers.Clear();
    }

    public override void PreUpdateProjectiles()
    {
        const float loadedDist = 1000f;

        List<HangerEntity> ActiveTEs = [];
        foreach (var p16 in hangers)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
                continue;

            if (h.State != 1 || !h.PartnerLocation.HasValue || !h.RopeID.HasValue)
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
            {
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist)
                    visable = true;
            }
            if (!visable)
                continue;

            ActiveTEs.Add(h);
        }

        if (ActiveTEs.Count == 0)
            return;

        foreach (HangerEntity te in ActiveTEs)
        {
            Vector2 StringStart = te.Position.ToWorldCoordinates();
            Vector2 StringEnd = te.PartnerLocation.Value.ToWorldCoordinates();

            if (te.MainVerlet.Count == 0)
            {
                te.MainVerlet = [];
                for (int k = 0; k < (int)te.Distance; k++)
                    te.MainVerlet.Add(new VerletSegment(Vector2.Lerp(StringStart, StringEnd, k / te.Distance), Vector2.Zero, false));
                te.MainVerlet[0].Locked = te.MainVerlet.Last().Locked = true;
            }

            Vector2[] segmentPositions = te.MainVerlet.Select(x => x.Position).ToArray();

            for (int k = 0; k < te.MainVerlet.Count; k++)
            {
                if (DecorationID.DecorationIDs.Contains(Main.LocalPlayer.HeldItem.type) && (k % 5 == 2))
                {
                    Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;

                    if (!decor.PlacingInProgress)
                    {
                        Color color = Color.White;
                        if (te.DecorationVerlets.ContainsKey(k))
                            color = Color.Red;
                        else if ((segmentPositions[k] - Main.MouseWorld).LengthSquared() < 25)
                        {
                            decor.HangIndex = k;
                            decor.StartingEntity = te;
                            color = Color.Green;
                        }

                        Particle particle = new GlowOrbParticle(segmentPositions[k], Vector2.Zero, false, 4, 0.5f, color, needed: true);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }
                }

                if (!te.MainVerlet[k].Locked)
                    te.MainVerlet[k].Position += Vector2.UnitX * (((float)Math.Sin(Main.windCounter / 13f) / 4f + 1) * Main.windSpeedCurrent * 16);
            }

            VerletSimulations.RopeVerletSimulation(te.MainVerlet, StringStart, 50f, new(Gravity: 0.5f), StringEnd);

            for (int i = 0; i < te.DecorationVerlets.Count; i++)
            {
                int index = te.DecorationVerlets.ElementAt(i).Key;
                List<VerletSegment> subVerlet = te.DecorationVerlets.ElementAt(i).Value.Item1;

                if (subVerlet.Count == 0)
                {
                    for (int k = 0; k < 20; k++)
                    {
                        te.DecorationVerlets.ElementAt(i).Value.Item1.Add(new VerletSegment(Vector2.Lerp(te.MainVerlet[index].Position, te.MainVerlet[index].Position + Vector2.UnitY * 150f, k / 10), Vector2.Zero, false));
                        te.DecorationVerlets.ElementAt(i).Value.Item1[k].OldPosition = subVerlet[k].Position;
                    }
                    te.DecorationVerlets.ElementAt(i).Value.Item1[0].Locked = true;
                    subVerlet = te.DecorationVerlets.ElementAt(i).Value.Item1;
                }

                for (int k = 0; k < subVerlet.Count; k++)
                {
                    if (!subVerlet[k].Locked)
                    {
                        if (Math.Abs(Main.windSpeedCurrent) < 0.0425f)
                            subVerlet[k].Velocity.X += (subVerlet[k].Position - subVerlet[k].OldPosition).X * 0.3f;

                        subVerlet[k].Velocity.X *= 0.925f;
                        if (Math.Abs(subVerlet[k].Velocity.X) < 0.1f)
                            subVerlet[k].Velocity.X = 0;

                        foreach (Player p in Main.ActivePlayers)
                        {
                            Rectangle hitbox = p.Hitbox;
                            hitbox.Inflate(4, 10);
                            if (hitbox.Contains((int)subVerlet[k].Position.X, (int)subVerlet[k].Position.Y))
                            {
                                subVerlet[k].Velocity.X = Math.Sign(p.velocity.X) * (p.velocity.Length());
                            }
                        }
                        float windSpeed = ((float)Math.Sin((Main.windCounter + index) / 13f) / 4f + 1) * Main.windSpeedCurrent * (2 * k);
                        subVerlet[k].Position.X += windSpeed;
                        //subVerlet[k].OldPosition.X += windSpeed;
                    }
                }

                VerletSimulations.RopeVerletSimulation(subVerlet, te.MainVerlet[index].Position, 20f, new());
            }
        }
    }

    private void DrawHangerVerlets(On_Main.orig_DrawProjectiles orig, Main self)
    {
        const float loadedDist = 1000f;

        List<HangerEntity> ActiveTEs = [];
        foreach (var p16 in hangers)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
                continue;

            if (h.State != 1 || !h.PartnerLocation.HasValue || !h.RopeID.HasValue)
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
            {
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist)
                    visable = true;
            }
            if (!visable)
                continue;

            ActiveTEs.Add(h);
        }

        //Main.NewText(ActiveTEs.Count);
        if (ActiveTEs.Count == 0)
        {
            orig(self);
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach(HangerEntity te in ActiveTEs)
        {
            Vector2[] segmentPositions;
            Twine twine = TwineID.GetTwine(te.RopeID.Value);

            foreach (Tuple<List<VerletSegment>, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
            {
                segmentPositions = Decoration.Item1.Select(x => x.Position).ToArray();

                for (int k = 0; k < segmentPositions.Length - 1; k++)
                {
                    twine.DrawDecorationSegment(Main.spriteBatch, k, segmentPositions);

                    DecorationID.GetDecoration(Decoration.Item2).DrawAlongVerlet(Main.spriteBatch, k, segmentPositions);
                }

                DecorationID.GetDecoration(Decoration.Item2).DrawOnVerletEnd(Main.spriteBatch, segmentPositions);
            }

            List<VerletSegment> TwineVerlet = te.MainVerlet;
            segmentPositions = TwineVerlet.Select(x => x.Position).ToArray();
            

            for (int k = 0; k < segmentPositions.Length - 1; k++)
                twine.DrawRopeSegment(Main.spriteBatch, k, segmentPositions);           
        }

        Main.spriteBatch.End();

        orig(self);
    }
}
