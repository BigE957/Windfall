using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WindfallAttempt1.UI.WanderersJournals;
using static WindfallAttempt1.UI.WanderersJournals.JournalFullUIState;

namespace WindfallAttempt1.Items.Journals
{
    public class JournalCompilation : ModItem
    {
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
                if (!JournalUISystem.isJournalOpen)
                {
                    JournalText.JournalContents = "";
                    //JournalText.JournalContents = "Mfw I'm the compilation item and therefore do not need to tell you my contents because I'm the coolest :sunglasses:";
                    for (JournalTypes i = 0; (int)i < 12; i++)
                    {
                        if (JournalUISystem.JournalsCollected[(int)i])
                        {
                            if (i == JournalTypes.Evil)
                            {
                                if (JournalUISystem.whichEvilJournal == "Crimson")
                                {
                                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.Crimson").Value;
                                }
                                else if (JournalUISystem.whichEvilJournal == "Corruption")
                                {
                                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.Corruption").Value;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.JournalContents.{i}").Value;
                            }
                            PageNumber = (int)i;
                            break;
                        }
                    }
                    ModContent.GetInstance<JournalUISystem>().ShowJournalUI();
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