using CalamityMod.NPCs.AstrumDeus;
using Luminance.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.Projectiles.Weapons.Rogue;
public class OratorSpearProj : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "Windfall/Assets/Items/Weapons/Rogue/OratorSpear";

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Projectile.timeLeft = SecondsToFrames(90);
    }

    public override void AI()
    {
        if (Projectile.ai[0] == 0f)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver4;
        }
        //Sticky Behaviour
        Projectile.StickyProjAI(30);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(12);
    public override bool? CanDamage() => Projectile.ai[0] == 1f ? false : base.CanDamage();

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 v = (Projectile.rotation - PiOver4).ToRotationVector2();
        Vector2 lineStart = Projectile.Center - (v * Projectile.width * 0.5f);
        Vector2 lineEnd = Projectile.Center + (v * Projectile.width * 0.5f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, Projectile.height, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 16; i++)
        {
            Vector2 spawnPos = Projectile.Center - ((Projectile.rotation - PiOver4).ToRotationVector2() * Main.rand.NextFloat(-64f, 64f));
            if (!Main.tile[spawnPos.ToTileCoordinates()].IsTileSolid())
                EmpyreanMetaball.SpawnDefaultParticle(spawnPos, Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextFloat(10f, 20f));
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.Calamity().stealthStrike)
        {
            int damage = 370;
            foreach(Projectile spear in Main.projectile.Where(p => p.active && p.ai[0] == 1f && (p.type == ModContent.ProjectileType<OratorSpearProj>())))
            {
                damage += (int)(spear.localAI[0] / 60 * 25);
                spear.Kill();
                spear.active = false;
            }
            NPC.HitInfo stealthHit = hit;
            stealthHit.Damage = damage;
            target.StrikeNPC(stealthHit);

            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
            ScreenShakeSystem.StartShake(5f);
            for (int i = 0; i <= 50; i++)
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
            CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
            GeneralParticleHandler.SpawnParticle(pulse);
            CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
            GeneralParticleHandler.SpawnParticle(explosion);
            Projectile.active = false;
        }
        else
        {
            if (target.type == NPCID.CultistBoss)
                target.buffImmune[BuffID.Frostburn2] = false;
            target.AddBuff(BuffID.Frostburn2, 1800);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Frostburn2, 1800);
}
