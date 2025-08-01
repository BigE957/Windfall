﻿using Windfall.Content.Tiles.Furnature;

namespace Windfall.Content.Items.Placeables.Furnature;
public class OratorRelic : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        // Vanilla has many useful methods like these, use them! This substitutes setting Item.createTile and Item.placeStyle aswell as setting a few values that are common across all placeable items
        Item.DefaultToPlaceableTile(ModContent.TileType<OratorRelicTile>(), 0);

        Item.width = 30;
        Item.height = 40;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Master;
        Item.master = true; // This makes sure that "Master" displays in the tooltip, as the rarity only changes the item name color
        Item.value = Item.buyPrice(0, 5);
    }
}
