using CalamityMod.Items.Fishing.FishingRods;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Items.Fishing;

namespace Windfall.Projectiles.Fishing
{
    public class AncientIlmeranBobber : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Fishing";
        public override string Texture => "CalamityMod/Projectiles/Typeless/WulfrumBobber";
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
            Projectile.penetrate = -1;
        }

        public override bool PreDrawExtras()
        {
            return Projectile.DrawFishingLine(ModContent.ItemType<AncientIlmeranRod>(), new Color(200, 200, 200, 100), 38, 28f);
        }
    }
}
