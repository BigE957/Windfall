﻿using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Windfall.UI.WanderersJournals;
using Windfall.Utilities;


namespace Windfall.Items.Journals
{
    public class JournalIlmeris : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Journals";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool OnPickup(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                WorldSaveSystem.JournalsCollected[3] = true;
            }
            return true;
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (!JournalUISystem.isJournalOpen)
                {
                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(Windfall)}.JournalContents.Ilmeris").Value;
                    ModContent.GetInstance<JournalUISystem>().ShowPageUI();
                }
                else
                {
                    ModContent.GetInstance<JournalUISystem>().HideWandererUI();
                }
            }
            return true;
        }
    }
}