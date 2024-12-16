using CalamityMod.Rarities;
using CalamityMod.Skies;
using CalamityMod.Tiles.Furniture.Monoliths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Windfall.Content.Tiles.Furnature;

namespace Windfall.Content.Items.Placeables.Furnature;

public class OratorMonolith : ModItem, ILocalizedModType, IModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 28;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<OratorMonolithTile>();
        Item.rare = ItemRarityID.Cyan;
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Monolith().OratorMonolith = 30;
        }
    }

    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Monolith().OratorMonolith = 30;
        }
    }
}