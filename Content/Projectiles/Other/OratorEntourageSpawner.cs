using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.NPCs.WanderingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Projectiles.Other
{
    public class OratorEntourageSpawner : ModProjectile
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_Head";
        public new static string LocalizationCategory => "Projectiles.Other";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }
        public override void OnSpawn(IEntitySource source)
        {
            counter = 0;
        }
        int counter = 0;
        public override void AI()
        {
            Player player = Main.player[0];
            switch (counter)
            {
                case 60 * 2:
                    NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(150, 0), ModContent.NPCType<LunarCultistArcher>(), 0, 0);
                    NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-150, 0), ModContent.NPCType<LunarCultistArcher>(), 0, 0);
                    break;

                case 60 * 3:
                    NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(110, 0), ModContent.NPCType<LunarCultistDevotee>(), 0, 0);
                    Projectile.NewProjectile(Entity.GetSource_FromAI(), player.Center + new Vector2(60, 0), Vector2.Zero, ModContent.ProjectileType<OratorProj>(), 0, 0);
                    NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-110, 0), ModContent.NPCType<LunarCultistDevotee>(), 0, 0);
                    break;

                case 60 * 4:
                    NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-60, 0), ModContent.NPCType<LunarBishop>(), 0, 0);
                    Projectile.active = false;
                    break;
            }
            counter++;
        }
    }
}
