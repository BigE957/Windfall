using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Windfall.NPCs.WanderingNPCs;

namespace Windfall.Projectiles.NPCAnimations
{
    public class IlmeranPaladinDig : ModProjectile, ILocalizedModType
    {
        public override string Texture => "Windfall/NPCs/WanderingNPCs/IlmeranPaladin";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 25;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.998f;
            Projectile.velocity.Y += 0.01f;
            if (Projectile.Center.Y < Main.player[Projectile.owner].Center.Y)
            {
                NPC.NewNPC(null, (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<IlmeranPaladin>(), 0, Projectile.velocity.Y);
                Projectile.active = false;
            }
        }
    }
}
