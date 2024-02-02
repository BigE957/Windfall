using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Windfall.Items.Journals;
using Windfall.Systems;

namespace Windfall.Items
{
    //            TooltipLine tooltipLine = new TooltipLine(Windfall.Instance, "JournalPage", "Use to add this page to your Wanderer's Journal");

    public class WindfallGlobalItem : GlobalItem
    {
        public override bool? UseItem(Terraria.Item item, Player player)
        {
            bool isJournalPage = item.type == ModContent.ItemType<JournalCorruption>() || item.type == ModContent.ItemType<JournalCrimson>() || item.type == ModContent.ItemType<JournalForest>() || item.type == ModContent.ItemType<JournalTundra>() || item.type == ModContent.ItemType<JournalIlmeris>() || item.type == ModContent.ItemType<JournalJungle>() || item.type == ModContent.ItemType<JournalDesert>() || item.type == ModContent.ItemType<JournalDungeon>() || item.type == ModContent.ItemType<JournalOcean>() || item.type == ModContent.ItemType<JournalSulphur>();

            if(isJournalPage)
            {
                if(FirstJournal() == true)
                {
                    Terraria.Item.NewItem(null, player.Center, 1, 1, ModContent.ItemType<JournalCompilation>());
                }
            }
            return base.UseItem(item, player);
        }
        internal static bool FirstJournal()
        {
            for(int i = 0; i < WorldSaveSystem.JournalsCollected.Count; i++)
            {
                if (WorldSaveSystem.JournalsCollected[i] == true)
                    return false;
            }
            return true;
        }
        public static void InsertJournalTooltop(List<TooltipLine> tooltips)
        {
            TooltipLine tooltipLine = new TooltipLine(Windfall.Instance, "JournalPage", "Use to add this page to your Wanderer's Journal");
            tooltips.Add(tooltipLine);
        }
    }
}
