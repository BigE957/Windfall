using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Windfall.NPCs.Enemies;
using Windfall.Systems;

namespace Windfall.NPCs
{
    public class WindfallGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type || npc.type == ModContent.NPCType<WFCnidrion>())
                DownedNPCSystem.downedCnidrion = true;
            if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
                DownedNPCSystem.downedSirenLure = true;
            if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type || npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
                DownedNPCSystem.downedEvil2Summon = true;
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
                npc.Transform(ModContent.NPCType<WFCnidrion>());
        }
    }
}
