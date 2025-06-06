﻿using Terraria.UI.Chat;
using Windfall.Common.Players;
using Windfall.Common.Systems;
using Windfall.Content.Items.Journals;

namespace Windfall.Content.Items.GlobalItems;

public class WindfallGlobalItem : GlobalItem
{
    public static readonly HashSet<int> SpacialLockAffectedItems = [];

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        // only exists to fix an issue with WMITF that was bugging me
        string overrideText = null;
        if (line.Text == "[Calamity: Windfall]")
            overrideText = "[Windfall]";

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, overrideText ?? line.Text, new Vector2(line.X, line.Y), line.OverrideColor ?? line.Color, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
        return false;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (GodlyPlayer.IsUsingAbility(player))
            return false;

        if(player.Buff().SpacialLock && SpacialLockAffectedItems.Contains(item.type))
            return false;

        return true;
    }       
    
    public override bool? UseItem(Item item, Player player)
    {
        bool isJournalPage = item.type == ModContent.ItemType<JournalCorruption>() || item.type == ModContent.ItemType<JournalCrimson>() || item.type == ModContent.ItemType<JournalForest>() || item.type == ModContent.ItemType<JournalTundra>() || item.type == ModContent.ItemType<JournalIlmeris>() || item.type == ModContent.ItemType<JournalJungle>() || item.type == ModContent.ItemType<JournalDesert>() || item.type == ModContent.ItemType<JournalDungeon>() || item.type == ModContent.ItemType<JournalOcean>() || item.type == ModContent.ItemType<JournalSulphur>();

        if (isJournalPage && FirstJournal() == true)
            Item.NewItem(null, player.Center, 1, 1, ModContent.ItemType<WandererJournal>());
        //if (item.type == ItemID.PurificationPowder && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "Decontamination")].Active)
        //    QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "Decontamination"));
        return base.UseItem(item, player);
    }
    internal static bool FirstJournal()
    {
        for (int i = 0; i < WorldSaveSystem.JournalsCollected.Count; i++)
        {
            if (WorldSaveSystem.JournalsCollected[i] == true)
                return false;
        }
        return true;
    }
    public static void InsertJournalTooltop(List<TooltipLine> tooltips)
    {
        TooltipLine tooltipLine = new(Windfall.Instance, "JournalPage", "Use to add this page to your Wanderer's Journal");
        tooltips.Add(tooltipLine);
    }
    
    public override bool OnPickup(Item item, Player player)
    {
        //if (item.type == ItemID.QueenSlimeCrystal)
            //QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "CrystalHunt"));
        return true;
    }
}
