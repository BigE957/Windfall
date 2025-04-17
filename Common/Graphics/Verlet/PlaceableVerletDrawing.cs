using Luminance.Common.VerletIntergration;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
using Windfall.Content.Tiles.TileEntities;
using Windfall.Content.Items.Tools;
using CalamityMod.Particles;
using Terraria;

namespace Windfall.Common.Graphics.Verlet;
public class PlaceableVerletDrawing : ModSystem
{
    public static readonly Dictionary<string, Asset<Texture2D>> ObjectAssets = [];
    
    public struct VerletObject(Vector2 center, string key, Vector2 size, List<List<VerletSegment>> chains, int segLength = 4, int loops = 30, float gravStrength = 0.8f)
    {
        Vector2 Center = center;
        Vector2 Size = size;
        float Rotation;
        string AssetKey = key;

        readonly Rectangle Hitbox => new((int)(Center.X - Size.X / 2f), (int)(Center.Y - Size.Y / 2f), (int)Size.X, (int)Size.Y);

        readonly List<List<VerletSegment>> Chains = chains;
        int SegmentLength = segLength;
        int LoopCount = loops;
        float Gravity = gravStrength;

        void Update()
        {
            foreach (List<VerletSegment> verlet in Chains)
            {
                AffectVerlets(verlet, Hitbox, 0.125f, 0.8f);
            }

            float chainDistance = Vector2.Distance(Chains[0][^1].Position, Chains[^1][^1].Position);
            Vector2 chainToChain = (Chains[^1][^1].Position - Chains[0][^1].Position).SafeNormalize(Vector2.UnitX);

            if (chainDistance != Size.X)
            {
                float distanceDif = chainDistance - Size.X;
                Chains[0][^1].Position += chainToChain * distanceDif / 2f;
                Chains[^1][^1].Position -= chainToChain * distanceDif / 2f;
            }

            Center = Chains[0][^1].Position + (Chains[^1][^1].Position - Chains[0][^1].Position) / 2f;
            Center.Y += 8;
            Rotation = chainToChain.ToRotation();

            foreach(List<VerletSegment> verlet in Chains)
                WFVerletSimulations.CalamitySimulation(verlet, SegmentLength, LoopCount, gravity: Gravity, windAffected: false);
        }

        void Draw(SpriteBatch sb)
        {
            foreach (List<VerletSegment> verlet in Chains)
            {
                for(int i = 0; i < verlet.Count - 1; i++)
                {
                    Vector2 line = verlet[i].Position - verlet[i + 1].Position;
                    Color lighting = Lighting.GetColor((verlet[i + 1].Position + (line / 2f)).ToTileCoordinates());
                    Main.spriteBatch.DrawLineBetween(verlet[i].Position, verlet[i + 1].Position, Color.White.MultiplyRGB(lighting), 3);
                }
            }
            Main.spriteBatch.Draw(ObjectAssets[AssetKey].Value, Center, Hitbox, Lighting.GetColor(Center.ToTileCoordinates()), Rotation, ObjectAssets[AssetKey].Size() * 0.5f, 1f, 0, 0);
        }
    }

    public List<VerletObject> NpcLayerObjects = [];
    public List<VerletObject> ProjectileLayerObjects = [];

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
                {
                    te.MainVerlet = [];
                    for (int k = 0; k < te.SegmentCount; k++)
                        te.MainVerlet.Add(new VerletSegment(Vector2.Lerp(StringStart, StringEnd, k / (float)te.SegmentCount), Vector2.Zero, false));
                    te.MainVerlet[0].Locked = te.MainVerlet.Last().Locked = true;
                }

                te.MainVerlet[0].Position = StringStart;
                te.MainVerlet.Last().Position = StringEnd;

                for (int j = 0; j < Main.maxPlayers; j++)
                {
                    Player p = Main.player[j];
                    if (!p.active || p.dead)
                        continue;

                    MoveChainBasedOnEntity(te.MainVerlet, p, 1f);
                }

                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    Projectile proj = Main.projectile[j];

                    if (!proj.active || proj.hostile)
                        continue;

                    MoveChainBasedOnEntity(te.MainVerlet, proj, 1f);
                }

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

                WFVerletSimulations.CalamitySimulation(te.MainVerlet, 15, 30);
                //VerletSimulations.RopeVerletSimulation(te.MainVerlet, StringStart, te.MainVerlet.Count, new(Gravity: 0.5f), StringEnd);
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

                //subVerlet[i].Position = startPos;
                
                DecorationID.GetDecoration(te.DecorationVerlets[index].Item2).UpdateDecoration([.. subVerlet.Select(x => x.Position)]);
                for (int k = 0; k < subVerlet.Count; k++)
                {
                    if (!subVerlet[k].Locked)
                    {

                        subVerlet[k].Velocity.X += (subVerlet[k].Position - subVerlet[k].OldPosition).X * 0.4f / (1 + Math.Abs(Main.windSpeedCurrent));

                        subVerlet[k].Velocity.X *= 0.925f;
                        if (Math.Abs(subVerlet[k].Velocity.X) < 0.1f)
                            subVerlet[k].Velocity.X = 0;

                        foreach (Player p in Main.ActivePlayers)
                        {
                            Rectangle hitbox = p.Hitbox;
                            hitbox.Inflate(4, 10);
                            if (hitbox.Contains((int)subVerlet[k].Position.X, (int)subVerlet[k].Position.Y))
                                subVerlet[k].Velocity.X += Math.Sign(p.velocity.X) * p.velocity.Length() / (2.5f + Math.Abs(Main.windSpeedCurrent));
                        }

                        foreach (Projectile p in Main.ActiveProjectiles)
                        {
                            Rectangle hitbox = p.Hitbox;
                            hitbox.Inflate(4, 10);
                            if (hitbox.Contains((int)subVerlet[k].Position.X, (int)subVerlet[k].Position.Y))
                                subVerlet[k].Velocity.X += Math.Sign(p.velocity.X) * p.velocity.Length() / (2.5f + Math.Abs(Main.windSpeedCurrent));
                        }

                        if (Math.Abs(Main.windSpeedCurrent) > 0.025f)
                        {
                            float windSpeed = ((float)Math.Sin((Main.windCounter + index) / 13f) / 2f + 1f) * Main.windSpeedCurrent * (4 * k);
                            subVerlet[k].Position.X -= windSpeed;
                        }
                    }
                }

                //WFVerletSimulations.CalamitySimulation(subVerlet, 15, 60, 0.6f);
                VerletSimulations.RopeVerletSimulation(subVerlet, startPos, 12 * subVerlet.Count, new());
            }
        }
    }

    public static void AffectVerlets(List<VerletSegment> verlet, NPC npc, float dampening, float cap)
    {
        for (int k = 0; k < verlet.Count; k++)
        {
            if (!verlet[k].Locked)
            {
                foreach (Player p in Main.ActivePlayers)
                {
                    MoveChainBasedOnEntity(verlet, p, dampening, cap);

                    if (p.Hitbox.Intersects(npc.Hitbox))
                    {
                        verlet[^2].Position += p.velocity * 0.25f;
                        verlet[^1].Position += p.velocity * 0.25f;
                    }
                }

                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    MoveChainBasedOnEntity(verlet, proj, dampening, cap);

                    if (proj.Hitbox.Intersects(npc.Hitbox))
                    {
                        verlet[^2].Position += proj.velocity * 0.25f;
                        verlet[^1].Position += proj.velocity * 0.25f;
                    }
                }
            }
        }

    }
    
    public static void AffectVerlets(List<VerletSegment> verlet, Rectangle hitbox, float dampening, float cap)
    {
        for (int k = 0; k < verlet.Count; k++)
        {
            if (!verlet[k].Locked)
            {
                foreach (Player p in Main.ActivePlayers)
                {
                    MoveChainBasedOnEntity(verlet, p, dampening, cap);

                    if (p.Hitbox.Intersects(hitbox))
                    {
                        verlet[^2].Position += p.velocity * 0.25f;
                        verlet[^1].Position += p.velocity * 0.25f;
                    }
                }

                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    MoveChainBasedOnEntity(verlet, proj, dampening, cap);

                    if (proj.Hitbox.Intersects(hitbox))
                    {
                        verlet[^2].Position += proj.velocity * 0.25f;
                        verlet[^1].Position += proj.velocity * 0.25f;
                    }
                }
            }
        }

    }

    public static void MoveChainBasedOnEntity(List<VerletSegment> chain, Entity e, float dampening = 0.425f, float cap = 5f)
    {
        // Cap the velocity to ensure it doesn't make the chains go flying.
        Vector2 entityVelocity = (e.velocity * dampening).ClampMagnitude(0f, cap);

        for (int i = 1; i < chain.Count - 1; i++)
        {
            VerletSegment segment = chain[i];
            VerletSegment next = chain[i + 1];

            // Check to see if the entity is between two verlet segments via line/box collision checks.
            // If they are, add the entity's velocity to the two segments relative to how close they are to each of the two.
            float _ = 0f;
            if (Collision.CheckAABBvLineCollision(e.TopLeft, e.Size, segment.Position, next.Position, 20f, ref _))
            {
                // Weigh the entity's distance between the two segments.
                // If they are close to one point that means the strength of the movement force applied to the opposite segment is weaker, and vice versa.
                float distanceBetweenSegments = segment.Position.Distance(next.Position);
                float distanceToChains = e.Distance(segment.Position);
                float currentMovementOffsetInterpolant = Utilities.InverseLerp(distanceToChains, distanceBetweenSegments, distanceBetweenSegments * 0.2f);
                float nextMovementOffsetInterpolant = 1f - currentMovementOffsetInterpolant;

                // Move the segments based on the weight values.
                segment.Position += entityVelocity * currentMovementOffsetInterpolant;
                if (!next.Locked)
                    next.Position += entityVelocity * nextMovementOffsetInterpolant;

                /*
                // Play some cool chain sounds.
                if (te.soundDelay <= 0 && entityVelocity.Length() >= 0.1f)
                {
                    SoundEngine.PlaySound(InfernumSoundRegistry.CeaselessVoidChainSound with { Volume = 0.25f, PitchVariance = 0.05f }, e.Center);
                    npc.soundDelay = 27;
                }
                */
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

            foreach (Tuple<List<VerletSegment>, int, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
            {
                segmentPositions = [.. Decoration.Item1.Select(x => x.Position)];

                for (int k = 0; k < segmentPositions.Length; k++)
                {
                    if (twine != null)
                        twine.DrawDecorationSegment(Main.spriteBatch, k, segmentPositions);
                    else if (k != segmentPositions.Length - 1)
                    {                       
                        Vector2 line = segmentPositions[k] - segmentPositions[k + 1];
                        Color lighting = Lighting.GetColor((segmentPositions[k + 1] + (line / 2f)).ToTileCoordinates());

                        Main.spriteBatch.DrawLineBetween(segmentPositions[k], segmentPositions[k + 1], Color.White.MultiplyRGB(lighting), 3);
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
                    segmentPositions = [.. TwineVerlet.Select(x => x.Position)];

                    if (twine != null)
                    {
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[0], (segmentPositions[1] - segmentPositions[0]).ToRotation());
                        twine.DrawOnRopeEnds(Main.spriteBatch, segmentPositions[^1], (segmentPositions[^2] - segmentPositions[^1]).ToRotation());
                    }

                    for (int k = 0; k < segmentPositions.Length; k++)
                        if(k != 1)
                            twine.DrawRopeSegment(Main.spriteBatch, k, segmentPositions);
                }
            }

            if (twine != null)
                foreach (Tuple<List<VerletSegment>, int, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
                {
                    segmentPositions = [.. Decoration.Item1.Select(x => x.Position)];
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
