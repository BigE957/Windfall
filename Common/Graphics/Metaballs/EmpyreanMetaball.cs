using CalamityMod.Graphics.Metaballs;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Windfall.Common.Interfaces;
using Windfall.Content.Items.Weapons.Magic;
using Windfall.Content.Items.Weapons.Ranged;
using Windfall.Content.Items.Weapons.Summon;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.Debug;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Common.Graphics.Metaballs;

public class EmpyreanMetaball : Metaball
{
    public class EmpyreanParticle(Vector2 center, Vector2 velocity, float size)
    {
        public float Size = size;

        public Vector2 Velocity = velocity;

        public Vector2 Center = center;

        public void Update()
        {
            Velocity *= 0.99f;
            Size *= 0.9f;
            Center += Velocity;
        }
    }
    
    public class EmpyreanBorderParticle(Projectile projectile, Vector2 offset, float sineOffset, float interpolant, float size, float rotation, bool spin)
    {
        public float Size = size;

        public int ProjectileIndex = projectile.whoAmI;

        public Vector2 Offset = offset;

        public float SineOffset = sineOffset;

        public float Interpolant = interpolant;

        public float Rotation = rotation;

        public Vector2 Center;

        public bool spin = spin;

        public void Update()
        {
            Projectile myProj = Main.projectile[ProjectileIndex];

            if (spin)
                if (Rotation > 0)
                    Rotation += 0.0175f / Math.Abs(Interpolant / 4);
                else
                    Rotation -= 0.0175f / Math.Abs(Interpolant / 4);

            Center = myProj.Center + Offset + (new Vector2(myProj.width / 2 * (myProj.scale / 5f) * 1.05f, 0).RotatedBy(Rotation + myProj.rotation));
            Center += (myProj.Center + Offset - Center).SafeNormalize(Vector2.Zero) * SumofSines(this, 1.5f, 2f);
            if (!spin)
                Center -= myProj.velocity / 2;
        }
    }

    public class OpulentFlake(Vector2 relativePosition, Vector2 velocity, float spin, float scale, float idealDir, bool isFront)
    {
        internal Vector2 Position = relativePosition;
        internal Vector2 Velocity = velocity;
        internal float Scale = scale;
        internal float Rotation;
        internal int Time = 0;
        internal float Opacity = 0f;
        internal bool Front = isFront;
        internal int SineOffset = Main.rand.Next(100);

        private readonly float Spin = spin;
        private readonly float IdealAngle = idealDir;
        private readonly int Shade = isFront ? Main.rand.Next(2) : Main.rand.Next(2) + 1;
        private readonly int Variant = Main.rand.Next(6);

        internal static Asset<Texture2D> Atlas;
        internal static Asset<Texture2D> overlay;

        public void Update()
        {
            if (Time < 8)
                Opacity += 1 / 8f;
            else
            {
                if(Time == 8)
                    Opacity = 1f;

                if (Time > 8)
                    Velocity = Velocity.RotateTowards(IdealAngle, 0.01f);

                if (Time > 45)
                {
                    Scale *= 0.985f;
                    if (Time > 75)
                        Opacity -= 1 / 15f;
                }
            }

            Position += Velocity;
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);

            Velocity.Y *= 0.975f;
            Velocity.X += (float)(Math.Sin(Time + SineOffset) / 2f + 0.5f) * 0.2f * Main.windSpeedCurrent;

            Time++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = Atlas.Value;

            Rectangle frame = texture.Frame(3, 6, Shade, Variant);

            spriteBatch.Draw(texture, Position - Main.screenPosition, frame, Color.Lerp(Color.White, Lighting.GetColor(Position.ToTileCoordinates()), Time / 45f) * Opacity, Rotation, frame.Size() * 0.5f, Scale, 0, 0f);
        }
    }


    private static List<Asset<Texture2D>> layerAssets;

    public static List<EmpyreanParticle> EmpyreanParticles
    {
        get;
        private set;
    } = [];

    public static List<EmpyreanBorderParticle> EmpyreanStickyParticles
    {
        get;
        private set;
    } = [];

    public static List<OpulentFlake> OpulentFlakeFrontParticles
    {
        get;
        private set;
    } = [];

    public static List<OpulentFlake> OpulentFlakeBackParticles
    {
        get;
        private set;
    } = [];

    public override bool AnythingToDraw =>
        EmpyreanParticles.Count != 0 ||
        EmpyreanStickyParticles.Count != 0 ||
        OpulentFlakeFrontParticles.Count != 0 ||
        OpulentFlakeBackParticles.Count != 0 ||
        AnyProjectiles(ModContent.ProjectileType<DarkGlob>()) ||
        AnyProjectiles(ModContent.ProjectileType<SelenicIdol>()) ||
        AnyProjectiles(ModContent.ProjectileType<EmpyreanThorn>()) ||
        AnyProjectiles(ModContent.ProjectileType<DarkCoalescence>()) ||
        AnyProjectiles(ModContent.ProjectileType<OratorBorder>()) ||
        AnyProjectiles(ModContent.ProjectileType<DarkTide>()) ||
        AnyProjectiles(ModContent.ProjectileType<OratorHandMinion>()) ||
        AnyProjectiles(ModContent.ProjectileType<HandRing>()) ||
        AnyProjectiles(ModContent.ProjectileType<UnstableDarkness>()) ||
        AnyProjectiles(ModContent.ProjectileType<FingerlingGun>()) ||
        AnyProjectiles(ModContent.ProjectileType<PotGlob>()) ||
        AnyProjectiles(ModContent.ProjectileType<MinionHandRing>()) ||
        AnyProjectiles(ModContent.ProjectileType<SelenicIdolMinion>()) ||
        AnyProjectiles(ModContent.ProjectileType<Dissolver>()) ||
        AnyProjectiles(ModContent.ProjectileType<WretchedFountain>()) ||
        AnyProjectiles(ModContent.ProjectileType<ShadowGrasp>()) ||
        NPC.AnyNPCs(ModContent.NPCType<OratorHand>()) ||
        NPC.AnyNPCs(ModContent.NPCType<SealingTablet>())
    ;

    public override IEnumerable<Texture2D> Layers
    {
        get
        {
            for (int i = 0; i < layerAssets.Count; i++)
                yield return layerAssets[i].Value;
        }
    }

    public override MetaballDrawLayer DrawContext => MetaballDrawLayer.AfterProjectiles;

    public static float BorderLerpValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1f) / 0.5f) + 0.5f;

    public static Color BorderColor => Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), BorderLerpValue);

    public override Color EdgeColor => BorderColor;

    public override void Load()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        OpulentFlake.Atlas = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Particles/OpulentFlakesAtlas");
        OpulentFlake.overlay = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Particles/OpulentFlakesOverlay");

        // Load layer assets.
        layerAssets = [];

        for(int i = 0; i < 4; i++)
            layerAssets.Add(ModContent.Request<Texture2D>($"Windfall/Assets/Graphics/Metaballs/Empyrean_Metaball/EmpyreanLayer{i+1}"));
    }

    public override void ClearInstances()
    {
        EmpyreanParticles.Clear();
        EmpyreanStickyParticles.Clear();
    }
    
    public static void SpawnDefaultParticle(Vector2 position, Vector2 velocity, float size) =>
        EmpyreanParticles.Add(new(position, velocity, size));
    
    public static void SpawnBorderParticle(Projectile projectile, Vector2 offset, float sineOffset, float interpolant, float size, float rotation, bool spin = true) =>
       EmpyreanStickyParticles.Add(new(projectile, offset, sineOffset, interpolant, size, rotation, spin));
   
    public static void SpawnFlakeParticle(Vector2 position, Vector2 velocity, float spin, float scale, float idealDir)
    {
        if (Main.rand.NextBool())
            OpulentFlakeBackParticles.Add(new(position, velocity, spin, scale, idealDir, false));
        else
            OpulentFlakeFrontParticles.Add(new(position, velocity, spin, scale, idealDir, true));
    }

    public override void Update()
    {
        // Update all particle instances.
        // Once sufficiently small, they vanish.
        foreach (EmpyreanParticle particle in EmpyreanParticles)
            particle.Update();      
        if (EmpyreanParticles.Count != 0)
            EmpyreanParticles.RemoveAll(p => p.Size <= 2.5f);
        
        foreach (EmpyreanBorderParticle particle in EmpyreanStickyParticles)
            particle.Update(); 
        if(EmpyreanStickyParticles.Count != 0)
            EmpyreanStickyParticles.RemoveAll(p => !Main.projectile.IndexInRange(p.ProjectileIndex) || !Main.projectile[p.ProjectileIndex].active);

        foreach (OpulentFlake flake in OpulentFlakeFrontParticles)
            flake.Update();
        if (OpulentFlakeFrontParticles.Count != 0)
            OpulentFlakeFrontParticles.RemoveAll(p => p.Time > 90);

        foreach (OpulentFlake flake in OpulentFlakeBackParticles)
            flake.Update();
        if (OpulentFlakeBackParticles.Count != 0)
            OpulentFlakeBackParticles.RemoveAll(p => p.Time > 90);
    }

    public override void PrepareSpriteBatch(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);
    }
    
    public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
    {
        switch (layerIndex)
        {
            case 0:
                return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.03f;

            case 1:
                return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.06f + Vector2.UnitY * (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.03f;
            case 2:
                Vector2 offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.04f) * 1f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.76f;
                return offset;
            case 3:
                offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.045f) * 1f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.84f;
                return offset;
            case 4:
                offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.05f) * 1f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.97f;
                return offset;
        }
        return Vector2.Zero;
    }

    public override void DrawInstances()
    {
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

        // Draw all particles.
        foreach (EmpyreanParticle particle in EmpyreanParticles)
        {
            Vector2 drawPosition = particle.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 scale = Vector2.One * particle.Size / tex.Size();
            Main.spriteBatch.Draw(tex, drawPosition, null, Color.White, 0f, origin, scale, 0, 0f);
        }
        foreach (EmpyreanBorderParticle particle in EmpyreanStickyParticles)
        {
            Vector2 drawPosition = particle.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 scale = Vector2.One * particle.Size / tex.Size();
            Main.spriteBatch.Draw(tex, drawPosition, null, Color.White, 0f, origin, scale, 0, 0f);
        }
        
        foreach (Projectile p in Main.projectile.Where(p => p.active && (
            p.type == ModContent.ProjectileType<DarkGlob>() || 
            p.type == ModContent.ProjectileType<SelenicIdol>() || 
            p.type == ModContent.ProjectileType<EmpyreanThorn>() || 
            p.type == ModContent.ProjectileType<DarkCoalescence>() || 
            p.type == ModContent.ProjectileType<OratorBorder>() || 
            p.type == ModContent.ProjectileType<DarkTide>() ||
            p.type == ModContent.ProjectileType<OratorHandMinion>() ||
            p.type == ModContent.ProjectileType<HandRing>() ||
            p.type == ModContent.ProjectileType<UnstableDarkness>() ||
            p.type == ModContent.ProjectileType<FingerlingGun>() ||
            p.type == ModContent.ProjectileType<PotGlob>() ||
            p.type == ModContent.ProjectileType<MinionHandRing>() ||
            p.type == ModContent.ProjectileType<SelenicIdolMinion>() ||
            p.type == ModContent.ProjectileType<Dissolver>() ||
            p.type == ModContent.ProjectileType<WretchedFountain>() ||
            p.type == ModContent.ProjectileType<ShadowGrasp>()
        )))
        {
            if(p.type == ModContent.ProjectileType<Dissolver>() || p.type == ModContent.ProjectileType<SelenicIdolMinion>() || p.type == ModContent.ProjectileType<OratorHandMinion>() || p.type == ModContent.ProjectileType<MinionHandRing>() ||  p.type == ModContent.ProjectileType<HandRing>() || p.type == ModContent.ProjectileType<DarkGlob>() || p.type == ModContent.ProjectileType<OratorBorder>() || p.type == ModContent.ProjectileType<DarkTide>() || p.type == ModContent.ProjectileType<SelenicIdol>() || p.type == ModContent.ProjectileType<FingerlingGun>() || p.type == ModContent.ProjectileType<PotGlob>())
            {
                Color c = Color.White;
                c.A = 0;
                p.ModProjectile.PostDraw(c);
            }
            else if (p.type == ModContent.ProjectileType<ShadowGrasp>())
                ShadowGrasp.DrawSelf(p);
            else
            {
                Color c = Color.White;
                c.A = 0;
                p.ModProjectile.PreDraw(ref c);
            }
        }
        foreach (NPC n in Main.npc.Where(n => n.active && (
            n.type == ModContent.NPCType<OratorHand>() ||
            n.type == ModContent.NPCType<SealingTablet>()
        )))
        {
            Vector2 drawPosition = n.Center - Main.screenPosition;
            if (n.type == ModContent.NPCType<OratorHand>())
                n.As<OratorHand>().PostDraw(Main.spriteBatch, Main.screenPosition, n.GetAlpha(Color.White));
            else if (n.ai[0] == 2)
                n.As<SealingTablet>().PostDraw(Main.spriteBatch, Main.screenPosition, n.GetAlpha(Color.White));
        }

        EmpyreanMetaballSystem.DrawDissolves(Main.spriteBatch);
    }

    public static float SumofSines(EmpyreanBorderParticle particle, float wavelength, float speed)
    {
        float time = Main.GlobalTimeWrappedHourly;
        float x = particle.SineOffset;
        float a = particle.Interpolant / 2f;
        float w = 2 / wavelength;
        float s = speed * w;
        return (a * 2f) * (float)Math.Sin(x * w + time * (s * 0.25f)) + a * (float)Math.Sin(x * (w * 0.75f) + time * (s * 2f)) + (a * 0.5f) * (float)Math.Sin(x * (w * 1.5f) + time * s);
    }
}

public class EmpyreanMetaballSystem : ModSystem
{
    public override void OnModLoad()
    {
        RenderTargetManager.RenderTargetUpdateLoopEvent += UpdateDissolveTargets;
        On_Main.DrawProjectiles += DrawFrontFlakes;
        On_Main.DoDraw_DrawNPCsOverTiles += DrawBackFlakes;
    }

    private void DrawBackFlakes(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (OpulentFlake flake in OpulentFlakeBackParticles)
            flake.Draw(Main.spriteBatch);

        Main.spriteBatch.End();

        orig(self);
    }

    private void DrawFrontFlakes(On_Main.orig_DrawProjectiles orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (OpulentFlake flake in OpulentFlakeFrontParticles)
            flake.Draw(Main.spriteBatch);

        Main.spriteBatch.End();

        orig(self);
    }

    public static CalamityMod.Graphics.ManagedRenderTarget dissolveTarget = null;

    private void UpdateDissolveTargets()
    {
        if (!ShaderManager.TryGetShader("Windfall.Dissolve", out ManagedShader dissolveShader))
            return;

        dissolveTarget ??= new(true, ManagedRenderTarget.CreateScreenSizedTarget);

        var gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(dissolveTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        dissolveShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.LinearWrap);

        foreach (Projectile p in Main.projectile.Where(p => p.active && p.ModProjectile is IEmpyreanDissolve))
        {
            IEmpyreanDissolve diss = p.ModProjectile as IEmpyreanDissolve;
            
            if (diss.DissolveIntensity == 1)
                continue;

            dissolveShader.TrySetParameter("dissolveIntensity", diss.DissolveIntensity);
            dissolveShader.TrySetParameter("sampleOffset", diss.sampleOffset);
            dissolveShader.Apply();

            diss.DrawOverlay(Main.spriteBatch);
        }

        gd.SetRenderTarget(null);
        Main.spriteBatch.End();
    }

    internal static void DrawDissolves(SpriteBatch sb)
    {
        sb.Draw(dissolveTarget, Main.screenLastPosition - Main.screenPosition, Color.White);
    }
}