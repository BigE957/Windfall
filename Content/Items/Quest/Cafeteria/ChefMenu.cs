using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Windfall.Content.UI.WanderersJournals.JournalFullUIState;
using Windfall.Common.Systems;
using Windfall.Content.UI.WanderersJournals;
using Windfall.Content.UI.Activities;

namespace Windfall.Content.Items.Quest.Cafeteria;
internal class ChefMenu : ModItem
{
    public override string Texture => "Windfall/Assets/Items/Journals/JournalForest";
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
            if (!ChefMenuUISystem.isChefMenuOpen)
                ModContent.GetInstance<ChefMenuUISystem>().ShowChefMenuUI();
            else
                ModContent.GetInstance<ChefMenuUISystem>().HideChefMenuUI();
        }
        return true;
    }
}