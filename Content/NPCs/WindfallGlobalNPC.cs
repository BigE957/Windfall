using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Enemies;
using Windfall.Content.Projectiles.NPCAnimations;
using Terraria.ID;

namespace Windfall.Content.NPCs
{
    public class WindfallGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            Mod windfall = Windfall.Instance;
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type || npc.type == ModContent.NPCType<WFCnidrion>())
                DownedNPCSystem.downedCnidrion = true;

            if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type || npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
                DownedNPCSystem.downedEvil2Summon = true;

            if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ScoogHunt"), 0);
            
            if (npc.type == calamity.Find<ModNPC>("GiantClam").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ClamHunt"), 0);

            if ((npc.type == calamity.Find<ModNPC>("HiveMind").Type && DownedBossSystem.downedHiveMind) || (npc.type == calamity.Find<ModNPC>("PerforatorHive").Type && DownedBossSystem.downedPerforator))
                SpawnWorldEventProjectile(ModContent.ProjectileType<StatisProj>());

            if (npc.type == calamity.Find<ModNPC>("SlimeGodCore").Type)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "SlimeGodHuntth"), 0);

            if (npc.type == NPCID.QueenSlimeBoss)
                QuestSystem.IncrementQuestProgress(QuestSystem.QuestLog.FindIndex(quest => quest.Name == "QueenSlimeHunt"), 0);

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
        internal  static void SpawnWorldEventProjectile(int type)
        {
            Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), new Vector2(Main.player[0].Center.X + 100, Main.player[0].Center.Y), Vector2.Zero, type, 0, 0);
        }
    }
}
