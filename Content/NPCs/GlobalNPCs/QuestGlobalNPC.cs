using Windfall.Common.Systems;
using Windfall.Content.NPCs.Enemies;
using Windfall.Content.Projectiles.NPCAnimations;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.NPCs.GlobalNPCs
{
    public class QuestGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            Mod windfall = Windfall.Instance;

            if ((npc.type == NPCID.EaterofSouls || npc.type == NPCID.Crimera) && QuestSystem.QuestLog[QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl")].Active)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "PestControl"));

            if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt"));

            if (npc.type == calamity.Find<ModNPC>("GiantClam").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ClamHunt"));

            if (npc.type == calamity.Find<ModNPC>("SlimeGodCore").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "SlimeGodHunt"));

            if (npc.type == NPCID.QueenSlimeBoss)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "QueenSlimeHunt"));

            if (npc.type == calamity.Find<ModNPC>("AquaticScourgeHead").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2"));
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
                npc.Transform(ModContent.NPCType<WFCnidrion>());
            if (npc.type == NPCID.CultistArcherBlue || npc.type == NPCID.CultistDevote)
                npc.active = false;
        }
        private static void SpawnWorldEventProjectile(int type, int xOffSet)
        {
            Projectile.NewProjectileDirect(Entity.GetSource_NaturalSpawn(), new Vector2(Main.player[0].Center.X + xOffSet, Main.player[0].Center.Y), Vector2.Zero, type, 0, 0);
        }
    }
}
