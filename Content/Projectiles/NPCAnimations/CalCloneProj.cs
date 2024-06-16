using CalamityMod.Dusts;
using CalamityMod.NPCs.Crags;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.CalClone;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class CalCloneProj : ModProjectile
    {
        //nabs some sounds from Calamity
        private static readonly SoundStyle DashSound = new("CalamityMod/Sounds/Custom/SCalSounds/SCalDash");
        private static readonly SoundStyle Teleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private static readonly SoundStyle BrotherSummon = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneBigShoot");
        //ai states define what our projectile should be doing
        private enum AIState
        {
            Spawning,
            WaitingForPlayer,
            Shocked,
            Summoning,
            Fleeing,
        }
        //defines CurrentAI and how it interacts with Projectile.ai
        private AIState CurrentAI
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/WanderingCalClone";
        private float i = 0f;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Calamitas");
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
        }
        public override void AI()
        {
            //despawns CalClone when day comes if the player is not in the crags
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (Main.dayTime && !closestPlayer.Calamity().ZoneCalamity)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(Teleport, Projectile.Center);
                Projectile.Kill();
            }

            //increments through CalClone's spritesheet
            if (++Projectile.frameCounter > 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            //detects the closest player to this projectile and faces them
            Projectile.spriteDirection = Projectile.direction = (closestPlayer.Center.X > Projectile.Center.X).ToDirectionInt();

            Lighting.AddLight(Projectile.Center, 0.9f, 0.1f, 0.3f);

            switch (CurrentAI)
            {
                case AIState.Spawning:
                    Point worldPosition = new Point((int)Projectile.position.ToTileCoordinates().X, (int)Projectile.position.ToTileCoordinates().Y);
                    float groundY = Utilities.FindGroundVertical(worldPosition).ToWorldCoordinates().Y;
                    Projectile.position.Y = groundY - Projectile.height - 32;
                    CurrentAI = AIState.WaitingForPlayer;
                    break;

                case AIState.WaitingForPlayer:
                    //waits until the player is close by
                    if (Projectile.WithinRange(closestPlayer.Center, 320f))
                    {
                        CurrentAI = AIState.Shocked;
                        Projectile.velocity.Y = 0;
                        if (closestPlayer.Center.X > Projectile.Center.X)
                            i = -20;
                        else
                            i = 20;
                        SoundEngine.PlaySound(DashSound, Projectile.Center);
                    }
                    else
                    {
                        i++;
                        //sine wave hover for CalClone
                        Projectile.velocity.Y = (float)(Math.Sin(i / 10) * 1);
                    }
                    break;
                case AIState.Shocked:
                    //Makes CalClone dash backwards away from the player
                    if (i > 0)
                        Projectile.velocity.X = i-- / 2;
                    else if (i < 0)
                        Projectile.velocity.X = i++ / 2;
                    else
                    {
                        //creates an Orange Combat Text out of the projectile
                        Color messageColor = Color.Orange;
                        Rectangle location = new((int)Projectile.Center.X, (int)Projectile.Center.Y, Projectile.width, Projectile.width);
                        CombatText.NewText(location, messageColor, "?!", true);
                        i = 30;
                        Projectile.velocity.X = 0;
                        if (CalCloneSpawnSystem.cragsCal)
                            CurrentAI = AIState.Summoning;
                        else
                            CurrentAI = AIState.Fleeing;
                    }

                    break;
                //exclusive to Brimstone Crags encounters, Calamitas will summon either three Soul Slupers of, if a Mech Boss is defeated, one of the Brothers
                case AIState.Summoning:
                    i--;
                    if (i == 0)
                    {
                        if (NPC.downedMechBossAny)
                        {
                            for (int i = 0; i < 40; i++)
                            {
                                int brimDust = Dust.NewDust(new Vector2((int)Projectile.Center.X - 50, (int)Projectile.position.Y - 200), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust].velocity *= 3f;
                                if (Main.rand.NextBool())
                                {
                                    Main.dust[brimDust].scale = 0.5f;
                                    Main.dust[brimDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                                }
                            }
                            for (int j = 0; j < 70; j++)
                            {
                                int brimDust2 = Dust.NewDust(new Vector2((int)Projectile.Center.X - 50, (int)Projectile.position.Y - 200), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                                Main.dust[brimDust2].noGravity = true;
                                Main.dust[brimDust2].velocity *= 5f;
                                brimDust2 = Dust.NewDust(new Vector2((int)Projectile.Center.X - 50, (int)Projectile.position.Y - 200), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust2].velocity *= 2f;
                            }
                            SoundEngine.PlaySound(BrotherSummon, Projectile.Center + new Vector2(0, -200));
                            if (Main.rand.NextBool(2))
                                NPC.NewNPC(null, (int)Projectile.Center.X, (int)Projectile.position.Y - 150, ModContent.NPCType<WorldCataclysm>());
                            else
                                NPC.NewNPC(null, (int)Projectile.Center.X, (int)Projectile.position.Y - 150, ModContent.NPCType<WorldCatastrophe>());
                        }
                        else
                        {
                            for (int i = 0; i < 50; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
                                Dust d1 = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -50), DustID.Blood, speed * 5, Scale: 1.5f);
                                d1.noGravity = true;
                                Dust d2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(25, -25), DustID.Blood, speed * 5, Scale: 1.5f);
                                d2.noGravity = true;
                                Dust d3 = Dust.NewDustPerfect(Projectile.Center + new Vector2(-25, -25), DustID.Blood, speed * 5, Scale: 1.5f);
                                d3.noGravity = true;
                            }
                            NPC.NewNPC(null, (int)Projectile.Center.X, (int)Projectile.position.Y - 50, ModContent.NPCType<SoulSlurper>());
                        }
                        i = 60;
                        CurrentAI = AIState.Fleeing;
                    }
                    break;
                //despawns CalClone with a slight delay (nearly the same as above when day comes)
                case AIState.Fleeing:
                    i--;
                    if (i == 0)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(Teleport, Projectile.Center);
                        Projectile.Kill();
                    }
                    else if (i == 31)
                    {
                        Color messageColor = Color.Orange;
                        Rectangle location = new((int)Projectile.Center.X, (int)Projectile.Center.Y, Projectile.width, Projectile.width);
                        CombatText.NewText(location, messageColor, GetWindfallTextValue("Dialogue.CalClone.WorldText.Brothers"), true);
                    }
                    break;
            }
        }
    }
}
