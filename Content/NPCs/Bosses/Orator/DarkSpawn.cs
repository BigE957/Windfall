using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.World;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.NPCs.Bosses.TheOrator
{
    public class DarkSpawn : ModNPC
    {
        public override string Texture => "CalamityMod/NPCs/Astral/Nova";
        private static Texture2D glowmask;
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[NPC.type] = 8;
            if (!Main.dedServ)
                glowmask = ModContent.Request<Texture2D>("CalamityMod/NPCs/Astral/NovaGlow", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void SetDefaults()
        {
            NPC.width = 78;
            NPC.height = 50;
            NPC.damage = 20;
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit49;
            NPC.DeathSound = SoundID.NPCDeath51;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
        }
        private float Acceleration = CalamityWorld.death ? 1f : CalamityWorld.revenge ? 0.9f : Main.expertMode ? 0.75f : 0.5f;
        private int MaxSpeed = CalamityWorld.revenge ? 8 : 5;
        enum AIState
        {
            Chasing,
            Shooting,
        }
        private AIState CurrentAI
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        private int aiCounter
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }     

        Vector2 toTarget = Vector2.Zero;
        private bool hasDashed = false;
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            if (!hasDashed)
            {
                if (aiCounter == 0)
                    NPC.ai[3] = Main.rand.Next(180, 240);
                if (aiCounter < NPC.ai[3] && NPC.life == NPC.lifeMax)
                {
                    toTarget = target.Center - NPC.Center;
                    NPC.velocity.X = (float)(20 * Math.Cos((double)aiCounter / 20));
                    NPC.velocity.Y = (float)(20 * Math.Sin((double)aiCounter / 20));
                    NPC.position += target.velocity;
                    NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                    NPC.spriteDirection = NPC.direction * -1;
                }
                else
                {
                    if (aiCounter < NPC.ai[3])
                        aiCounter = (int)NPC.ai[3];
                    if (aiCounter == NPC.ai[3])
                        NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -5;
                    NPC.velocity += toTarget.SafeNormalize(Vector2.Zero) * 0.5f;
                    if ((target.Center - NPC.Center).Length() > 600)
                        hasDashed = true;
                }
            }
            else
            {
                //MaxSpeed = 12;
                //Acceleration = 1f;
                NPC boss = null;
                if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                    boss = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                if (boss == null || (boss.ai[0] != 2 && boss.ai[0] != 0))
                {
                    toTarget = target.Center - NPC.Center;
                    NPC.velocity -= toTarget.SafeNormalize(Vector2.UnitX);
                    NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                    NPC.spriteDirection = NPC.direction * -1;
                    if (toTarget.Length() > 800)
                    {
                        NPC.active = false;
                        if (boss != null && boss.ModNPC is TheOrator orator)
                            orator.noSpawnsEscape = false;
                    }
                }
                else
                {
                    if (Main.rand.NextBool(50) && CurrentAI == AIState.Chasing && aiCounter >= 30 && (target.Center - NPC.Center).Length() < 400)
                    {
                        CurrentAI = AIState.Shooting;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * -5;
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0);
                    }
                    switch (CurrentAI)
                    {
                        case AIState.Chasing:
                            toTarget = target.Center - NPC.Center;
                            if((target.Center - NPC.Center).Length() > 300)
                                NPC.velocity += (toTarget.SafeNormalize(Vector2.UnitX) * Acceleration);
                            else
                                NPC.velocity -= (toTarget.SafeNormalize(Vector2.UnitX) * Acceleration);

                            if (NPC.velocity.Length() > MaxSpeed)
                                NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.UnitX);
                            break;
                        case AIState.Shooting:
                            NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitX) / -2;
                            if (NPC.velocity.Length() < 2)
                            {
                                CurrentAI = AIState.Chasing;
                                aiCounter = 0;
                            }
                            break;
                    }
                    NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                    NPC.spriteDirection = NPC.direction * -1;
                }
            }
            aiCounter++;
            Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }

            //DO DUST
            Dust d = CalamityGlobalNPC.SpawnDustOnNPC(NPC, 114, frameHeight, ModContent.DustType<AstralOrange>(), new Rectangle(78, 34, 36, 18), Vector2.Zero, 0.45f, true);
            if (d != null)
                d.customData = 0.04f;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return;

            Vector2 drawPosition = NPC.Center - screenPos - new Vector2(0, NPC.scale * 4f);
            spriteBatch.Draw(glowmask, drawPosition, NPC.frame, Color.White * 0.75f, NPC.rotation, new Vector2(57f, 37f), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        public override bool CheckActive() => false;
    }
}
