using CalamityMod.Items;

namespace Windfall.Content.Items.Tools;
public class HammerChisel : ModItem
{
    public override string Texture => "CalamityMod/Items/Weapons/Melee/FallenPaladinsHammer";
    public override void SetDefaults()
    {
        Item.damage = 0;
        Item.useAnimation = Item.useTime = 9;

        Item.width = 30;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.useTurn = true;

        Item.hammer = 99;
    }
}
