using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
using Windfall.Content.Tiles.TileEntities;
using Windfall.Content.Items.Tools;
using CalamityMod.Particles;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Common.Graphics.Verlet;
public class PlaceableVerletDrawing : ModSystem
{
    public static readonly Dictionary<string, Asset<Texture2D>> ObjectAssets = [];

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawHangerVerlets;

        if (!Main.dedServ)
        {
            ObjectAssets.Add("DragonSkull", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonSkull", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonRibs", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonRibs", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonFrontWing", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonFrontWing", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonBackWing", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonBackWing", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonFrontLeg", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonFrontLeg", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonBackLeg", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonBackLeg", AssetRequestMode.AsyncLoad));
            ObjectAssets.Add("DragonTail", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonTail", AssetRequestMode.AsyncLoad));
        }
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
            {
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

                if (te.MainVerlet.Count != te.SegmentCount)
                    te.MainVerlet = CreateVerletChain(StringStart, StringEnd, te.SegmentCount, 15, lockEnd: true);

                te.MainVerlet[0].Position = StringStart;
                te.MainVerlet[^1].Position = StringEnd;

                AffectVerlets(te.MainVerlet, 1f, 0.425f);

                Vector2[] segmentPositions = [.. te.MainVerlet.Select(x => x.Position)];

                for (int k = 0; k < te.MainVerlet.Count; k++)
                {
                    if (k % 5 == 2)
                    {
                        particle = null;
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
                }

                VerletSimulation(te.MainVerlet, 30);
            }
            
            for (int i = 0; i < te.DecorationVerlets.Count; i++)
            {
                int index = te.DecorationVerlets.ElementAt(i).Key;
                List<VerletPoint> subVerlet = te.DecorationVerlets[index].chain;
                Vector2 startPos;
                if (index == -1)
                    startPos = te.Position.ToWorldCoordinates();
                else
                    startPos = te.MainVerlet[index].Position;

                if (subVerlet.Count != te.DecorationVerlets[index].segmentCount)
                {
                    te.DecorationVerlets[index].chain.Clear();
                    te.DecorationVerlets[index].chain.AddRange(CreateVerletChain(startPos, startPos + Vector2.UnitY * (12 * te.DecorationVerlets[index].segmentCount), te.DecorationVerlets[index].segmentCount, 15));
                    subVerlet = te.DecorationVerlets[index].chain;
                }
                
                DecorationID.GetDecoration(te.DecorationVerlets[index].decorationID).UpdateDecoration([.. subVerlet.Select(x => x.Position)]);
                
                AffectVerlets(subVerlet, 0.125f, 0.8f);

                VerletSimulation(subVerlet, 10, 0.6f);
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

            foreach ((List<VerletPoint> chain, int decorationID, int segementCount) in te.DecorationVerlets.Values.Where(v => v.chain.Count > 0))
            {
                segmentPositions = [.. chain.Select(x => x.Position)];

                for (int k = 0; k < segmentPositions.Length; k++)
                {
                    if (twine != null)
                        twine.DrawDecorationSegment(Main.spriteBatch, chain, k);
                    else if (k != segmentPositions.Length - 1)
                    {                       
                        Vector2 line = segmentPositions[k] - segmentPositions[k + 1];
                        Color lighting = Lighting.GetColor((segmentPositions[k + 1] + (line / 2f)).ToTileCoordinates());

                        Main.spriteBatch.DrawLineBetween(segmentPositions[k], segmentPositions[k + 1], Color.White.MultiplyRGB(lighting), 3);
                    }                  
                }

                for(int k = 0; k < segmentPositions.Length; k++)
                    DecorationID.GetDecoration(decorationID).DrawAlongVerlet(Main.spriteBatch, k, segmentPositions);

                DecorationID.GetDecoration(decorationID).DrawOnVerletEnd(Main.spriteBatch, segmentPositions);
            }

            if (te.State == 1)
            {
                if (te.MainVerlet.Count > 0)
                {
                    List<VerletPoint> TwineVerlet = te.MainVerlet;
                    segmentPositions = [.. TwineVerlet.Select(x => x.Position)];

                    if (twine != null)
                    {
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[0], (segmentPositions[1] - segmentPositions[0]).ToRotation());
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[^1], (segmentPositions[^2] - segmentPositions[^1]).ToRotation());
                    }

                    for (int k = 0; k < segmentPositions.Length; k++)
                        twine.DrawRopeSegment(Main.spriteBatch, TwineVerlet, k);
                }
            }

            if (twine != null)
                foreach ((List<VerletPoint> chain, int, int) Decoration in te.DecorationVerlets.Values.Where(v => v.chain.Count > 0))
                {
                    segmentPositions = [.. Decoration.chain.Select(x => x.Position)];
                    twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[0], (segmentPositions[1] - segmentPositions[0]).ToRotation());
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
