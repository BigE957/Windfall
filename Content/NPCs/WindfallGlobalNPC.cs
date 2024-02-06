using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Enemies;

namespace Windfall.Content.NPCs
{
    public class WindfallGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type || npc.type == ModContent.NPCType<WFCnidrion>())
                DownedNPCSystem.downedCnidrion = true;

            if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type || npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
                DownedNPCSystem.downedEvil2Summon = true;

            if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt"), 0);
            
            if (npc.type == calamity.Find<ModNPC>("GiantClam").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ClamHunt"), 0);

            if (npc.type == calamity.Find<ModNPC>("SlimeGodCore").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "SlimeGodHunt"), 0);

            if (npc.type == calamity.Find<ModNPC>("AquaticScourgeHead").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2"), 0);

            if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
                DownedNPCSystem.downedSirenLure = true;
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
                npc.Transform(ModContent.NPCType<WFCnidrion>());
        }
    }
}
