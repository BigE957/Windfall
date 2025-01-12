using Luminance.Common.VerletIntergration;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
using Windfall.Content.Tiles.TileEntities;
using Windfall.Content.Items.Tools;

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
        List<Point16> PointsForRemoval = [];
        foreach (var p16 in hangers)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
            {
                PointsForRemoval.Add(p16);
                continue;
            }

            if ((h.State != 1 || !h.PartnerLocation.HasValue) && !h.DecorationVerlets.ContainsKey(-1))
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
            {
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || h.PartnerLocation.HasValue && (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist)
                    visable = true;
            }
            if (!visable)
                continue;

            ActiveTEs.Add(h);
        }

        foreach (Point16 p16 in PointsForRemoval)
            hangers.Remove(p16);
        PointsForRemoval.Clear();

        if (ActiveTEs.Count == 0)
            return;

        foreach (HangerEntity te in ActiveTEs)
        {
            if (te.State == 1 && te.PartnerLocation.HasValue)
            {
                Vector2 StringStart = te.Position.ToWorldCoordinates();
                Vector2 StringEnd = te.PartnerLocation.Value.ToWorldCoordinates();

                if (te.MainVerlet.Count != te.SegmentCount)
                {
                    te.MainVerlet = [];
                    for (int k = 0; k < te.SegmentCount; k++)
                        te.MainVerlet.Add(new VerletSegment(Vector2.Lerp(StringStart, StringEnd, k / (float)te.SegmentCount), Vector2.Zero, false));
                    te.MainVerlet[0].Locked = te.MainVerlet.Last().Locked = true;
                }

                Vector2[] segmentPositions = te.MainVerlet.Select(x => x.Position).ToArray();

                for (int k = 0; k < te.MainVerlet.Count; k++)
                {
                    if (k % 5 == 2)
                    {
                        Particle particle = null;
                        if (DecorationID.DecorationTypes.Contains(Main.LocalPlayer.HeldItem.type))
                        {
                            Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if (te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((segmentPositions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                decor.HangIndex = k;
                                decor.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(segmentPositions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }
                        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordShears>())
                        {
                            CordShears shears = (CordShears)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if(!te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((segmentPositions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                shears.HangIndex = k;
                                shears.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(segmentPositions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }
                        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordageMeter>())
                        {
                            CordageMeter meter = (CordageMeter)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if (!te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((segmentPositions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                meter.HangIndex = k;
                                meter.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(segmentPositions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }

                        if(particle != null)
                            GeneralParticleHandler.SpawnParticle(particle);
                    }

                    if (!te.MainVerlet[k].Locked)
                        te.MainVerlet[k].Position += Vector2.UnitX * (((float)Math.Sin(Main.windCounter / 13f) / 4f + 1) * Main.windSpeedCurrent * 16);
                }

                VerletSimulations.RopeVerletSimulation(te.MainVerlet, StringStart, te.MainVerlet.Count, new(Gravity: 0.5f), StringEnd);
            }
            for (int i = 0; i < te.DecorationVerlets.Count; i++)
            {
                int index = te.DecorationVerlets.ElementAt(i).Key;
                List<VerletSegment> subVerlet = te.DecorationVerlets[index].Item1;
                Vector2 startPos;
                if (index == -1)
                    startPos = te.Position.ToWorldCoordinates();
                else
                    startPos = te.MainVerlet[index].Position;

                if (subVerlet.Count != te.DecorationVerlets[index].Item3)
                {
                    te.DecorationVerlets[index].Item1.Clear();
                    for (int k = 0; k < te.DecorationVerlets[index].Item3; k++)
                    {
                        te.DecorationVerlets[index].Item1.Add(new VerletSegment(Vector2.Lerp(startPos, startPos + Vector2.UnitY * (12 * te.DecorationVerlets[index].Item3), k / (float)te.DecorationVerlets[index].Item3), Vector2.Zero, false));
                        te.DecorationVerlets[index].Item1[k].OldPosition = subVerlet[k].Position;
                        te.DecorationVerlets[index].Item1[k].Velocity.Y = 19;
                    }
                    te.DecorationVerlets[index].Item1[0].Locked = true;
                    subVerlet = te.DecorationVerlets[index].Item1;
                }

                DecorationID.GetDecoration(te.DecorationVerlets[index].Item2).UpdateDecoration(subVerlet.Select(x => x.Position).ToArray());

                for (int k = 0; k < subVerlet.Count; k++)
                {
                    if (!subVerlet[k].Locked)
                    {
                        if (Math.Abs(Main.windSpeedCurrent) < 0.05f)
                            subVerlet[k].Velocity.X += (subVerlet[k].Position - subVerlet[k].OldPosition).X * 0.3f;

                        subVerlet[k].Velocity.X *= 0.925f;
                        if (Math.Abs(subVerlet[k].Velocity.X) < 0.1f)
                            subVerlet[k].Velocity.X = 0;

                        foreach (Player p in Main.ActivePlayers)
                        {
                            Rectangle hitbox = p.Hitbox;
                            hitbox.Inflate(4, 10);
                            if (hitbox.Contains((int)subVerlet[k].Position.X, (int)subVerlet[k].Position.Y))
                                subVerlet[k].Velocity.X = Math.Sign(p.velocity.X) * (p.velocity.Length());
                        }
                        if (Math.Abs(Main.windSpeedCurrent) >= 0.05f)
                        {
                            float windSpeed = ((float)Math.Sin((Main.windCounter + index) / 13f) / 4f + 1) * Main.windSpeedCurrent * (2 * k);
                            subVerlet[k].Position.X += windSpeed;
                        }
                        //subVerlet[k].OldPosition.X += windSpeed;
                    }
                }

                VerletSimulations.RopeVerletSimulation(subVerlet, startPos, 12 * subVerlet.Count, new());
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

            if ((h.State != 1 || !h.PartnerLocation.HasValue) && !h.DecorationVerlets.ContainsKey(-1))
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
            {
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || (h.PartnerLocation.HasValue && (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist))
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
            Cord twine = te.CordID.HasValue ? CordID.GetTwine(te.CordID.Value) : null;
            if(twine == null && te.PartnerLocation.HasValue)
            {
                HangerEntity partner = FindTileEntity<HangerEntity>(te.PartnerLocation.Value.X, te.PartnerLocation.Value.Y, 1, 1);
                if (partner != null)
                    twine = partner.CordID.HasValue ? CordID.GetTwine(partner.CordID.Value) : null;
            }

            foreach (Tuple<List<VerletSegment>, int, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
            {
                segmentPositions = Decoration.Item1.Select(x => x.Position).ToArray();

                for (int k = 0; k < segmentPositions.Length; k++)
                {
                    if (twine != null)
                        twine.DrawDecorationSegment(Main.spriteBatch, k, segmentPositions);
                    else if (k != segmentPositions.Length - 1)
                    {                       
                        Vector2 line = segmentPositions[k] - segmentPositions[k + 1];
                        Color lighting = Lighting.GetColor((segmentPositions[k + 1] + (line / 2f)).ToTileCoordinates());

                        Main.spriteBatch.DrawLineBetter(segmentPositions[k], segmentPositions[k + 1], Color.White.MultiplyRGB(lighting), 3);
                    }                  
                }

                for(int k = 0; k < segmentPositions.Length; k++)
                    DecorationID.GetDecoration(Decoration.Item2).DrawAlongVerlet(Main.spriteBatch, k, segmentPositions);

                DecorationID.GetDecoration(Decoration.Item2).DrawOnVerletEnd(Main.spriteBatch, segmentPositions);
            }

            if (te.State == 1)
            {
                if (te.MainVerlet.Count > 0)
                {
                    List<VerletSegment> TwineVerlet = te.MainVerlet;
                    segmentPositions = TwineVerlet.Select(x => x.Position).ToArray();

                    if (twine != null)
                    {
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[0], (segmentPositions[1] - segmentPositions[0]).ToRotation());
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[^1], (segmentPositions[^2] - segmentPositions[^1]).ToRotation());
                    }

                    for (int k = 0; k < segmentPositions.Length; k++)
                        twine.DrawRopeSegment(Main.spriteBatch, k, segmentPositions);
                }
            }

            if (twine != null)
                foreach (Tuple<List<VerletSegment>, int, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
                {
                    segmentPositions = Decoration.Item1.Select(x => x.Position).ToArray();
                    twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[0], (segmentPositions[1] - segmentPositions[0]).ToRotation());
                }
        }

        Main.spriteBatch.End();

        orig(self);
    }
}
