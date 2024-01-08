using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WindfallAttempt1.UI.WanderersJournals;
using WindfallAttempt1.Utilities;


namespace WindfallAttempt1.Items.Journals
{
    public class JournalCorruption : ModItem
    {
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
                WorldSaveSystem.JournalsCollected[4] = true;
                JournalUISystem.whichEvilJournal = "Corruption";
            }
            return true;
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (!JournalUISystem.isJournalOpen)
                {
                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.Corruption").Value;
                    ModContent.GetInstance<JournalUISystem>().ShowPageUI();
                }
                else
                {
                    ModContent.GetInstance<JournalUISystem>().HideMyUI();
                }
            }
            return true;
        }
    }
}