using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WindfallAttempt1.UI.WanderersJournals;

namespace WindfallAttempt1.Items.Journals
{
    public class JournalForest : ModItem
    {
        internal bool isJournalOpen = false;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (!isJournalOpen)
                {
                    isJournalOpen = true;
                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.Forest").Value;
                    ModContent.GetInstance<JournalUISystem>().ShowMyUI();
                }
                else
                {
                    isJournalOpen = false;
                    ModContent.GetInstance<JournalUISystem>().HideMyUI();
                }
            }
            return true;
        }
    }
}