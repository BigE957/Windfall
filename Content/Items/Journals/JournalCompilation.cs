using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Windfall.Content.UI.WanderersJournals.JournalFullUIState;
using Windfall.Content.UI.WanderersJournals;
using Windfall.Content.Systems;

namespace Windfall.Content.Items.Journals
{
    public class JournalCompilation : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Journals";
        public override string Texture => "Windfall/Assets/Items/Journals/JournalCompilation";
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
                        if (WorldSaveSystem.JournalsCollected[(int)i])
                        {
                            if (i == JournalTypes.Evil)
                            {
                                if (JournalUISystem.whichEvilJournal == "Crimson")
                                {
                                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(Windfall)}.JournalContents.Crimson").Value;
                                }
                                else if (JournalUISystem.whichEvilJournal == "Corruption")
                                {
                                    JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(Windfall)}.JournalContents.Corruption").Value;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                JournalText.JournalContents = Language.GetOrRegister($"Mods.{nameof(Windfall)}.JournalContents.{i}").Value;
                            }
                            PageNumber = (int)i;
                            break;
                        }
                    }
                    ModContent.GetInstance<JournalUISystem>().ShowJournalUI();
                }
                else
                {
                    ModContent.GetInstance<JournalUISystem>().HideWandererUI();
                }
            }
            return true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Leather, 2);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
    }
}