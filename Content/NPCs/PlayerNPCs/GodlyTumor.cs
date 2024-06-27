using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.NormalNPCs;
using Windfall.Content.Buffs.StatBuffs;

namespace Windfall.Content.NPCs.PlayerNPCs
{
    public class GodlyTumor : ModNPC
    {
        public override string Texture => "CalamityMod/NPCs/HiveMind/HiveTumor";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.CantTakeLunchMoney[Type] = false;
            NPCID.Sets.ImmuneToAllBuffs[NPC.type] = true;
            this.HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.npcSlots = 0f;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 50;
            NPC.height = 30;
            NPC.defense = 0;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0f;
            NPC.netAlways = true;
            NPC.chaseable = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0;
            NPC.SpawnedFromStatue = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().ProvidesProximityRage = false;
            NPC.Calamity().DoesNotDisappearInBossRush = true;
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = 500;
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 oldPos = NPC.position;
            NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

            float altY = 0;
            for (int i = 0; i < 2; i++)
            {
                altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
                if (altY < NPC.position.Y)
                    NPC.position.Y = altY;
            }
            SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton, NPC.Center);

            for (int k = 0; k < 50; k++)
            {
                Dust.NewDustPerfect(NPC.Bottom + new Vector2(Main.rand.NextFloat(-24f, 24f), 0f), DustID.Demonite, new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2.5f, 0f)));
            }

            Particle pulse = new StaticPulseRing(NPC.Center, Vector2.Zero, Color.Purple, new Vector2(1f, 1f), 0f, 0f, 0.2925f, 11);
            GeneralParticleHandler.SpawnParticle(pulse);
        }
        private int WitherFactor = 1;
        private int aiCounter = 0;
        private Particle Aura = null;
        public Player Owner = null;
        public override void AI()
        {
            #region Area Effects
            foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly && !n.SpawnedFromStatue && n.type != ModContent.NPCType<SuperDummyNPC>() && !n.dontTakeDamage && (n.Center - NPC.Center).LengthSquared() <= 90000)) //300
            {
                if (npc.type != ModContent.NPCType<GodlyTumor>())
                    npc.AddBuff(ModContent.BuffType<BrainRot>(), 120);
            }           
            if (Main.player.Where(p => p.active && !p.dead && (p.Center - NPC.Center).LengthSquared() <= 90000).Any())
                WitherFactor++;
            foreach (Player player in Main.player.Where(p => p.active && !p.dead && (p.Center - NPC.Center).LengthSquared() <= 90000)) //300
            {
                player.AddBuff(ModContent.BuffType<WretchedHarvest>(), 120);
            }
            if (WitherFactor > 480 || aiCounter >= 9000)
                NPC.StrikeInstantKill();
            #endregion
            #region Visual Effects
            if (aiCounter == 10)
            {
                Aura = new StaticPulseRing(NPC.Center, Vector2.Zero, Color.Purple, new Vector2(1f, 1f), 0f, 0.2925f, 0.2925f, 9000);
                GeneralParticleHandler.SpawnParticle(Aura);
            }
            else if (aiCounter >= 10)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2CircularEdge(301.6f, 301.6f), DustID.Shadowflame);
                    dust.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    dust.noGravity = true;
                }

                for (int i = 0; i < 1; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(292.5f, 292.5f), DustID.VilePowder);
                    dust.scale = Main.rand.NextFloat(0.3f, 0.9f);
                    dust.noGravity = true;
                }
            } 
            if(WitherFactor % 120 == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2CircularEdge(20f, 20f), DustID.Shadowflame);
                    dust.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    dust.noGravity = true;
                }
            }
            aiCounter++;
            #endregion
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Demonite, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                if (WitherFactor < 480 && 30 * (WitherFactor / 480) > 15)
                    Owner.Godly().Ambrosia += 30 * (WitherFactor / 480);
                else
                    Owner.Godly().Ambrosia += 15;
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Demonite, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!hit.InstantKill)
                    foreach (Player player in Main.player.Where(p => p.active && !p.dead && (p.Center - NPC.Center).LengthSquared() <= 90000)) //300
                    {
                        player.AddBuff(ModContent.BuffType<WretchedHarvest>(), 240);
                    }
                GeneralParticleHandler.RemoveParticle(Aura);
                Aura = new StaticPulseRing(NPC.Center, Vector2.Zero, Color.Purple, new Vector2(1f, 1f), 0f, 0.2925f, 0f, 10);
                GeneralParticleHandler.SpawnParticle(Aura);
            }
        }
    }
}
