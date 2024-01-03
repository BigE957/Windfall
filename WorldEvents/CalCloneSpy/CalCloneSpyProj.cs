using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;
using Terraria.ID;
using Terraria.Audio;

namespace WindfallAttempt1.WorldEvents.CalCloneSpy
{
    public class CalCloneSpyProj : ModProjectile
    {
        public static readonly SoundStyle DashSound = new("CalamityMod/Sounds/Custom/SCalSounds/SCalDash");
        public static readonly SoundStyle CalCloneTeleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public enum AIState
        {
            WaitingForPlayer,
            Shocked,
            Fleeing,
        }
        public float i = 0f;
        public int timeTillSpawn;
        public AIState CurrentAI
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Calamitas");
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
        }
        public override void AI()
        {
            if (Main.dayTime)
            {
                Projectile.Kill();
            }

            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Projectile.spriteDirection = Projectile.direction = (closestPlayer.Center.X > Projectile.Center.X).ToDirectionInt();
            
            Lighting.AddLight(Projectile.Center, 0.9f, 0.1f, 0.3f);

            switch (CurrentAI)
            {
                case AIState.WaitingForPlayer:
                    if (Projectile.WithinRange(closestPlayer.Center, 320f))
                    {
                        CurrentAI = AIState.Shocked;
                        Projectile.velocity.Y = 0;
                        if (closestPlayer.Center.X > Projectile.Center.X)
                        {
                            i = -20;
                        }
                        else
                        {
                            i = 20;
                        }
                        SoundEngine.PlaySound(DashSound, Projectile.Center);
                    }
                    else
                    {
                        i++;
                        Projectile.velocity.Y = (float)(Math.Sin(i/10)*1);
                    }
                    break;
                case AIState.Shocked:
                    if (i > 0)
                    {
                        Projectile.velocity.X = i--/2;
                    }
                    else if(i < 0)
                    {
                        Projectile.velocity.X = i++/2;
                    }
                    else
                    {
                        i = 30;
                        Projectile.velocity.X = 0;
                        CurrentAI = AIState.Fleeing;
                        Color messageColor = Color.DarkRed;
                        Rectangle location = new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, Projectile.width, Projectile.width);
                        CombatText.NewText(location, messageColor, "?!", true);
                    }

                    break;
                case AIState.Fleeing:
                    i--;
                    if (i == 0)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(CalCloneTeleport, Projectile.Center);
                        Projectile.Kill();
                    }

                    break;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position.Y -= 16; 
            return false;
        }
    }
}
