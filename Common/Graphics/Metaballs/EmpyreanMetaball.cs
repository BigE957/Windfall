﻿using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Metaballs;
using Luminance.Assets;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.Weapons.Summon;

namespace Windfall.Common.Graphics.Metaballs
{
    public class EmpyreanMetaball : Metaball
    {
        public class EmpyreanParticle(Vector2 center, Vector2 velocity, float size)
        {
            public float Size = size;

            public Vector2 Velocity = velocity;

            public Vector2 Center = center;

            public void Update()
            {
                Size *= 0.94f;
                Center += Velocity;
                Velocity *= 0.96f;
            }
        }
        public class EmpyreanBorderParticle(Projectile projectile, Vector2 offset, float sineOffset, float interpolant, float size, float rotation, bool spin)
        {
            public float Size = size;

            public Projectile Projectile = projectile;

            public Vector2 Offset = offset;

            public float SineOffset = sineOffset;

            public float Interpolant = interpolant;

            public float Rotation = rotation;

            public Vector2 Center;

            public bool spin = spin;
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

        // Check if there are any extraneous particles or if the Gruesome Eminence projectile is present when deciding if this particle should be drawn.
        public override bool AnythingToDraw =>
            EmpyreanParticles.Count != 0 ||
            EmpyreanStickyParticles.Count != 0 ||
            AnyProjectiles(ModContent.ProjectileType<DarkGlob>()) ||
            AnyProjectiles(ModContent.ProjectileType<DarkMonster>()) ||
            AnyProjectiles(ModContent.ProjectileType<EmpyreanThorn>()) ||
            AnyProjectiles(ModContent.ProjectileType<DarkCoalescence>()) ||
            AnyProjectiles(ModContent.ProjectileType<OratorBorder>()) ||
            AnyProjectiles(ModContent.ProjectileType<DarkTide>()) ||
            AnyProjectiles(ModContent.ProjectileType<ShadowHand_Minion>()) ||
            NPC.AnyNPCs(ModContent.NPCType<ShadowHand>()) ||
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

        public override Color EdgeColor => new(117, 255, 159);

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // Load layer assets.
            layerAssets = [];

            layerAssets.Add(ModContent.Request<Texture2D>("Windfall/Assets/Graphics/TurbulentNoise"));
            layerAssets.Add(ModContent.Request<Texture2D>("Windfall/Assets/Graphics/AlphaWavyBlotchNoise1"));
            layerAssets.Add(ModContent.Request<Texture2D>("Windfall/Assets/Graphics/AlphaWavyBlotchNoise2"));
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
        public override void Update()
        {
            // Update all particle instances.
            // Once sufficiently small, they vanish.
            foreach (EmpyreanParticle particle in EmpyreanParticles)
            {
                particle.Velocity *= 0.99f;
                particle.Size *= 0.9f;
                particle.Center += particle.Velocity;
            }
            EmpyreanParticles.RemoveAll(p => p.Size <= 2.5f);
            
            foreach (EmpyreanBorderParticle particle in EmpyreanStickyParticles)
            {
                Projectile myProj = particle.Projectile;
                
                if(particle.spin)
                    if (particle.Rotation > 0)
                        particle.Rotation += 0.0175f / Math.Abs(particle.Interpolant / 4);
                    else
                        particle.Rotation -= 0.0175f / Math.Abs(particle.Interpolant / 4);
                
                particle.Center = myProj.Center + particle.Offset + (new Vector2(myProj.width / 2 * (myProj.scale / 5f) * 1.05f, 0).RotatedBy(particle.Rotation + myProj.rotation));
                particle.Center += (myProj.Center + particle.Offset - particle.Center).SafeNormalize(Vector2.Zero) * SumofSines(particle, 1.5f, 2f);
                if (!particle.spin)
                    particle.Center -= myProj.velocity / 2;  
            }
            if(EmpyreanStickyParticles.Count != 0)
                EmpyreanStickyParticles.RemoveAll(p => p.Projectile == null || !p.Projectile.active);
        }

        public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
        {
            switch (layerIndex)
            {
                // Background.
                case 0:
                    return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.03f;

                // Gaseous skulls.
                case 1:
                    Vector2 offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.041f) * 2f;
                    offset = offset.RotatedBy((float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.08f) * 0.97f);
                    return offset;

                // Spooky faces 1.
                case 2:
                    offset = (Main.GlobalTimeWrappedHourly * 2.02f).ToRotationVector2() * 0.036f;
                    offset.Y += (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.161f) * 0.5f + 0.5f;
                    return offset;

                // Spooky faces 2.
                case 3:
                    offset = Vector2.UnitX * Main.GlobalTimeWrappedHourly * -0.04f + (Main.GlobalTimeWrappedHourly * 1.89f).ToRotationVector2() * 0.03f;
                    offset.Y += PerlinNoise2D(Main.GlobalTimeWrappedHourly * 0.187f, Main.GlobalTimeWrappedHourly * 0.193f, 2, 466920161) * 0.025f;
                    return offset;

                // Spooky faces 3.
                case 4:
                    offset = Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.037f + (Main.GlobalTimeWrappedHourly * 1.77f).ToRotationVector2() * 0.04725f;
                    offset.Y += PerlinNoise2D(Main.GlobalTimeWrappedHourly * 0.187f, Main.GlobalTimeWrappedHourly * 0.193f, 2, 577215664) * 0.05f;
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
                p.type == ModContent.ProjectileType<DarkMonster>() || 
                p.type == ModContent.ProjectileType<EmpyreanThorn>() || 
                p.type == ModContent.ProjectileType<DarkCoalescence>() || 
                p.type == ModContent.ProjectileType<OratorBorder>() || 
                p.type == ModContent.ProjectileType<DarkTide>() ||
                p.type == ModContent.ProjectileType<ShadowHand_Minion>()
            )))
            {
                if(p.type == ModContent.ProjectileType<ShadowHand_Minion>())
                {
                    Vector2 drawPosition = p.Center - Main.screenPosition;
                    p.As<ShadowHand_Minion>().DrawSelf(drawPosition, p.GetAlpha(Color.White), p.rotation);
                }
                else
                {
                    Color c = Color.White;
                    c.A = 0;
                    p.ModProjectile.PreDraw(ref c);
                }
            }
            foreach (NPC n in Main.npc.Where(n => n.active && (
                n.type == ModContent.NPCType<ShadowHand>() || 
                n.type == ModContent.NPCType<OratorHand>() ||
                n.type == ModContent.NPCType<SealingTablet>()
            )))
            {
                Vector2 drawPosition = n.Center - Main.screenPosition;
                if (n.type == ModContent.NPCType<ShadowHand>())
                    n.As<ShadowHand>().DrawSelf(drawPosition, n.GetAlpha(Color.White), n.rotation);
                else if (n.type == ModContent.NPCType<OratorHand>())
                    n.As<OratorHand>().PostDraw(Main.spriteBatch, Main.screenPosition, n.GetAlpha(Color.White));
                else if (n.ai[0] == 2)
                    n.As<SealingTablet>().PostDraw(Main.spriteBatch, Main.screenPosition, n.GetAlpha(Color.White));
            }
        }
        private static float SumofSines(EmpyreanBorderParticle particle, float wavelength, float speed)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float x = particle.SineOffset;
            float a = particle.Interpolant / 2f;
            float w = 2 / wavelength;
            float s = speed * w;
            return (a * 2f) * (float)Math.Sin(x * w + time * (s * 0.25f)) + a * (float)Math.Sin(x * (w * 0.75f) + time * (s * 2f)) + (a * 0.5f) * (float)Math.Sin(x * (w * 1.5f) + time * s);
        }
    }
}
