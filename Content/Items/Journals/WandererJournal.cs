using Windfall.Common.Systems;
using Windfall.Content.UI.WanderersJournals;
using static Windfall.Content.UI.WanderersJournals.JournalFullUIState;

namespace Windfall.Content.Items.Journals;

public class WandererJournal : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Journals";
    public override string Texture => "Windfall/Assets/Items/Journals/WandererJournal";
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.scale = 0.75f;
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
                for (JournalTypes i = 0; (int)i < 12; i++)
                {
                    if (WorldSaveSystem.JournalsCollected[(int)i])
                    {
                        if (i == JournalTypes.Evil)
                        {
                            if (JournalUISystem.whichEvilJournal == "Crimson")
                            {
                                JournalText.JournalContents = GetWindfallTextValue("UI.JournalContents.Crimson");
                            }
                            else if (JournalUISystem.whichEvilJournal == "Corruption")
                            {
                                JournalText.JournalContents = GetWindfallTextValue("UI.JournalContents.Corruption");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            JournalText.JournalContents = GetWindfallTextValue($"UI.JournalContents.{i}");

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