﻿using CalamityMod.Graphics.Metaballs;
using CalamityMod.Projectiles.Magic;
using Terraria.Graphics.Renderers;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Common.Graphics.Metaballs
{
    public class EmpyreanMetaball : Metaball
    {
        public class EmpyreanParticle
        {
            public float Size;

            public Vector2 Velocity;

            public Vector2 Center;

            public EmpyreanParticle(Vector2 center, Vector2 velocity, float size)
            {
                Center = center;
                Velocity = velocity;
                Size = size;
            }

            public void Update()
            {
                Size *= 0.94f;
                Center += Velocity;
                Velocity *= 0.96f;
            }
        }

        private static List<Asset<Texture2D>> layerAssets;

        public static List<EmpyreanParticle> EmpyreanParticles
        {
            get;
            private set;
        } = new();

        // Check if there are any extraneous particles or if the Gruesome Eminence projectile is present when deciding if this particle should be drawn.
        public override bool AnythingToDraw => EmpyreanParticles.Any() || AnyProjectiles(ModContent.ProjectileType<DarkGlob>()) || AnyProjectiles(ModContent.ProjectileType<DarkMonster>());

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
            layerAssets = new();

            for (int i = 1; i <= 3; i++)
                layerAssets.Add(ModContent.Request<Texture2D>($"Windfall/Assets/Graphics/Metaballs/Empyrean_Metaball/Empyrean_Distortion_Layer{i}", AssetRequestMode.ImmediateLoad));
        }

        public override void ClearInstances() => EmpyreanParticles.Clear();

        public static void SpawnParticle(Vector2 position, Vector2 velocity, float size) =>
            EmpyreanParticles.Add(new(position, velocity, size));

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
            foreach (Projectile p in Main.projectile.Where(p => p.active))
            {
                if (p.type == ModContent.ProjectileType<DarkGlob>() || p.type == ModContent.ProjectileType<DarkMonster>())
                {
                    Color c = Color.White;
                    c.A = 0;
                    p.ModProjectile.PreDraw(ref c);
                }
            }

        }
    }
}