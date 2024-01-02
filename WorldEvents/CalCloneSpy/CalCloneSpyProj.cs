using CalamityMod.Items.Placeables.Banners;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace WindfallAttempt1.WorldEvents.CalCloneSpy
{
    public class CalCloneSpyProj : ModProjectile

    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Calamitas");
            Main.projFrames[Type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
        }
    }
}
