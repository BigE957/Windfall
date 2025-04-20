using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
using Windfall.Content.Tiles.TileEntities;
using Windfall.Content.Items.Tools;
using CalamityMod.Particles;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Common.Graphics.Verlet;
public class PlaceableVerletDrawing : ModSystem
{

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawHangerVerlets;
    }

    public override void PreUpdateProjectiles()
    {       
        if (Main.dedServ)
            return;

        const float loadedDist = 1000f;

        List<HangerEntity> ActiveTEs = [];
        foreach (var p16 in TileEntity.ByPosition.Keys)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
                continue;

            if ((h.State != 1 || !h.PartnerLocation.HasValue) && !h.DecorationVerlets.ContainsKey(-1))
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || h.PartnerLocation.HasValue && (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist)
                    visable = true;
            
            if (!visable)
                continue;

            ActiveTEs.Add(h);
        }

        if (ActiveTEs.Count == 0)
            return;

        foreach (HangerEntity te in ActiveTEs)
        {
            #region TileEntity Item Highlights
            Particle particle = null;

            HandleTileEntityItemInteractions(te, ref particle);

            if (particle != null)
                GeneralParticleHandler.SpawnParticle(particle);
            #endregion

            if (te.State == 1 && te.PartnerLocation.HasValue)
            {
                Point16 p16 = te.PartnerLocation.Value;
                HandleTileEntityItemInteractions(FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1), ref particle);

                if (particle != null)
                    GeneralParticleHandler.SpawnParticle(particle);

                Vector2 StringStart = te.Position.ToWorldCoordinates();
                Vector2 StringEnd = te.PartnerLocation.Value.ToWorldCoordinates();

                if (te.MainVerlet == null || te.MainVerlet.Count != te.SegmentCount)
                    te.MainVerlet = CreateVerletChain(StringStart, StringEnd, te.SegmentCount, 15, lockEnd: true);

                te.MainVerlet[0].Position = StringStart;
                te.MainVerlet[^1].Position = StringEnd;

                AffectVerletObject(te.MainVerlet, 1f, 0.425f);

                for (int k = 0; k < te.MainVerlet.Count; k++)
                    if (k % 5 == 2)
                    {
                        particle = null;
                        if (DecorationID.DecorationTypes.Contains(Main.LocalPlayer.HeldItem.type))
                        {
                            Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if (te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((te.MainVerlet.Positions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                decor.HangIndex = k;
                                decor.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(te.MainVerlet.Positions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }
                        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordShears>())
                        {
                            CordShears shears = (CordShears)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if(!te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((te.MainVerlet.Positions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                shears.HangIndex = k;
                                shears.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(te.MainVerlet.Positions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }
                        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordageMeter>())
                        {
                            CordageMeter meter = (CordageMeter)Main.LocalPlayer.HeldItem.ModItem;

                            Color color = Color.White;
                            if (!te.DecorationVerlets.ContainsKey(k))
                                color = Color.Red;
                            else if ((te.MainVerlet.Positions[k] - Main.MouseWorld).LengthSquared() < 25)
                            {
                                meter.HangIndex = k;
                                meter.HE = te;
                                color = Color.Green;
                            }

                            particle = new GlowOrbParticle(te.MainVerlet.Positions[k], Vector2.Zero, false, 2, 0.5f, color, needed: true);
                        }

                        if(particle != null)
                            GeneralParticleHandler.SpawnParticle(particle);
                    }

                VerletSimulation(te.MainVerlet, 30);
            }
            
            for (int i = 0; i < te.DecorationVerlets.Count; i++)
            {
                int index = te.DecorationVerlets.ElementAt(i).Key;
                VerletObject subVerlet = te.DecorationVerlets[index].chain;
                Vector2 startPos;
                if (index == -1)
                    startPos = te.Position.ToWorldCoordinates();
                else
                    startPos = te.MainVerlet[index].Position;

                if (subVerlet == null || subVerlet.Count != te.DecorationVerlets[index].segmentCount)
                {
                    te.DecorationVerlets[index] = new(CreateVerletChain(startPos, startPos + Vector2.UnitY * (12 * te.DecorationVerlets[index].segmentCount), te.DecorationVerlets[index].segmentCount, 15), te.DecorationVerlets[index].decorationID, te.DecorationVerlets[index].segmentCount);
                    subVerlet = te.DecorationVerlets[index].chain;
                }

                subVerlet[0].Position = startPos;

                DecorationID.GetDecoration(te.DecorationVerlets[index].decorationID).UpdateDecoration(subVerlet.Positions);
                
                AffectVerletObject(subVerlet, 0.125f, 0.8f);

                VerletSimulation(subVerlet, 10, 0.8f);
            }
        }
    }

    private void DrawHangerVerlets(On_Main.orig_DrawProjectiles orig, Main self)
    {
        const float loadedDist = 1000f;

        List<HangerEntity> ActiveTEs = [];
        foreach (var p16 in TileEntity.ByPosition.Keys)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
                continue;

            if ((h.State != 1 || !h.PartnerLocation.HasValue) && !h.DecorationVerlets.ContainsKey(-1))
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || (h.PartnerLocation.HasValue && (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist))
                    visable = true;

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
            Cord twine = te.CordID.HasValue ? CordID.GetTwine(te.CordID.Value) : null;
            if(twine == null && te.PartnerLocation.HasValue)
            {
                HangerEntity partner = FindTileEntity<HangerEntity>(te.PartnerLocation.Value.X, te.PartnerLocation.Value.Y, 1, 1);
                if (partner != null)
                    twine = partner.CordID.HasValue ? CordID.GetTwine(partner.CordID.Value) : null;
            }

            foreach ((VerletObject obj, int decorationID, int segementCount) in te.DecorationVerlets.Values.Where(v => v.chain != null && v.chain.Count > 0))
            {
                for (int k = 0; k < obj.Count; k++)
                {
                    if (twine != null)
                        twine.DrawDecorationSegment(Main.spriteBatch, obj.Points, k);
                    else if (k != obj.Count - 1)
                    {                       
                        Vector2 line = obj.Positions[k] - obj.Positions[k + 1];
                        Color lighting = Lighting.GetColor((obj.Positions[k + 1] + (line / 2f)).ToTileCoordinates());

                        Main.spriteBatch.DrawLineBetween(obj.Positions[k], obj.Positions[k + 1], Color.White.MultiplyRGB(lighting), 3);
                    }                  
                }

                for(int k = 0; k < obj.Count; k++)
                    DecorationID.GetDecoration(decorationID).DrawAlongVerlet(Main.spriteBatch, k, obj.Positions);

                DecorationID.GetDecoration(decorationID).DrawOnVerletEnd(Main.spriteBatch, obj.Positions);
            }

            if (te.State == 1 && te.MainVerlet != null && te.MainVerlet.Count > 0)
            {
                VerletObject obj = te.MainVerlet;

                if (twine != null)
                {
                    twine.DrawOnRopeEnds(Main.spriteBatch, obj.Positions[0], (obj.Positions[1] - obj.Positions[0]).ToRotation());
                    twine.DrawOnRopeEnds(Main.spriteBatch, obj.Positions[^1], (obj.Positions[^2] - obj.Positions[^1]).ToRotation());
                }

                for (int k = 0; k < obj.Count; k++)
                    twine.DrawRopeSegment(Main.spriteBatch, obj.Points, k);
            }

            if (twine != null)
                foreach ((VerletObject obj, int decorationID, int segmentCount) in te.DecorationVerlets.Values.Where(v => v.chain != null && v.chain.Count > 0))
                {
                    if (obj == null)
                        continue;
                    twine.DrawOnRopeEnds(Main.spriteBatch, obj.Positions[0], (obj.Positions[1] - obj.Positions[0]).ToRotation());
                }
        }

        Main.spriteBatch.End();

        orig(self);
    }

    private static void HandleTileEntityItemInteractions(HangerEntity te, ref Particle particle)
    {
        if (DecorationID.DecorationTypes.Contains(Main.LocalPlayer.HeldItem.type))
        {
            Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;
            Vector2 worldPos = te.Position.ToWorldCoordinates();

            Color color = Color.White;
            if (te.DecorationVerlets.ContainsKey(-1))
                color = Color.Red;
            else if ((worldPos - Main.MouseWorld).LengthSquared() < 25)
            {
                decor.HangIndex = -2;
                decor.HE = te;
                color = Color.Green;
            }

            particle = new GlowOrbParticle(worldPos, Vector2.Zero, false, 2, 0.5f, color, needed: true);
        }
        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordShears>())
        {
            CordShears shears = (CordShears)Main.LocalPlayer.HeldItem.ModItem;
            Vector2 worldPos = te.Position.ToWorldCoordinates();

            Color color = Color.White;
            if (!(te.DecorationVerlets.ContainsKey(-1) || te.State != 0))
            {
                color = Color.Red;
            }
            else if ((worldPos - Main.MouseWorld).LengthSquared() < 25)
            {
                shears.HangIndex = -2;
                shears.HE = te;
                color = Color.Green;
            }

            particle = new GlowOrbParticle(worldPos, Vector2.Zero, false, 2, 0.5f, color, needed: true);
        }
        else if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<CordageMeter>())
        {
            CordageMeter meter = (CordageMeter)Main.LocalPlayer.HeldItem.ModItem;
            Vector2 worldPos = te.Position.ToWorldCoordinates();

            Color color = Color.White;
            if (!(te.DecorationVerlets.ContainsKey(-1) || te.State != 0))
            {
                color = Color.Red;
            }
            else if ((worldPos - Main.MouseWorld).LengthSquared() < 25)
            {
                meter.HangIndex = -2;
                meter.HE = te;
                color = Color.Green;
            }

            particle = new GlowOrbParticle(worldPos, Vector2.Zero, false, 2, 0.5f, color, needed: true);
        }
    }
}
