using CalamityMod;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Windfall.Common.Utilities;

namespace Windfall.Content.WorldEvents.CalClone
{
    public class CalCloneSpawnSystem : ModSystem
    {
        public enum SystemState
        {
            CheckReqs,
            CheckChance,
            Spawn,
        }
        //used to stop Calamitas from spawning regardless of other conditions. serves as a cooldown so she only spawns once a night
        internal int CalDown = 1;
        public static bool cragsCal;
        internal double timeTillSpawn;
        internal int cragsTimer;

        public SystemState State = SystemState.CheckReqs;

        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            //Requirements for Calamitas to spawn.
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (mainPlayer.ZoneUnderworldHeight)
                    {
                        if (!Main.hardMode || !mainPlayer.Calamity().ZoneCalamity || CalDown == 1 || CalamityUtils.AnyBossNPCS())
                        {
                            if (!mainPlayer.Calamity().ZoneCalamity)
                            {
                                //resets the cooldown when outside of the crags.
                                CalDown = 0;
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (!Main.hardMode || Main.dayTime || CalDown == 1 || CalamityUtils.AnyBossNPCS())
                        {
                            if (Main.dayTime)
                            {
                                //resets the cooldown when day comes.
                                CalDown = 0;
                            }
                            break;
                        }
                    }
                    State = SystemState.CheckChance;
                    break;
                case SystemState.CheckChance:
                    if (Main.rand.NextBool(5))
                    {
                        State = SystemState.Spawn;
                    }
                    else
                    {
                        CalDown = 1;
                        State = SystemState.CheckReqs;
                    }
                    break;
                case SystemState.Spawn:
                    if (CalDown != 2)
                    {
                        Main.NewText("Something calamitous is approaching...", Color.Red);
                        //defines the delay between night starting and CalClone spawning
                        if (mainPlayer.Calamity().ZoneCalamity)
                        {
                            cragsCal = true;
                            cragsTimer = 0;
                            timeTillSpawn = Main.rand.Next(1800, 3600);
                        }
                        else
                        {
                            cragsCal = false;
                            timeTillSpawn = Main.nightLength / Main.rand.Next(2, 4);
                        }
                        CalDown = 2;
                    }
                    else
                    {
                        if (cragsCal)
                        {
                            if (!mainPlayer.Calamity().ZoneCalamity || CalamityUtils.AnyBossNPCS())
                            {
                                Main.NewText("Something calamitous has vanished...", Color.Red);
                                State = SystemState.CheckReqs;
                                CalDown = 1;
                                break;
                            }
                        }
                        else
                        {
                            if (Main.dayTime || CalamityUtils.AnyBossNPCS())
                            {
                                Main.NewText("Something calamitous has vanished...", Color.Red);
                                State = SystemState.CheckReqs;
                                CalDown = 1;
                                break;
                            }
                        }
                    }
                    if (!cragsCal)
                    {
                        if (Main.time > timeTillSpawn && mainPlayer.townNPCs > 2f && !mainPlayer.dead && mainPlayer.active)
                        {
                            Main.NewText("Something calamitous has awoken!", Color.Red);
                            //sets the spawn location 
                            Vector2 CalCloneSpawnLocation;
                            if (mainPlayer.direction == 1)
                            {
                                CalCloneSpawnLocation.X = mainPlayer.Center.X + 1000f;
                            }
                            else
                            {
                                CalCloneSpawnLocation.X = mainPlayer.Center.X - 1000f;
                            }

                            CalCloneSpawnLocation.Y = mainPlayer.Center.Y;
                            //multiplayer shenanaganry: spawns the projectile for all players
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                Player p = Main.player[i];
                                if (!p.dead && p.active)
                                {
                                    Utilities.NewProjectileBetter(CalCloneSpawnLocation, Vector2.Zero, ModContent.ProjectileType<CalCloneProj>(), 0, 0f);
                                    break;
                                }
                            }
                            State = SystemState.CheckReqs;
                            CalDown = 1;
                        }
                    }
                    else
                    {
                        cragsTimer++;
                        if (cragsTimer >= timeTillSpawn && !mainPlayer.dead && mainPlayer.active)
                        {
                            Main.NewText("Something calamitous has awoken!", Color.Red);
                            //sets the spawn location 
                            Vector2 CalCloneSpawnLocation;
                            if (mainPlayer.direction == 1)
                            {
                                CalCloneSpawnLocation.X = mainPlayer.Center.X + 1000f;
                            }
                            else
                            {
                                CalCloneSpawnLocation.X = mainPlayer.Center.X - 1000f;
                            }

                            CalCloneSpawnLocation.Y = mainPlayer.Center.Y;
                            //multiplayer shenanaganry: spawns the projectile for all players
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                Player p = Main.player[i];
                                if (!p.dead && p.active)
                                {
                                    Utilities.NewProjectileBetter(CalCloneSpawnLocation, Vector2.Zero, ModContent.ProjectileType<CalCloneProj>(), 0, 0f);
                                    break;
                                }
                            }
                            State = SystemState.CheckReqs;
                            CalDown = 1;
                        }

                    }
                    break;
            }
        }
    }
}
