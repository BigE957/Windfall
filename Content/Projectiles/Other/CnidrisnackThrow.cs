using CalamityMod.Items.Weapons.Rogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Content.Projectiles.Other
{
    public class CnidrisnackThrow : ModProjectile
    {
        public override string Texture => "Windfall/Assets/Items/Weapons/Misc/Cnidrisnack";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.DamageType = DamageClass.Default;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.aiStyle = 2;
        }
        public override void OnKill(int timeLeft)
        {
            Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<Cnidrisnack>());
        }
    }
}
