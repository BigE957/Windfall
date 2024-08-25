using CalamityMod.World;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Common.Utils;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.NPCs.Bosses.Orator
{
    public class ShadowHand : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/Enemies/ShadowHand";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(ShadowHand)}")),
        });
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 64;
            NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 500 : CalamityWorld.death ? 420 : CalamityWorld.revenge ? 350 : Main.expertMode ? 240 : 180);
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lifeMax = 1200;
            NPC.knockBackResist = 0f;
            NPC.scale = 1.25f;
            NPC.HitSound = SoundID.DD2_LightningBugHurt with { Volume = 0.5f };
            NPC.DeathSound = SoundID.DD2_LightningBugDeath;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
            NPC.Calamity().canBreakPlayerDefense = true;
        }
        internal enum AIState
        {
            Spawning,
            OnBoss,
            Hunting,
            Recoil,
            Dashing,
            Globbing,
            Sacrifice,
        }
        internal AIState CurrentAI
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
        bool attackBool = false;

        public override void OnSpawn(IEntitySource source)
        {
            CurrentAI = AIState.Spawning;
            NPC.velocity = Main.rand.NextFloat(0, TwoPi).ToRotationVector2() * Main.rand.Next(10, 15);
            NPC.rotation = NPC.velocity.ToRotation();
            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(7f, 7f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
        }
        bool movingBackward = false;
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            NPC Orator = null;
            if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
            if (CurrentAI <= AIState.OnBoss)
            {
                NPC.dontTakeDamage = true;
                NPC.damage = 0;
            }
            else
            {
                NPC.dontTakeDamage = false;
                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 500 : CalamityWorld.death ? 420 : CalamityWorld.revenge ? 350 : Main.expertMode ? 240 : 180);
            }

            #region Despawning
            if (Orator == null)
            {
                toTarget = target.Center - NPC.Center;
                NPC.velocity -= toTarget.SafeNormalize(Vector2.UnitX);
                //NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                //NPC.spriteDirection = NPC.direction * -1;
                if (toTarget.Length() > 800)
                    NPC.active = false;
                return;
            }
            else if (Orator.ai[0] != 2 && Orator.ai[0] != 0)
            {
                if (CurrentAI != AIState.Sacrifice)
                    aiCounter = 0;
                CurrentAI = AIState.Sacrifice;
            }
            #endregion

            //testing code :P
            //if (CurrentAI == AIState.OnBoss || CurrentAI == AIState.Spawning)
            //   CurrentAI = AIState.Hunting;

            switch (CurrentAI)
            {
                case AIState.Spawning:
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * Main.rand.NextFloat(-16, 16) + new Vector2(-54, 0).RotatedBy(NPC.rotation), Main.rand.NextBool(3) ? 191 : dustStyle);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    if (NPC.velocity.Length() < 2)
                    {
                        CurrentAI = AIState.OnBoss;
                        aiCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    }
                    break;
                case AIState.OnBoss:
                    NPC.velocity += (Orator.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 1.5f;
                    if (NPC.velocity.Length() > 10)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 10;
                    NPC.rotation = (Orator.Center - NPC.Center).ToRotation();
                    if (NPC.rotation + Pi > Pi / 2 && NPC.rotation + Pi < 3 * Pi / 2)
                        NPC.rotation = 0 + PiOver4 * (NPC.velocity.Length() / 10);
                    else
                        NPC.rotation = Pi - PiOver4 * (NPC.velocity.Length() / 10);
                    NPC.NPCAntiClump();
                    break;
                case AIState.Hunting:

                    #region Movement
                    Vector2 homeInVector = target.Center - NPC.Center;
                    float targetDist = homeInVector.Length();
                    homeInVector.Normalize();
                    if (targetDist > 300f)
                    {
                        float velocity = 12f;
                        NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                        movingBackward = NPC.velocity.LengthSquared() < NPC.oldVelocity.LengthSquared();
                    }
                    else
                    {
                        if (targetDist < 250f)
                        {
                            float velocity = -12f;
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                            movingBackward = NPC.velocity.LengthSquared() > NPC.oldVelocity.LengthSquared();
                        }
                        else
                        {
                            NPC.velocity *= 0.9f;
                        }
                    }
                    NPC.rotation = (target.Center - NPC.Center).ToRotation();
                    if (NPC.rotation + Pi > Pi / 2 && NPC.rotation + Pi < 3 * Pi / 2)
                        NPC.rotation = 0 + PiOver4 * (NPC.velocity.Length() / (10 * (movingBackward ? -1 : 1)));
                    else
                        NPC.rotation = Pi - PiOver4 * (NPC.velocity.Length() / (10 * (movingBackward ? -1 : 1)));
                    #endregion

                    #region Attack
                    Vector2 toTarget = target.Center - NPC.Center;
                    attackBool = false;
                    if (Main.rand.NextBool(60) || toTarget.Length() > 600f)
                    {
                        NPC.rotation = toTarget.ToRotation();
                        if (Main.rand.NextBool(5) || toTarget.Length() > 600f)
                        {
                            attackBool = true;
                            NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -5;
                            CurrentAI = AIState.Dashing;
                        }
                        else
                        {
                            if (Main.rand.NextBool(3))
                            {
                                attackBool = NPC.position.X > target.position.X;
                                NPC.velocity = Vector2.Zero;
                                CurrentAI = AIState.Globbing;
                            }
                            else
                            {
                                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -10;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    if (Main.rand.NextBool())
                                    {
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                    }
                                    else
                                    {
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / 8), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / -8), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                    }
                                CurrentAI = AIState.Recoil;
                            }
                        }
                        aiCounter = 0;
                    }
                    #endregion

                    break;
                case AIState.Dashing:
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    if (NPC.velocity.Length() < 2)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, NPC.Center);
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * -36;
                        CurrentAI = AIState.Recoil;
                    }
                    break;
                case AIState.Globbing:
                    Vector2 baseAngle;
                    float rotation;
                    if (attackBool)
                    {
                        baseAngle = 0f.ToRotationVector2();
                        rotation = -Pi / 8;
                    }
                    else
                    {
                        baseAngle = Pi.ToRotationVector2();
                        rotation = Pi / 8;
                    }
                    if (aiCounter % 5 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, NPC.Center);
                        Vector2 myAngle = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * Math.Ceiling((double)aiCounter / 5));
                        for (int i = 0; i < 10; i++)
                            EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + myAngle * 40, myAngle.RotatedByRandom(Pi / 6) * Main.rand.NextFloat(0f, 15f), 20 * Main.rand.NextFloat(1f, 2f));
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, myAngle * 15, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                    }
                    NPC.rotation = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * ((float)aiCounter / 5)).ToRotation();
                    if (aiCounter % 30 == 0)
                        CurrentAI = AIState.Hunting;
                    break;
                case AIState.Recoil:
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    if (attackBool)
                    {
                        NPC.velocity = NPC.velocity.RotateTowards((target.Center - NPC.Center).ToRotation(), 0.02f);
                        NPC.rotation = NPC.velocity.ToRotation();
                        dustStyle = Main.rand.NextBool() ? 66 : 263;
                        dust = Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * Main.rand.NextFloat(-16, 16) + new Vector2(-54, 0).RotatedBy(NPC.rotation), Main.rand.NextBool(3) ? 191 : dustStyle);
                        dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                        dust.noGravity = true;
                        dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    }
                    if (NPC.velocity.Length() < 2)
                    {
                        CurrentAI = AIState.Hunting;
                        aiCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    }
                    break;
                case AIState.Sacrifice:

                    #region Movement
                    toTarget = Orator.Center - NPC.Center;
                    NPC.velocity = toTarget.SafeNormalize(Vector2.UnitY) * aiCounter / 5;
                    if (NPC.velocity.Length() > 20)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 20;
                    NPC.rotation = toTarget.ToRotation();
                    #endregion

                    #region Healing
                    if (NPC.Hitbox.Intersects(Orator.Hitbox))
                    {
                        Orator.life += Orator.lifeMax / 100;
                        if (Orator.life > Orator.lifeMax)
                            Orator.life = Orator.lifeMax;
                        CombatText.NewText(NPC.Hitbox, Color.LimeGreen, Orator.lifeMax / 100);
                        if (Orator.ModNPC is TheOrator orator)
                            TheOrator.noSpawnsEscape = false;
                        SoundEngine.PlaySound(SoundID.AbigailUpgrade, NPC.Center);
                        for (int i = 0; i <= 20; i++)
                            EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(6f, 6f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                        NPC.active = false;
                    }
                    #endregion

                    break;
            }
            aiCounter++;

            EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + new Vector2(-32, 0).RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(NPC.rotation) * -8, NPC.scale * 34);
            if (Main.rand.NextBool(3))
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + Main.rand.NextVector2Circular(2, 2) + new Vector2(-32, 0).RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(NPC.rotation + Main.rand.NextFloat(-0.5f, 0.5f)) * -Main.rand.NextFloat(6f, 8f), NPC.scale * Main.rand.NextFloat(30f, 40f));
            Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
        public override void OnKill()
        {
            for (int i = 0; i <= 25; i++)
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(8f, 8f), 40 * Main.rand.NextFloat(1.75f, 2.5f));
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
        }
        public override bool CheckActive() => false;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
        public void DrawSelf(Vector2 drawPosition, Color color, float rotation)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (!(NPC.rotation + Pi > Pi / 2 && NPC.rotation + Pi < 3 * Pi / 2) && CurrentAI != AIState.Globbing)
                spriteEffects = SpriteEffects.FlipVertically;
            if (attackBool && CurrentAI == AIState.Globbing)
                spriteEffects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, color, rotation, NPC.frame.Size() * 0.5f, NPC.scale / 2f, spriteEffects, 0);
        }
    }
}
