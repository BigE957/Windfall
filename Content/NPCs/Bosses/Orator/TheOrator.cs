using CalamityMod.Items.Weapons.Summon;
using CalamityMod.World;
using Terraria.Graphics.Shaders;
using Terraria;
using Windfall.Common.Systems;
using Windfall.Content.Projectiles.Boss.Orator;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework.Graphics;

namespace Windfall.Content.NPCs.Bosses.TheOrator
{
    public class TheOrator : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator";
        public static readonly SoundStyle Dash = new("CalamityMod/Sounds/Item/DeadSunShot") { PitchVariance = 0.35f, Volume = 0.5f };
        public static readonly SoundStyle DashWarn = new("CalamityMod/Sounds/Item/DeadSunRicochet") { Volume = 0.5f };
        public static readonly SoundStyle HurtSound = new("CalamityMod/Sounds/NPCHit/ShieldHit", 3);
        public override void SetDefaults()
        {
            NPC.friendly = false;
            NPC.boss = true;
            NPC.width = NPC.height = 44;
            NPC.Size = new Vector2(150, 150);
            NPC.aiStyle = -1;
            //Values gotten from Lunatic Cultist. Subject to change.
            NPC.DR_NERD(0.10f);
            NPC.LifeMaxNERB(Main.masterMode ? 153000 : Main.expertMode ? 120000 : 80000, 144000);
            NPC.npcSlots = 5f;
            NPC.defense = 50;
            NPC.HitSound = HurtSound;
            NPC.DeathSound = SoundID.NPCDeath59;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
        }
        private float forcefieldOpacity = 1f;
        private int hitTimer = 0;
        private float forcefieldScale = 1;

        private static readonly int MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
        internal static readonly int GlobDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 260 : CalamityWorld.death ? 220 : CalamityWorld.revenge ? 180 : Main.expertMode ? 140 : 100);
        internal static readonly int BoltDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 264 : CalamityWorld.death ? 188 : CalamityWorld.revenge ? 176 : Main.expertMode ? 152 : 90);
        //Will eventually be used for a special drop if the player kills every Dark Spawn the Orator summons and doesn't let any escape.
        public bool noSpawnsEscape = true;
        public override void OnSpawn(IEntitySource source)
        {
            noSpawnsEscape = true;
        }
        private enum States
        {
            Spawning,
            DarkMonster,
            DarkSpawn,
            DarkOrbit,
            DarkSlice,
            DarkStorm,
        }
        private States AIState
        {
            get => (States)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        int aiCounter = 0;
        int attackCounter = 0;
        Vector2 VectorToTarget = Vector2.Zero;
        public override bool PreAI()
        {

            return true;
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            if (target.active == false || target.dead)
                NPC.active = false;
            if (NPC.Center.X < target.Center.X)
                NPC.direction = 1;
            else
                NPC.direction = -1;
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));

            switch (AIState)
            {
                case States.Spawning:
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    if (aiCounter == 240)
                    {
                        aiCounter = -30;
                        SoundEngine.PlaySound(DashWarn);
                        attackCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.DarkMonster:
                    if (aiCounter == 1)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero * 20, ModContent.ProjectileType<DarkMonster>(), MonsterDamage, 0f);
                    VectorToTarget = target.Center - NPC.Center;
                    if (target.velocity.Length() < 1)
                    {
                        if (VectorToTarget.Length() < 300)
                            NPC.velocity -= VectorToTarget.SafeNormalize(Vector2.Zero);
                        else
                            NPC.velocity += VectorToTarget.SafeNormalize(Vector2.Zero);
                        if (NPC.velocity.Length() > 5)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 5;
                    }
                    else
                    {
                        NPC.velocity = (target.Center + target.velocity * 60 - NPC.position) / 20;
                        VectorToTarget = target.Center - NPC.Center;
                        if (VectorToTarget.Length() < 200)
                        {
                            if (NPC.Center.Y > target.Center.Y)
                                NPC.velocity.Y += 10;
                            else
                                NPC.velocity.Y -= 10;

                            if (NPC.Center.X > target.Center.X)
                                NPC.velocity.X += 10;
                            else
                                NPC.velocity.X -= 10;
                        }
                    }
                    VectorToTarget = target.Center - NPC.Center;
                    if (aiCounter % 30 == 0)
                    {                       
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0, 0.5f);
                        if (aiCounter % 90 == 0)
                        {
                            Vector2 MyVector = VectorToTarget.RotatedBy(Pi / 2);
                            for (int i = 0; i < 18; i++)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, MyVector.SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                                MyVector = MyVector.RotatedBy(Pi / 18);
                            }
                        }
                    }
                    /*
                    if (aiCounter % 60 == 0)
                    {
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 4f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -4f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                    }
                    */
                    if (aiCounter == 900)
                    {
                        aiCounter = 0;
                        AIState = States.DarkSpawn;
                        Main.projectile[FindFirstProjectile(ModContent.ProjectileType<DarkMonster>())].ai[0] = 1;
                        return;
                    }
                    break;
                case States.DarkSpawn:
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    if(aiCounter == 0)
                        attackCounter = 0;

                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;

                    if (aiCounter % 15 == 0 && attackCounter < 8)
                    {
                        NPC.NewNPC(NPC.GetSource_FromThis(), (int)target.Center.X, (int)target.Center.Y - 350, ModContent.NPCType<DarkSpawn>());
                        attackCounter++;
                    }
                    if (!NPC.AnyNPCs(ModContent.NPCType<DarkSpawn>()) && aiCounter < 660)
                        aiCounter = 570;
                    if (aiCounter >= 600)
                    {
                        aiCounter = -30;
                        SoundEngine.PlaySound(DashWarn);
                        attackCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.DarkOrbit:
                    if (aiCounter >= 0)
                    {
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (aiCounter == 0)
                        {
                            NPC.position.X = target.position.X;
                            NPC.position.Y = target.position.Y - 400;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[3] = Main.rand.Next(300, 400);
                        }
                        if (aiCounter < NPC.ai[3] - 45)
                        {
                            VectorToTarget = target.Center - NPC.Center;
                            NPC.velocity.X = (float)(20 * Math.Cos((double)aiCounter / 25));
                            NPC.velocity.Y = (float)(20 * Math.Sin((double)aiCounter / 25));
                            NPC.position += target.velocity;
                            if (aiCounter % 10 == 0)
                            {                               
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0);
                            }
                            if (CalamityWorld.revenge && aiCounter < NPC.ai[3] - 90)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * -15, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                        }
                        else if(aiCounter < NPC.ai[3])
                        {
                            if (aiCounter == NPC.ai[3] - 30)
                            {
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -4;
                            }
                        }
                        else
                        {
                            if (aiCounter == NPC.ai[3])
                            {
                                VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -50;
                                SoundEngine.PlaySound(Dash);
                                //values gotten from Astrum Deus' contact damage. Subject to change.
                                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                            }
                            NPC.velocity = VectorToTarget;
                            VectorToTarget -= VectorToTarget.SafeNormalize(Vector2.UnitX);

                            #region Dash Projectiles
                            if (aiCounter % 10 == 0 && CalamityWorld.revenge && (attackCounter == 2 && CalamityWorld.death || !CalamityWorld.death))
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            else if (aiCounter % 5 == 0 && Main.expertMode)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                if (CalamityWorld.revenge && (attackCounter == 2 && CalamityWorld.death || !CalamityWorld.death))
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            }
                            #endregion

                            if (VectorToTarget.Length() <= 1)
                            {
                                NPC.damage = 0;
                                if (++attackCounter == 3 || !CalamityWorld.death)
                                {
                                    aiCounter = 0;
                                    AIState = States.DarkMonster;
                                }
                                else
                                    aiCounter = (int)NPC.ai[3];
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (NPC.Center.Y < target.Center.Y - 250)
                            NPC.velocity.Y++;
                        else
                            NPC.velocity.Y--;
                        if (NPC.Center.X < target.Center.X)
                            NPC.velocity.X++;
                        else
                            NPC.velocity.X--;
                        if (NPC.velocity.Length() > 15)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    }
                    break;
                case States.DarkSlice:
                    if (aiCounter == 0)
                    {
                        SoundEngine.PlaySound(Dash);
                        VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -60;                       
                        //values gotten from Astrum Deus' contact damage. Subject to change.
                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                    }
                    if (aiCounter > 0)
                    {
                        NPC.velocity = VectorToTarget;
                        VectorToTarget -= VectorToTarget.SafeNormalize(Vector2.UnitX);

                        #region Dash Projectiles
                        if (aiCounter % 10 == 0 && CalamityWorld.revenge)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        else if (aiCounter % 5 == 0 && Main.expertMode)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            if (CalamityWorld.revenge)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                        }
                        #endregion

                        if (VectorToTarget.Length() <= 1)
                        {
                            NPC.damage = 0;
                            aiCounter = -30;
                            if (++attackCounter == 3)
                            {
                                aiCounter = 0;
                                AIState = States.DarkStorm;
                            }
                            else
                            {
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                            }
                            return;
                        }
                    }
                    break;
                case States.DarkStorm:
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    if(aiCounter > 120 && NPC.Center.Y < target.Center.Y - 50)
                    {
                        if (aiCounter % 5 == 0)
                        {
                            //The Anti-Cheesers
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X - 400, target.Center.Y - 600), new Vector2(Main.rand.NextFloat(-3f, -1), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X + 400, target.Center.Y - 600), new Vector2(Main.rand.NextFloat(1, 3f), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);
                            
                            if (aiCounter % 10 == 0)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(-10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                if (CalamityWorld.revenge && aiCounter % 20 == 0)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(0, -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                            }
                        }
                        
                    }
                    if(aiCounter > 720)
                    {
                        aiCounter = -60;
                        attackCounter = 0;
                        AIState = States.DarkOrbit;
                    }
                    break;
            }
            aiCounter++;
            if (hitTimer > 0)
                hitTimer--;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                hitTimer = 35;
                NPC.netUpdate = true;
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i <= 50; i++)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle);
                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
            }
            NPC.downedAncientCultist = true;
            //WorldGen.TriggerLunarApocalypse();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.Center.Y) - screenPos;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            DrawForcefield(spriteBatch);
            return false;
        }

        public void DrawForcefield(SpriteBatch spriteBatch)
        {
            spriteBatch.EnterShaderRegion();
            float intensity = 35f / 35f;

            // Shield intensity is always high during invincibility, except during cast animations, so that she can be more easily seen.
            if (NPC.immortal)
                intensity = 0.75f + Math.Abs((float)Math.Cos(Main.GlobalTimeWrappedHourly * 1.7f)) * 0.1f;

            // Make the forcefield weaker in the second phase as a means of showing desparation.
            if (NPC.ai[0] >= 3f)
                intensity *= 0.6f;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            float flickerPower = 0f;
            if (lifeRatio < 0.6f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.3f)
                flickerPower += 0.15f;
            if (lifeRatio < 0.1f)
                flickerPower += 0.2f;
            if (lifeRatio < 0.05f)
                flickerPower += 0.1f;
            float opacity = forcefieldOpacity;
            opacity *= Lerp(1f, Max(1f - flickerPower, 0.56f), (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * Lerp(3f, 5f, flickerPower)), 24D));

            // During/prior to a charge the forcefield is always darker than usual and thus its intensity is also higher.
            if (!NPC.dontTakeDamage)
                intensity = 1.1f;

            // Dampen the opacity and intensity slightly, to allow SCal to be more easily visible inside of the forcefield.
            intensity *= 0.75f;
            opacity *= 0.75f;

            Texture2D forcefieldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SupremeCalamitas/ForcefieldTexture").Value;
            GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/Perlin");

            Color forcefieldColor = Color.Black;
            Color secondaryForcefieldColor = new Color(117, 255, 159) * 1.4f;

            if (!NPC.dontTakeDamage && 1 == 0)
            {
                forcefieldColor *= 0.25f;
                secondaryForcefieldColor = Color.Lerp(secondaryForcefieldColor, Color.Black, 0.7f);
            }

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(intensity);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

            spriteBatch.Draw(forcefieldTexture, NPC.Center - Main.screenPosition, null, Color.White * opacity, 0f, forcefieldTexture.Size() * 0.5f, forcefieldScale * 3f, SpriteEffects.None, 0f);

            spriteBatch.ExitShaderRegion();
        }
    }
}
