using CalamityMod.Items.Fishing.FishingRods;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.Items.Fishing;

namespace Windfall.Content.Projectiles.Fishing
{
    public class AncientIlmeranBobber : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Fishing";
        public override string Texture => "Windfall/Assets/Projectiles/Fishing/AncientIlmeranBobber";
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 30;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
            Projectile.penetrate = -1;
        }

        public override bool PreDrawExtras()
        {
            return Projectile.DrawFishingLine(ModContent.ItemType<AncientIlmeranRod>(), new Color(144, 65, 57, 100), 38, 28f);
        }
    }
}
