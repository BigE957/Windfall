using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WindfallAttempt1.UI.WanderersJournals;

namespace WindfallAttempt1.Items.Journals
{
    public class JournalDesert : ModItem
    {
        internal bool isJournalOpen = false;
        public static readonly SoundStyle UseSound = new("WindfallAttempt1/Sounds/Items/JournalPageTurn");
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = Item.useTime = 20;
            Item.UseSound = UseSound with
            {
                Pitch = -0.25f,
                PitchVariance = 0.5f,
            };
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                SoundEngine.PlaySound(UseSound, player.Center);
                if (!isJournalOpen)
                {
                    isJournalOpen = true;
                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.Desert").Value;
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