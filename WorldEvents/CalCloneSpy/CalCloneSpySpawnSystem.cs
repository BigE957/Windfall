using CalamityMod.NPCs.Signus;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using System.Threading;

namespace WindfallAttempt1.WorldEvents.CalCloneSpy
{
    public class CalCloneSpySpawnSystem : ModSystem
    {
        //used to stop Calamitas from spawning regardless of other conditions. serves as a cooldown so she only spawns once a night
        public int CalDown = 0;
        public double timeTillSpawn;
        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            //Requirements for Calamitas to spawn.
            if (!Main.hardMode || Main.dayTime || CalDown == 1 || CalamityUtils.AnyBossNPCS())
            {
                if (Main.dayTime)
                {
                    //resets the cooldown when day comes.
                    CalDown = 0;
                }
                return;
            }else if (!Main.rand.NextBool(5) && CalDown != 2)
            {
                // 1/5 chance for the spawn to go through
                CalDown = 1;
                return;
            }
            else
            {
                if (CalDown != 2)
                {
                    Main.NewText("Something calamitous is approaching...", Color.Red);
                    //defines the delay between night starting and CalClone spawning
                    timeTillSpawn = Main.nightLength / Main.rand.Next(2, 4);
                    CalDown = 2;
                }
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
                            Utilities.NewProjectileBetter(CalCloneSpawnLocation, Vector2.Zero, ModContent.ProjectileType<CalCloneSpyProj>(), 0, 0f);
                            break;
                        }
                    }
                    CalDown = 1;
                }
                else
                {
                    return;
                }
            }
        }
    }
}
