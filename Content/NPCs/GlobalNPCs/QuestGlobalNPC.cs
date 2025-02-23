using Windfall.Common.Systems;
using Windfall.Content.NPCs.Enemies;

namespace Windfall.Content.NPCs.GlobalNPCs;

public class QuestGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        /*
        if ((npc.type == NPCID.EaterofSouls || npc.type == NPCID.Crimera) && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl")].Active)
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl"));
        */
        Quest quest = QuestSystem.Quests["ScoogHunt"];
        if (quest.InProgress && npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
            quest.IncrementProgress();

        quest = QuestSystem.Quests["ClamHunt"];
        if (quest.InProgress && npc.type == calamity.Find<ModNPC>("GiantClam").Type)
            quest.IncrementProgress();

        quest = QuestSystem.Quests["ScoogHunt2"];
        if (quest.InProgress && npc.type == calamity.Find<ModNPC>("AquaticScourgeHead").Type)
            quest.IncrementProgress();
    }
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
            npc.Transform(ModContent.NPCType<WFCnidrion>());
        if (npc.type == NPCID.CultistArcherBlue || npc.type == NPCID.CultistDevote)
            npc.active = false;
    }
}
