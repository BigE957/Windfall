﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Windfall.Common.Graphics;
using Windfall.Content.Buffs.DoT;
using Windfall.Content.NPCs.Bosses.TheOrator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class OratorBorder : ModProjectile
    {
        public ref float counter => ref Projectile.ai[0];
        public ref float trueScale => ref Projectile.ai[1];

        public float Radius = 750f;

        public override void SetDefaults()
        {
            Projectile.height = Projectile.width = 900;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 3f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 5f;
            Projectile.ai[1] = 5f;
            Radius = (750 / 3f) * Projectile.scale;
            const int pCount = 250;
            for (int i = 0; i < 150; i++)
            {
                SpawnBorderParticle(Projectile, 5, 50, TwoPi / 150 * i, false);
            }
            for (int i = 0; i <= pCount; i++)
            {
                SpawnBorderParticle(Projectile, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(80, 160), TwoPi / pCount * i);
                SpawnBorderParticle(Projectile, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(60, 100), TwoPi / pCount * -i - TwoPi / (pCount / 2));
            }
        }
        public override void AI()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width * Projectile.scale / 9.65f, Projectile.width * Projectile.scale / 9.65f);
                SpawnDefaultParticle(spawnPosition, ((Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(2f, 5f)), Main.rand.NextFloat(60f, 100f));
            }

            int oratorIndex = NPC.FindFirstNPC(ModContent.NPCType<TheOrator>());
            if (oratorIndex < 0)
            {
                trueScale += 0.05f;
                return;
            }
            else if (trueScale > 3f)
            {
                trueScale -= 0.05f;
                counter = (3 * Pi / 2) * 20;
            }
            Projectile.scale = (float)(trueScale - (Math.Sin(counter / 20f) + 1f) / 8);
            Radius = (750 / 3f) * Projectile.scale;
            Projectile.timeLeft = 30;           

            // Move towards the target.
            //Projectile.Center = Projectile.Center.MoveTowards(Main.npc[oratorIndex].Center, 1.1f);

            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (!target.WithinRange(Projectile.Center, Radius + 12f))
                target.AddBuff(ModContent.BuffType<Entropy>(), 5);
            counter++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
    }
}