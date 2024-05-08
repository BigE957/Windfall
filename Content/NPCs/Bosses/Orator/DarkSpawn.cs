using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.World;
using Windfall.Common.Systems;
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
            NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 300 : CalamityWorld.death ? 220 : CalamityWorld.revenge ? 180 : Main.expertMode ? 120 : 80);
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lifeMax = 1200;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit49;
            NPC.DeathSound = SoundID.NPCDeath51;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
        }
        internal enum AIState
        {
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
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            NPC Orator = null;
            if(NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];

            if (CurrentAI == AIState.OnBoss)
            {
                NPC.dontTakeDamage = true;
                NPC.damage = 0;
            }
            else
            {
                NPC.dontTakeDamage = false;
                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 300 : CalamityWorld.death ? 220 : CalamityWorld.revenge ? 180 : Main.expertMode ? 120 : 80);
            }
            #region Despawning
            if (Orator == null)
            {
                toTarget = target.Center - NPC.Center;
                NPC.velocity -= toTarget.SafeNormalize(Vector2.UnitX);
                NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                NPC.spriteDirection = NPC.direction * -1;
                if (toTarget.Length() > 800)
                    NPC.active = false;
                return;
            }
            else if (Orator.ai[0] != 2 && Orator.ai[0] != 0)
            {
                if(CurrentAI != AIState.Sacrifice)
                    aiCounter = 0;
                CurrentAI = AIState.Sacrifice;  
            }
            #endregion
            /*
            if(CurrentAI == AIState.OnBoss)
                CurrentAI++;
            */
            switch (CurrentAI)
            {
                case AIState.OnBoss:
                    if (NPC.Center.Y < Orator.Center.Y)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < Orator.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    NPC.rotation = NPC.velocity.ToRotation() + Pi;
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
                    }
                    else
                    {
                        if (targetDist < 250f)
                        {
                            float velocity = -12f;
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                        }
                        else
                            NPC.velocity *= 0.97f;
                    }
                    NPC.rotation = NPC.velocity.ToRotation() + Pi;
                    #endregion

                    #region Attack
                    Vector2 toTarget = (target.Center - NPC.Center);
                    if (Main.rand.NextBool(60) || toTarget.Length() > 600f)
                    {                      
                        NPC.rotation = toTarget.ToRotation() + Pi;
                        if (Main.rand.NextBool(5) || toTarget.Length() > 600f)
                        {
                            attackBool = NPC.position.X > target.position.X;
                            NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -5;
                            CurrentAI = AIState.Dashing;
                        }
                        else
                        {
                            if (Main.rand.NextBool(3))
                            {
                                NPC.velocity = Vector2.Zero;
                                CurrentAI = AIState.Globbing;
                            }
                            else
                            {
                                NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -10;
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
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * -30;
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
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * Math.Ceiling((double)aiCounter / 5)) * 15, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);                   
                    NPC.rotation = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * ((float)aiCounter / 5)).ToRotation() + Pi;
                    if (aiCounter % 30 == 0)
                        CurrentAI = AIState.Hunting;
                    break;
                case AIState.Recoil:
                    NPC.velocity += NPC.velocity.SafeNormalize(Vector2.UnitX) / -2;
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
                    NPC.rotation = toTarget.ToRotation() + Pi;
                    NPC.spriteDirection = NPC.direction * -1;
                    #endregion

                    #region Healing
                    if (NPC.Hitbox.Intersects(Orator.Hitbox))
                    {
                        Orator.life += Orator.lifeMax / 100;
                        CombatText.NewText(NPC.Hitbox, Color.LimeGreen, Orator.lifeMax / 100);
                        if (Orator.ModNPC is TheOrator orator)
                            TheOrator.noSpawnsEscape = false;
                        NPC.active = false;
                    }
                    #endregion

                    break;
            }
            aiCounter++; 
            NPC.spriteDirection = NPC.direction * -1;
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
