﻿

namespace Windfall.Content.Items.Quests.SealingRitual;

public class PrimalLightShard : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "Windfall/Assets/Items/Quest/PrimalLightShard";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Type] = true;
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 30;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 99;
    }
    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        Lighting.AddLight(Item.Center, Color.White.ToVector3());
    }
}
