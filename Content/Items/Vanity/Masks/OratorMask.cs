﻿namespace Windfall.Content.Items.Vanity.Masks;

[AutoloadEquip(EquipType.Head)]
public class OratorMask : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetStaticDefaults()
    {
        if (Main.netMode != NetmodeID.Server)
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;
    }
}
