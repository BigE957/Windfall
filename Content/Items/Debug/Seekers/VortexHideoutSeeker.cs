﻿using CalamityMod.Items;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Items.Debug.Seekers
{
    public class VortexHideoutSeeker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "CalamityMod/Items/LabFinders/GreenSeekingMechanism";

        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/IlmeranHorn");

        public override void SetDefaults()
        {
            Item.useAnimation = Item.useTime = 100;
            Item.width = 42;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = UseSound;
            Item.useTurn = true;
        }
        public override bool? UseItem(Player player)
        {
            Main.NewText($"You are {player.position.X / 16 - LunarCultActivitySystem.VortexHideoutLocation.X}, {player.position.Y / 16 - LunarCultActivitySystem.VortexHideoutLocation.Y} from the Vortex Hideout.", Color.Yellow);
            return true;
        }
    }
}
