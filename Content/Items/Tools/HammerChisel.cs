using CalamityMod.Items;
using Luminance.Common.Utilities;

namespace Windfall.Content.Items.Tools;
public class HammerChisel : ModItem
{
    public override string Texture => "Windfall/Assets/Items/Tools/HammerAndChiselSmall";
    public override void SetDefaults()
    {
        Item.damage = 0;
        Item.useAnimation = Item.useTime = 15;

        Item.width = 30;
        Item.height = 40;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<HammerChiselProjectile>();

        Item.hammer = 99;
    }
}
