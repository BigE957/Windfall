﻿using CalamityMod.Items;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Items.Debug.Seekers
{
    public class SolarHideoutSeeker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "CalamityMod/Items/LabFinders/YellowSeekingMechanism";

        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/IlmeranHorn");

        public override void SetDefaults()
        {
            Item.useAnimation = Item.useTime = 100;
            Item.width = 42;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = CalamityGlobalItem.Rarity2BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = UseSound;
            Item.useTurn = true;
        }
        public override bool? UseItem(Player player)
        {
            Main.NewText($"You are {player.position.X / 16 - CultMeetingSystem.SolarHideoutLocation.X}, {player.position.Y / 16 - CultMeetingSystem.SolarHideoutLocation.Y} from the Solar Hideout.", Color.Yellow);
            return true;
        }
    }
}