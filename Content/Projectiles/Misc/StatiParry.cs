﻿using CalamityMod.Dusts;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using Windfall.Content.Buffs.Cooldowns;
using Windfall.Content.Buffs.Weapons;
using Windfall.Common.Systems;

namespace Windfall.Content.Projectiles.Misc
{
    public class StatiParry : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/Summon/SlimePuppet";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 6000;
            Projectile.tileCollide = true;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            Player owner = Main.player[Projectile.owner];
            if(owner.Calamity().cooldowns.ContainsKey(ParryWeapon.ID))
                owner.Calamity().cooldowns[ParryWeapon.ID].timeLeft = 0;
            owner.AddBuff(ModContent.BuffType<PerfectFlow>(), 5 * 60);
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ParryIntro"), 0);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 75; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.PurpleCosmilite, speed * 5, Scale: 1.5f);
                d.noGravity = true;
            }
        }
    }
}