﻿using Windfall.Content.Tiles.Furnature;

namespace Windfall.Content.Items.Placeables.Furnature;
public class OratorTrophy : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.value = 50000;
        Item.rare = ItemRarityID.Blue;
        Item.createTile = ModContent.TileType<OratorTrophyTile>();
    }
}
