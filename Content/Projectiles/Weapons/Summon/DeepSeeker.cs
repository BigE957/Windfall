using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.World;
using MonoMod.Core.Utils;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Players;
using Windfall.Common.Systems;
using Windfall.Content.Buffs.Weapons.Minions;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.Projectiles.Weapons.Summon
{
    public class DeepSeeker : ModProjectile
    {
        public override string Texture => "Windfall/Assets/NPCs/Enemies/DarkSpawn";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 50;
            Projectile.damage = 150;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.manualDirectionChange = true;
            Projectile.scale = 1.25f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }
        internal enum AIState
        {
            Spawning,
            NoTarget,
            Hunting,
            Recoil,
            Dashing,
            Globbing,
            Sacrifice,
        }
        internal AIState CurrentAI
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        private int aiCounter
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }     
        Vector2 toTarget = Vector2.Zero;
        private bool attackBool = false;
        public Player Owner => Main.player[Projectile.owner];
        public NPC Target
        {
            get
            {
                NPC target = null;

                if (Owner.HasMinionAttackTargetNPC)
                    target = CheckNPCTargetValidity(Main.npc[Owner.MinionAttackTargetNPC]);

                if (target != null)
                    return target;

                else
                {
                    for (int npcIndex = 0; npcIndex < Main.npc.Length; npcIndex++)
                    {
                        target = CheckNPCTargetValidity(Main.npc[npcIndex]);
                        if (target != null)
                            return target;
                    }
                }

                return null;
            }
        }
        public static float AggroRange = 1600f;
        public NPC CheckNPCTargetValidity(NPC potentialTarget)
        {
            if (potentialTarget.CanBeChasedBy(this, false))
            {
                float targetDist = Vector2.Distance(potentialTarget.Center, Projectile.Center);

                if ((targetDist < AggroRange) && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, potentialTarget.position, potentialTarget.width, potentialTarget.height))
                {
                    return potentialTarget;
                }
            }

            return null;
        }

        public override void OnSpawn(IEntitySource source)
        {
            CurrentAI = AIState.Spawning;
            Projectile.velocity = Main.rand.NextFloat(0, TwoPi).ToRotationVector2() * Main.rand.Next(10, 15);
            Projectile.rotation = Projectile.velocity.ToRotation() + Pi;

        }
        public override void AI()
        {
            AggroRange = 2000f;
            #region Frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= Main.projFrames[Projectile.type])
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
            #endregion           

            Player player = Main.player[Projectile.owner];
            BuffPlayer buffPlayer = player.Buff();           

            #region Buff
            player.AddBuff(ModContent.BuffType<DeepSeekerBuff>(), 3600);
            if (player.dead)
                buffPlayer.DeepSeeker = false;
            if (buffPlayer.DeepSeeker)
                Projectile.timeLeft = 2;
            Projectile.MinionAntiClump();
            #endregion

            NPC target = Target;
            if (target == null)
                CurrentAI = AIState.NoTarget;
            else if(CurrentAI == AIState.NoTarget)
                CurrentAI = AIState.Hunting;
            switch (CurrentAI)
            {
                case AIState.Spawning:
                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 191 : dustStyle);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                    //dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    if (Projectile.velocity.Length() < 2)
                    {
                        CurrentAI = AIState.NoTarget;
                        aiCounter = 0;
                        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    }
                    break;
                case AIState.NoTarget:
                    Projectile.velocity += (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.5f;
                    if (Projectile.velocity.Length() > 10)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    if (Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2)
                        Projectile.rotation = 0 + (PiOver4 * (Projectile.velocity.Length() / 10));
                    else
                        Projectile.rotation = Pi - (PiOver4 * (Projectile.velocity.Length() / 10));

                    aiCounter = 0;
                    break;
                case AIState.Hunting:

                    #region Movement
                    Vector2 homeInVector = target.Center - Projectile.Center;
                    float targetDist = homeInVector.Length();
                    homeInVector.Normalize();
                    if (targetDist > 250f)
                    {
                        float velocity = 30f;
                        Projectile.velocity = (Projectile.velocity * 40f + homeInVector * velocity) / 41f;
                    }
                    else
                    {
                        if (targetDist < 200f)
                        {
                            float velocity = -30f;
                            Projectile.velocity = (Projectile.velocity * 40f + homeInVector * velocity) / 41f;
                        }
                        else
                            Projectile.velocity *= 0.97f;
                    }
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    if (Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2)
                        Projectile.rotation = 0 + (PiOver4 * (Projectile.velocity.Length() / 10));
                    else
                        Projectile.rotation = Pi - (PiOver4 * (Projectile.velocity.Length() / 10));
                    #endregion

                    #region Attack
                    Vector2 toTarget = (target.Center - Projectile.Center);
                    if (targetDist < 350f)
                    {
                        if (aiCounter % 45 == 0 && aiCounter <= 140)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_OgreSpit, Projectile.Center);
                            Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * -10;
                            Projectile.rotation = Projectile.velocity.ToRotation() + Pi;
                            Projectile Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                            Bolt.hostile = false;
                            Bolt.friendly = true;
                            Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / 8), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                            Bolt.hostile = false;
                            Bolt.friendly = true;
                            Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(-Pi / 8), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                            Bolt.hostile = false;
                            Bolt.friendly = true;
                            CurrentAI = AIState.Recoil;
                        }
                        else if (aiCounter >= 180)
                        {
                            Projectile.rotation = toTarget.ToRotation();
                            if (Main.rand.NextBool() || toTarget.Length() > 600f)
                            {
                                attackBool = Projectile.position.X > target.position.X;
                                Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * -5;
                                CurrentAI = AIState.Dashing;
                            }
                            else
                            {
                                Projectile.velocity = Vector2.Zero;
                                CurrentAI = AIState.Globbing;
                            }
                            aiCounter = 0;
                        }
                        else
                            attackBool = false;
                    }
                    #endregion

                    break;
                case AIState.Dashing:
                    Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), Pi / 10);
                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    if (Projectile.velocity.Length() < 2)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, Projectile.Center);
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * -30;
                        attackBool = true;
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
                        SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
                        Vector2 myAngle = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * Math.Ceiling((double)aiCounter / 5));
                        for (int i = 0; i < 10; i++)
                        {
                            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (myAngle * 40), myAngle.RotatedByRandom(Pi/6) * Main.rand.NextFloat(0f, 15f), 20 * Main.rand.NextFloat(1f, 2f));
                        }
                        Projectile Glob = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, myAngle * 15, ModContent.ProjectileType<DarkGlob>(), Projectile.damage, 0f, -1, 1, 0.5f);
                        Glob.hostile = false;
                        Glob.friendly = true;
                    }
                    Projectile.rotation = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * ((float)aiCounter / 5)).ToRotation();
                    if (aiCounter % 30 == 0)
                    {
                        aiCounter = -30;
                        CurrentAI = AIState.Hunting;
                    }
                    break;
                case AIState.Recoil:
                    if (attackBool)
                    {
                        Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), 0.05f);
                        Projectile.rotation = Projectile.velocity.ToRotation();
                    }
                    else
                        Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                    if (Projectile.velocity.Length() >= 8)
                    {
                        dustStyle = Main.rand.NextBool() ? 66 : 263;
                        dust = Dust.NewDustPerfect(Projectile.Center - Projectile.rotation.ToRotationVector2() * 20, Main.rand.NextBool(3) ? 191 : dustStyle);
                        dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                        //dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                        dust.noGravity = true;
                        dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    }
                    if (Projectile.velocity.Length() < 2)
                    {
                        CurrentAI = AIState.Hunting;
                        attackBool = false;
                        Projectile.velocity = Vector2.Zero;
                    }
                    break;
            }
            aiCounter++;
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type] / 2);
            Vector2 drawPosition = new Vector2(Projectile.Center.X, Projectile.Center.Y) - Main.screenPosition;
            Vector2 origin = TextureAssets.Projectile[Projectile.type].Size() * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (!(Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2))
                spriteEffects = SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}
