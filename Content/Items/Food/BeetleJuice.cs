using CalamityMod.Buffs.StatBuffs;

namespace Windfall.Content.Items.Food;
public class BeetleJuice : ModItem
{
    public override string Texture => $"Windfall/Assets/Items/Food/{nameof(BeetleJuice)}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;

        // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(240, 240, 240),
            new Color(200, 200, 200),
            new Color(140, 140, 140)
        ];
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 26;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item3;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.buyPrice(gold: 1);
        Item.buffType = ModContent.BuffType<AbsorberRegen>();
        Item.buffTime = 5400;
    }
}
