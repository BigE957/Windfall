using Windfall.Content.NPCs.Enemies;

namespace Windfall.Content.NPCs.GlobalNPCs;

public class QuestGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        Mod windfall = Windfall.Instance;
        /*
        if ((npc.type == NPCID.EaterofSouls || npc.type == NPCID.Crimera) && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl")].Active)
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl"));

        if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Active)
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt"));

        if (npc.type == calamity.Find<ModNPC>("GiantClam").Type && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ClamHunt")].Active)
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ClamHunt"));

        if (npc.type == calamity.Find<ModNPC>("AquaticScourgeHead").Type && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2")].Active)
            QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2"));        
        */
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
